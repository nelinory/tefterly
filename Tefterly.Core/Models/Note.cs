using System;

namespace Tefterly.Core.Models
{
    public class Note
    {
        public Guid Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }

        // note attributes
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public Guid Category { get; set; }

        // note state
        public bool IsStarred { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
    }
}