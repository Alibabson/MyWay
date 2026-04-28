using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWay.Models
{
    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private int _difficulty = 1; // 1=Łatwe, 2=Średnie, 3=Trudne
        [ObservableProperty] private DateTime _dueDate = DateTime.Today;
        [ObservableProperty] private bool _isCompleted;
        [ObservableProperty] private int _timeSpentSeconds;
        [ObservableProperty] private bool _isOverdue;

        public int Points => Difficulty;

        public string DifficultyLabel => Difficulty switch
        {
            1 => "Łatwe",
            2 => "Średnie",
            3 => "Trudne",
            _ => "?"
        };

        public string DifficultyIcon => Difficulty switch
        {
            1 => "⭐",
            2 => "⭐⭐",
            3 => "⭐⭐⭐",
            _ => ""
        };

        public string TimeSpentLabel
        {
            get
            {
                var ts = TimeSpan.FromSeconds(TimeSpentSeconds);
                return ts.Hours > 0
                    ? $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s"
                    : $"{ts.Minutes}m {ts.Seconds}s";
            }
        }

        public void UpdateOverdue()
        {
            IsOverdue = !IsCompleted && DueDate.Date < DateTime.Today;
        }
    }
}
