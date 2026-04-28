using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWay.Models
{
    public partial class Habit : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private int _currentStreak;
        [ObservableProperty] private int _bestStreak;
        [ObservableProperty] private DateTime? _lastCompletedDate;
        [ObservableProperty] private bool _isCompletedToday;

        public bool IsStreakBroken =>
            LastCompletedDate.HasValue &&
            LastCompletedDate.Value.Date < DateTime.Today.AddDays(-1);

        public string StreakLabel => CurrentStreak switch
        {
            0 => "Brak serii",
            1 => "1 dzień 🔥",
            _ => $"{CurrentStreak} dni 🔥"
        };

        public void CheckAndResetStreak()
        {
            if (IsStreakBroken)
                CurrentStreak = 0;

            IsCompletedToday = LastCompletedDate.HasValue &&
                               LastCompletedDate.Value.Date == DateTime.Today;
        }
    }
}
