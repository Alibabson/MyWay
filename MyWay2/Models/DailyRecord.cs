using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWay.Models
{
    public partial class DailyRecord : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private DateTime _date = DateTime.Today;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MoodLabel))]
        [NotifyPropertyChangedFor(nameof(MoodEmoji))]
        private int _moodScore = 0; // 0 means not set, 1-5 valid
        [ObservableProperty] private int _extraPoints;
        [ObservableProperty] private string _quoteOfTheDay = string.Empty;
        [ObservableProperty] private int _taskPoints;

        public int TotalPoints => TaskPoints + ExtraPoints;

        public string MoodLabel => MoodScore switch
        {
            0 => "Wybierz nastrój",
            1 => "😞 Fatalnie",
            2 => "😕 Słabo",
            3 => "😐 Średnio",
            4 => "😊 Dobrze",
            5 => "😄 Świetnie",
            _ => "😐"
        };

        public string MoodEmoji => MoodScore switch
        {
            0 => "❓",
            1 => "😞",
            2 => "😕",
            3 => "😐",
            4 => "😊",
            5 => "😄",
            _ => "😐"
        };
    }
}
