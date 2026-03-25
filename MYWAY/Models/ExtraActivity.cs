using System;

namespace MYWAY.Models
{
    public class ExtraActivity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Points { get; set; } = 3; // Default
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
