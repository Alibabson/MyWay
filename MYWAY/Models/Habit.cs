using System;

namespace MYWAY.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public DateTime? LastCompletedDate { get; set; }

        public bool IsCompletedToday => LastCompletedDate?.Date == DateTime.Today;
    }
}
