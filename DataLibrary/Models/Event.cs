namespace DataLibrary.Models
{
    public class Event
    {
        public Guid EventId { get; set; }
        public string Subject { get; set; } = "";
        public string? Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Description { get; set; }
        public bool IsAllDay { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? RecurrenceException { get; set; }
        public int? RecurrenceID { get; set; }
        public Guid UserId { get; set; }
        public bool IsGlobal { get; set; }
        public Guid? FuneralId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}