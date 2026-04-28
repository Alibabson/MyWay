using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWay.Models
{
    public partial class DailyRecord : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private DateTime _date = DateTime.Today;
        [ObservableProperty] private int _moodScore = 3; // 1-5
        [ObservableProperty] private int _extraPoints;
        [ObservableProperty] private string _quoteOfTheDay = string.Empty;
        [ObservableProperty] private int _taskPoints;

        public int TotalPoints => TaskPoints + ExtraPoints;

        public string MoodLabel => MoodScore switch
        {
            1 => "😞 Fatalnie",
            2 => "😕 Słabo",
            3 => "😐 Średnio",
            4 => "😊 Dobrze",
            5 => "😄 Świetnie",
            _ => "😐"
        };

        public string MoodEmoji => MoodScore switch
        {
            1 => "😞",
            2 => "😕",
            3 => "😐",
            4 => "😊",
            5 => "😄",
            _ => "😐"
        };
    }
}
