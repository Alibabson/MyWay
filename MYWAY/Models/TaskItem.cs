using System;

namespace MYWAY.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 1; // 1 - Easy, 2 - Medium, 3 - Hard
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int TimeSpent { get; set; } // in seconds

        public bool IsOverdue => !IsCompleted && DueDate.Date < DateTime.Today;
        
        public string DifficultyText => Difficulty switch
        {
            1 => "Łatwe (1 pkt)",
            2 => "Średnie (2 pkt)",
            3 => "Trudne (3 pkt)",
            _ => "Brak"
        };
    }
}
