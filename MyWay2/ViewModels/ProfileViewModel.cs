using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyWay.Models;
using MyWay.Services;

namespace MyWay.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;

        // ── Profil ─────────────────────────────────────────────────────────
        private UserProfile _profile = new();
        public UserProfile Profile
        {
            get => _profile;
            set { _profile = value; OnPropertyChanged(); OnPropertyChanged(nameof(WelcomeText)); }
        }

        public string WelcomeText => $"{Profile.AvatarEmoji}  {Profile.DisplayName}";

        // ── Dostępne awatary ───────────────────────────────────────────────
        public List<string> AvailableAvatars { get; } = new()
        {
            "🧑", "👩", "👨", "🧔", "👱", "🧑‍💻", "👩‍💻", "🦸", "🧙", "🎯",
            "🚀", "⭐", "🔥", "🌟", "💪", "🎮", "🎨", "📚", "🌈", "🐉"
        };

        // ── Edycja ─────────────────────────────────────────────────────────
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); }
        }
        public bool IsNotEditing => !IsEditing;

        private string _editName = "";
        public string EditName
        {
            get => _editName;
            set { _editName = value; OnPropertyChanged(); }
        }

        private string _editAvatar = "🧑";
        public string EditAvatar
        {
            get => _editAvatar;
            set { _editAvatar = value; OnPropertyChanged(); }
        }

        private int _editGoal = 10;
        public int EditGoal
        {
            get => _editGoal;
            set { _editGoal = value; OnPropertyChanged(); }
        }

        // ── Statystyki całościowe ──────────────────────────────────────────
        private int _totalPoints;
        public int TotalPoints
        {
            get => _totalPoints;
            set { _totalPoints = value; OnPropertyChanged(); }
        }

        private int _totalTasksDone;
        public int TotalTasksDone
        {
            get => _totalTasksDone;
            set { _totalTasksDone = value; OnPropertyChanged(); }
        }

        private double _avgMoodAllTime;
        public double AvgMoodAllTime
        {
            get => _avgMoodAllTime;
            set { _avgMoodAllTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(AvgMoodLabel)); }
        }
        public string AvgMoodLabel => AvgMoodAllTime > 0 ? $"{AvgMoodAllTime:F1} / 5" : "Brak danych";

        private int _activeDays;
        public int ActiveDays
        {
            get => _activeDays;
            set { _activeDays = value; OnPropertyChanged(); }
        }

        // Pasek postępu dziennego celu (0.0 – 1.0)
        private double _dailyProgress;
        public double DailyProgress
        {
            get => _dailyProgress;
            set { _dailyProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(DailyProgressLabel)); }
        }
        public string DailyProgressLabel =>
            $"{Math.Min((int)(DailyProgress * Profile.DailyPointsGoal), Profile.DailyPointsGoal)} / {Profile.DailyPointsGoal} pkt";

        // ── Komendy ────────────────────────────────────────────────────────
        public ICommand StartEditCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SelectAvatarCommand { get; }

        public ProfileViewModel(DatabaseService db)
        {
            _db = db;

            StartEditCommand   = new RelayCommand(_ => BeginEdit());
            CancelEditCommand  = new RelayCommand(_ => CancelEdit());
            SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
            SelectAvatarCommand = new RelayCommand(p => { if (p is string e) EditAvatar = e; });

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            Profile = await _db.GetOrCreateProfileAsync();
            await LoadStatsAsync();
        }

        private void BeginEdit()
        {
            EditName   = Profile.DisplayName;
            EditAvatar = Profile.AvatarEmoji;
            EditGoal   = Profile.DailyPointsGoal;
            IsEditing  = true;
        }

        private void CancelEdit() => IsEditing = false;

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName))
            {
                MessageBox.Show("Nazwa nie może być pusta.", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EditGoal < 1 || EditGoal > 9999)
            {
                MessageBox.Show("Cel dzienny musi być między 1 a 9999 punktów.", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Profile.DisplayName     = EditName.Trim();
            Profile.AvatarEmoji     = EditAvatar;
            Profile.DailyPointsGoal = EditGoal;

            await _db.UpdateProfileAsync(Profile);

            OnPropertyChanged(nameof(Profile));
            OnPropertyChanged(nameof(WelcomeText));
            IsEditing = false;
        }

        public async Task RefreshStatsAsync() => await LoadStatsAsync();

        private async Task LoadStatsAsync()
        {
            var records = await _db.GetRecordsForPeriodAsync(DateTime.MinValue, DateTime.Today);
            TotalPoints  = records.Sum(r => r.TotalPoints);
            ActiveDays   = records.Count;

            var moodRecords = records.Where(r => r.MoodScore > 0).ToList();
            AvgMoodAllTime  = moodRecords.Any() ? moodRecords.Average(r => r.MoodScore) : 0;

            var today = records.FirstOrDefault(r => r.Date.Date == DateTime.Today);
            var todayPts = today?.TotalPoints ?? 0;
            DailyProgress = Profile.DailyPointsGoal > 0
                ? Math.Min((double)todayPts / Profile.DailyPointsGoal, 1.0)
                : 0;

            var tasks = await _db.GetTasksAsync();
            TotalTasksDone = tasks.Count(t => t.IsCompleted);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
