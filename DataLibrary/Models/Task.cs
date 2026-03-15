namespace DataLibrary.Models
{
    public class NsTask
    {
        public Guid TaskId { get; set; }
        public string TaskNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid FuneralId { get; set;}

        private static int TaskCounter = 0;

        public static string AssignNumber()
        {
            TaskCounter++;
            return $"Task-{TaskCounter}";
        }
    }

}
