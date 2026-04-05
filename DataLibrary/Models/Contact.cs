using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary.Models
{
    public class Contact
    {
        public Guid ContactId { get; set; } = Guid.NewGuid();
        public string ContactName { get; set; } = string.Empty;
        public string ContactType { get; set; } = "Uncategorized";
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Location { get; set; }
        public string? VendorType { get; set; }
        public Guid UserId { get; set; }
        public bool IsGlobal { get; set; }
        public Guid? FuneralId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
