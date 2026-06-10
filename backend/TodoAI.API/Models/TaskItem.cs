
public class TaskItem
{
    public int Id { get; set; }
    public string Task { get; set; }
    public string Priority { get; set; }
    public string Category { get; set; }
    public bool Is_Completed { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime? Due_Date { get; set; }
    public string? UserID { get; set; }

}
