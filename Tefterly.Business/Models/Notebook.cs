using System;

namespace Tefterly.Business.Models
{
    public class Notebook
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string IconFont { get; set; }
        public int TotalItems { get; set; }
        public bool IsSystem { get; set; }
    }
}