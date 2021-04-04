using System;
using Tefterly.Core;

namespace Tefterly.Business.Models
{
    public class Note
    {
        public Guid Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public Guid NotebookCategory { get; set; }

        // state
        public bool IsStarred
        {
            get { return NotebookCategory == NotebookCategories.Starred; }
        }
        public bool IsReadOnly
        {
            get { return NotebookCategory == NotebookCategories.Archived; }
        }
    }
}