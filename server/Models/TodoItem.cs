namespace Server.Models
{
    public class TodoItem
    {
        public int Id {get; set;}
        public DateTime Date { get; set; }
        public string? Category { get; set; }
        public bool RemindEnabled { get; set; }
        public string? Description { get; set; }
    }
}    