namespace DataLibrary.Models
{
    public class Note
    {
        public Guid NoteId { get; set; } = Guid.NewGuid();
        public string NoteText { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public Guid ObjectId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
