namespace DataLibrary.Models
{
    public class NsTask
    {
        public Guid TaskId { get; set; } = Guid.NewGuid();
        public string TaskNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? CompletionDate { get; set; }
        public Guid FuneralId { get; set; }
    }
}
