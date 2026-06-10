using Npgsql;
using Microsoft.AspNetCore.Mvc;
using System.Data;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly OpenAIService _service;
    private readonly IConfiguration _config;

    public TaskController(OpenAIService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }
    [HttpGet("{userID}")]
    public async Task<ActionResult<List<TaskItem>>> Get([FromRoute] string userID)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        var tasks = new List<TaskItem>();
        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT * FROM tasks where user_id = @userID", conn);
        cmd.Parameters.AddWithValue("userID", userID);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(new TaskItem
            {
                Id = (int)reader.GetInt64("id"),
                Task = reader.GetString("task"),
                Priority = reader.GetString("priority"),
                Category = reader.GetString("category")
            });
        }
        await conn.CloseAsync();
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<List<TaskItem>>> Process([FromBody] TaskRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Input))
            return BadRequest("Input is required");
        var result = await _service.ProcessTasks(request.Input);
        await SaveTasksToDb(result, request.UserID);
        return Ok(result);
    }

    [HttpPut("complete/{id}")]
    public async Task<IActionResult> MarkComplete(int id)
    {
        var connString = _config.GetConnectionString("DefaultConnection");

        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(
            "UPDATE tasks SET is_completed = TRUE WHERE id = @id", conn);

        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();

        await conn.CloseAsync();

        return Ok();
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItem task)
    {
        // Call AI again to re-evaluate
        var updatedTasks = await _service.ProcessTasks(task.Task);

        var updated = updatedTasks.FirstOrDefault();

        if (updated == null)
            return BadRequest("AI failed");

        var connString = _config.GetConnectionString("DefaultConnection");

        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(@"
        UPDATE tasks 
        SET task = @task,
            priority = @priority,
            category = @category
        WHERE id = @id", conn);

        cmd.Parameters.AddWithValue("task", updated.Task);
        cmd.Parameters.AddWithValue("priority", updated.Priority);
        cmd.Parameters.AddWithValue("category", updated.Category);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();

        return Ok(updated);
    }

    private async Task SaveTasksToDb(List<TaskItem> tasks, string userID)
    {
        var connString = _config.GetConnectionString("DefaultConnection");

        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        foreach (var t in tasks)
        {
            var cmd = new NpgsqlCommand(@"
            INSERT INTO tasks (task, priority, category, is_completed, created_at, user_id)
            VALUES (@task, @priority, @category, @is_completed, @created_at, @userID)", conn);

            cmd.Parameters.AddWithValue("task", t.Task);
            cmd.Parameters.AddWithValue("priority", t.Priority);
            cmd.Parameters.AddWithValue("category", t.Category);
            cmd.Parameters.AddWithValue("is_completed", false);
            cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("userID", userID);

            await cmd.ExecuteNonQueryAsync();
        }

        await conn.CloseAsync();
    }
}
public class TaskRequest
{
    public string Input { get; set; }
    public string UserID { get; set; }
}