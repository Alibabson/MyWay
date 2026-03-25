using System;

namespace MYWAY.Models
{
    public class DailyRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int MoodScore { get; set; } // 1-5
        public int ExtraPoints { get; set; }
        public string QuoteOfTheDay { get; set; } = string.Empty;
    }
}
