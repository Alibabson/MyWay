using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWay.Models
{
    public partial class UserProfile : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _displayName = "Użytkownik";
        [ObservableProperty] private string _avatarEmoji = "🧑";
        [ObservableProperty] private int _dailyPointsGoal = 10;
        [ObservableProperty] private DateTime _joinedDate = DateTime.Today;

        public string JoinedLabel => $"Dołączył(a): {JoinedDate:dd.MM.yyyy}";

        public int DaysActive => (int)(DateTime.Today - JoinedDate).TotalDays + 1;
    }
}
