using System;
using System.Collections.ObjectModel;
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
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly QuoteService _quoteService;
        private readonly PdfExportService _pdfService;

        // ── Daily Record / Mood ────────────────────────────────────────────
        private DailyRecord _todayRecord = new();
        public DailyRecord TodayRecord
        {
            get => _todayRecord;
            set { _todayRecord = value; OnPropertyChanged(); }
        }

        private string _quoteText = "";
        public string QuoteText
        {
            get => _quoteText;
            set { _quoteText = value; OnPropertyChanged(); }
        }

        // ── Habits ─────────────────────────────────────────────────────────
        public ObservableCollection<Habit> Habits { get; } = new();

        private string _newHabitTitle = "";
        public string NewHabitTitle
        {
            get => _newHabitTitle;
            set { _newHabitTitle = value; OnPropertyChanged(); OnPropertyChanged(nameof(HabitTitleError)); }
        }

        private bool _habitTouched;
        public string HabitTitleError => string.IsNullOrWhiteSpace(NewHabitTitle) && _habitTouched
            ? "Tytuł nawyku nie może być pusty" : "";

        // Właściwości wyliczane ze statystyk nawyków
        public int TotalStreakDays => Habits.Sum(h => h.CurrentStreak);
        public int BestStreak => Habits.Any() ? Habits.Max(h => h.BestStreak) : 0;
        public int CompletedToday => Habits.Count(h => h.IsCompletedToday);

        // ── Stats period ───────────────────────────────────────────────────
        private string _statsPeriod = "Tydzień";
        public string StatsPeriod
        {
            get => _statsPeriod;
            set { _statsPeriod = value; OnPropertyChanged(); _ = LoadStatsAsync(); }
        }

        public ObservableCollection<DailyRecord> PeriodRecords { get; } = new();

        private int _periodTotalPoints;
        public int PeriodTotalPoints
        {
            get => _periodTotalPoints;
            set { _periodTotalPoints = value; OnPropertyChanged(); }
        }

        private double _periodAvgMood;
        public double PeriodAvgMood
        {
            get => _periodAvgMood;
            set { _periodAvgMood = value; OnPropertyChanged(); OnPropertyChanged(nameof(PeriodAvgMoodLabel)); }
        }
        public string PeriodAvgMoodLabel => $"{PeriodAvgMood:F1}";

        private bool _isExporting;
        public bool IsExporting
        {
            get => _isExporting;
            set { _isExporting = value; OnPropertyChanged(); }
        }

        // ── Commands ───────────────────────────────────────────────────────
        public ICommand SaveMoodCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ChangePeriodCommand { get; }

        // Commands dla nawyków
        public ICommand AddHabitCommand { get; }
        public ICommand DeleteHabitCommand { get; }
        public ICommand ToggleHabitCommand { get; }

        public DashboardViewModel(DatabaseService db, QuoteService quoteService, PdfExportService pdfService)
        {
            _db = db;
            _quoteService = quoteService;
            _pdfService = pdfService;

            // Komendy z Dashboardu
            SaveMoodCommand = new AsyncRelayCommand(SaveMoodAsync);
            ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
            ChangePeriodCommand = new RelayCommand(p => { if (p is string s) StatsPeriod = s; });

            // Komendy z Nawyków
            AddHabitCommand = new AsyncRelayCommand(AddHabitAsync);
            DeleteHabitCommand = new AsyncRelayCommand(async p => await DeleteHabitAsync(p as Habit));
            ToggleHabitCommand = new AsyncRelayCommand(async p => await ToggleHabitAsync(p as Habit));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadTodayAsync();
            await LoadHabitsAsync(); // Ładowanie nawyków
            await LoadStatsAsync();
        }

        // ── Metody Dashboardu ──────────────────────────────────────────────
        public async Task LoadTodayAsync()
        {
            var record = await _db.GetTodayRecordAsync();
            if (record == null)
            {
                record = new DailyRecord { Date = DateTime.Today };
                var quote = await _quoteService.GetDailyQuoteAsync();
                record.QuoteOfTheDay = quote;
                await _db.UpsertDailyRecordAsync(record);
            }
            TodayRecord = record;
            QuoteText = record.QuoteOfTheDay;
        }

        private async Task SaveMoodAsync()
        {
            if (TodayRecord.MoodScore < 1 || TodayRecord.MoodScore > 5)
            {
                MessageBox.Show("Nastrój musi być wartością od 1 do 5.", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await _db.UpsertDailyRecordAsync(TodayRecord);
            MessageBox.Show("Nastrój zapisany! 😊", "Zapisano",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task AddTaskPointsAsync(int points)
        {
            TodayRecord.TaskPoints += points;
            await _db.UpsertDailyRecordAsync(TodayRecord);
            OnPropertyChanged(nameof(TodayRecord));
        }

        private async Task LoadStatsAsync()
        {
            var (from, to) = StatsPeriod switch
            {
                "Tydzień" => (DateTime.Today.AddDays(-6), DateTime.Today),
                "Miesiąc" => (DateTime.Today.AddDays(-29), DateTime.Today),
                _ => (DateTime.Today.AddDays(-6), DateTime.Today)
            };

            var records = await _db.GetRecordsForPeriodAsync(from, to);
            PeriodRecords.Clear();
            foreach (var r in records) PeriodRecords.Add(r);

            PeriodTotalPoints = records.Sum(r => r.TotalPoints);
            PeriodAvgMood = records.Any() ? records.Average(r => r.MoodScore) : 0;
        }

        private async Task ExportPdfAsync()
        {
            if (IsExporting) return;
            IsExporting = true;
            try
            {
                var path = await _pdfService.ExportStatsAsync(PeriodRecords.ToList(), StatsPeriod);
                MessageBox.Show($"PDF zapisany na pulpicie:\n{System.IO.Path.GetFileName(path)}",
                    "Eksport gotowy", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd eksportu: {ex.Message}", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsExporting = false; }
        }

        // ── Metody Nawyków ─────────────────────────────────────────────────
        public async Task LoadHabitsAsync()
        {
            var items = await _db.GetHabitsAsync();
            Habits.Clear();
            foreach (var h in items) Habits.Add(h);
            RefreshHabitStats();
        }

        private void RefreshHabitStats()
        {
            OnPropertyChanged(nameof(TotalStreakDays));
            OnPropertyChanged(nameof(BestStreak));
            OnPropertyChanged(nameof(CompletedToday));
        }

        private async Task AddHabitAsync()
        {
            _habitTouched = true;
            OnPropertyChanged(nameof(HabitTitleError));

            if (string.IsNullOrWhiteSpace(NewHabitTitle)) return;

            if (Habits.Any(h => h.Title.Equals(NewHabitTitle.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Nawyk o tej nazwie już istnieje.",
                    "Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var habit = new Habit { Title = NewHabitTitle.Trim() };
            habit.Id = await _db.AddHabitAsync(habit);
            Habits.Add(habit);

            NewHabitTitle = "";
            _habitTouched = false;
            OnPropertyChanged(nameof(HabitTitleError));
            RefreshHabitStats();
        }

        private async Task DeleteHabitAsync(Habit? habit)
        {
            if (habit == null) return;
            var r = MessageBox.Show($"Usunąć nawyk \"{habit.Title}\"? Stracisz cały streak!",
                "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;
            await _db.DeleteHabitAsync(habit.Id);
            Habits.Remove(habit);
            RefreshHabitStats();
        }

        private async Task ToggleHabitAsync(Habit? habit)
        {
            if (habit == null) return;

            if (habit.IsCompletedToday)
            {
                habit.IsCompletedToday = false;
                habit.CurrentStreak = Math.Max(0, habit.CurrentStreak - 1);
                habit.LastCompletedDate = habit.CurrentStreak > 0 ? DateTime.Today.AddDays(-1) : null;
            }
            else
            {
                habit.IsCompletedToday = true;
                habit.CurrentStreak++;
                if (habit.CurrentStreak > habit.BestStreak)
                    habit.BestStreak = habit.CurrentStreak;
                habit.LastCompletedDate = DateTime.Today;
            }

            await _db.UpdateHabitAsync(habit);
            RefreshHabitStats();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}