using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.Models
{
    public class Funeral
    {
        public Guid FuneralId { get; set; }
        public string DeceasedName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? DateOfService { get; set; }
        public int NumberOfTasks { get; set; } = 0;
        public Guid UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? Obituary { get; set; }
        public bool IsArchived = false;
    }
}
