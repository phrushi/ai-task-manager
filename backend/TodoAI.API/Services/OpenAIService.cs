using System.Net.Http.Headers;
using System.Text;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenRouter:ApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<List<TaskItem>> ProcessTasks(string input)
    {
        var prompt = $@"
                    Convert the following raw tasks into structured JSON.

                    Rules:
                    - Extract individual tasks
                    - Assign priority: High / Medium / Low
                    - Add category: Work / Admin / Meeting / Personal
                    - Return ONLY valid JSON array
                    - Do NOT include any explanation

                    Input:
                    {input}
                    ";

        var requestBody = new
        {
            model = "meta-llama/llama-3-8b-instruct",
            messages = new[]
            {
            new { role = "user", content = prompt }
        }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://openrouter.ai/api/v1/chat/completions"
        );

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "AI Todo App");

        request.Content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"OpenRouter Error: {error}");
        }

        var result = await response.Content.ReadAsStringAsync();

        // Step 1: Extract content from OpenRouter response
        dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
        string content = json.choices[0].message.content;

        // Step 2: Clean content (sometimes AI adds ```json)
        content = content.Replace("```json", "").Replace("```", "").Trim();

        // Step 3: Convert to List<TaskItem>
        var tasks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TaskItem>>(content);
        tasks[0].Created_At = DateTime.Now;
        return tasks;
    }
}