using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MyWay.Models;
using MyWay.Services;

namespace MyWay.ViewModels
{
    public class TasksViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        public event Action<int>? PointsEarned;

        // ── Collections ────────────────────────────────────────────────────
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        private ICollectionView _tasksView = null!;
        public ICollectionView TasksView => _tasksView;

        // ── Form fields ────────────────────────────────────────────────────
        private string _newTitle = "";
        public string NewTitle
        {
            get => _newTitle;
            set { _newTitle = value; OnPropertyChanged(); OnPropertyChanged(nameof(TitleError)); }
        }

        private int _newDifficulty = 1;
        public int NewDifficulty
        {
            get => _newDifficulty;
            set { _newDifficulty = value; OnPropertyChanged(); }
        }

        private DateTime _newDueDate = DateTime.Today;
        public DateTime NewDueDate
        {
            get => _newDueDate;
            set { _newDueDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(DueDateError)); }
        }

        // ── Kalendarz i filtry ─────────────────────────────────────────────
        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                NewDueDate = value; // Automatycznie przestawia datę dodawania nowego zadania!
                _tasksView.Refresh();
            }
        }

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(); _tasksView.Refresh(); }
        }

        private string _filterStatus = "Wszystkie";
        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(); _tasksView.Refresh(); }
        }

        private string _sortBy = "Data";
        public string SortBy
        {
            get => _sortBy;
            set { _sortBy = value; OnPropertyChanged(); ApplySort(); }
        }

        // ── Editing ────────────────────────────────────────────────────────
        private TaskItem? _editingTask;
        public TaskItem? EditingTask
        {
            get => _editingTask;
            set { _editingTask = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsEditing)); }
        }
        public bool IsEditing => EditingTask != null;

        // ── Focus/Timer ────────────────────────────────────────────────────
        private TaskItem? _focusTask;
        public TaskItem? FocusTask
        {
            get => _focusTask;
            set { _focusTask = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasFocusTask)); }
        }
        public bool HasFocusTask => FocusTask != null;

        private bool _timerRunning;
        public bool TimerRunning
        {
            get => _timerRunning;
            set { _timerRunning = value; OnPropertyChanged(); }
        }

        private string _timerDisplay = "00:00";
        public string TimerDisplay
        {
            get => _timerDisplay;
            set { _timerDisplay = value; OnPropertyChanged(); }
        }

        private CancellationTokenSource? _timerCts;
        private int _timerSeconds;

        // ── Validation ─────────────────────────────────────────────────────
        public string TitleError => string.IsNullOrWhiteSpace(NewTitle) && _touched
            ? "Tytuł nie może być pusty" : "";
        public string DueDateError => NewDueDate < DateTime.Today && _touched
            ? "Data nie może być w przeszłości" : "";
        private bool _touched;

        // ── Stats ──────────────────────────────────────────────────────────
        private int _totalPoints;
        public int TotalPoints
        {
            get => _totalPoints;
            set { _totalPoints = value; OnPropertyChanged(); }
        }

        private int _todayCompleted;
        public int TodayCompleted
        {
            get => _todayCompleted;
            set { _todayCompleted = value; OnPropertyChanged(); }
        }

        private int _overdueCount;
        public int OverdueCount
        {
            get => _overdueCount;
            set { _overdueCount = value; OnPropertyChanged(); }
        }

        // ── Commands ────────────────────────────────────────────────────────
        public ICommand AddTaskCommand { get; }
        public ICommand AddCompletedTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleCompleteCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand StartFocusCommand { get; }
        public ICommand StopFocusCommand { get; }
        public ICommand ToggleTimerCommand { get; }
        public ICommand SaveTimerCommand { get; }

        public TasksViewModel(DatabaseService db)
        {
            _db = db;

            AddTaskCommand = new AsyncRelayCommand(AddTaskAsync);
            AddCompletedTaskCommand = new AsyncRelayCommand(AddCompletedTaskAsync);
            DeleteTaskCommand = new AsyncRelayCommand(async p => await DeleteTaskAsync(p as TaskItem));
            ToggleCompleteCommand = new AsyncRelayCommand(async p => await ToggleCompleteAsync(p as TaskItem));
            StartEditCommand = new RelayCommand(p => StartEdit(p as TaskItem));
            SaveEditCommand = new AsyncRelayCommand(SaveEditAsync);
            CancelEditCommand = new RelayCommand(() => { EditingTask = null; });
            StartFocusCommand = new RelayCommand(p => { FocusTask = p as TaskItem; });
            StopFocusCommand = new RelayCommand(StopFocus);
            ToggleTimerCommand = new RelayCommand(ToggleTimer);
            SaveTimerCommand = new AsyncRelayCommand(SaveTimerAsync);

            _tasksView = CollectionViewSource.GetDefaultView(Tasks);
            _tasksView.Filter = FilterTask;

            _ = LoadTasksAsync();
        }

        public async Task LoadTasksAsync()
        {
            var items = await _db.GetTasksAsync();
            Tasks.Clear();
            foreach (var t in items) Tasks.Add(t);
            RefreshStats();
        }

        private void RefreshStats()
        {
            TotalPoints = Tasks.Where(t => t.IsCompleted).Sum(t => t.Points);
            TodayCompleted = Tasks.Count(t => t.IsCompleted && t.DueDate.Date == DateTime.Today);
            OverdueCount = Tasks.Count(t => t.IsOverdue);
        }

        private async Task AddTaskAsync()
        {
            _touched = true;
            OnPropertyChanged(nameof(TitleError));
            OnPropertyChanged(nameof(DueDateError));

            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            if (NewDueDate < DateTime.Today) return;

            if (Tasks.Any(t => t.Title.Equals(NewTitle.Trim(), StringComparison.OrdinalIgnoreCase)
                              && !t.IsCompleted))
            {
                MessageBox.Show("Zadanie o tej nazwie już istnieje na liście.",
                    "Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new TaskItem
            {
                Title = NewTitle.Trim(),
                Difficulty = NewDifficulty,
                DueDate = NewDueDate
            };

            task.Id = await _db.AddTaskAsync(task);
            Tasks.Add(task);

            NewTitle = "";
            NewDifficulty = 1;
            NewDueDate = SelectedDate; // Wraca do aktualnie wybranego dnia w kalendarzu
            _touched = false;
            OnPropertyChanged(nameof(TitleError));
            OnPropertyChanged(nameof(DueDateError));
            RefreshStats();
        }

        private async Task AddCompletedTaskAsync()
        {
            _touched = true;
            OnPropertyChanged(nameof(TitleError));
            OnPropertyChanged(nameof(DueDateError));

            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            if (NewDueDate < DateTime.Today) return;

            if (Tasks.Any(t => t.Title.Equals(NewTitle.Trim(), StringComparison.OrdinalIgnoreCase)
                              && !t.IsCompleted))
            {
                MessageBox.Show("Zadanie o tej nazwie już istnieje na liście.",
                    "Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new TaskItem
            {
                Title = NewTitle.Trim(),
                Difficulty = NewDifficulty,
                DueDate = NewDueDate,
                IsCompleted = true
            };

            task.Id = await _db.AddTaskAsync(task);
            Tasks.Add(task);

            PointsEarned?.Invoke(task.Points);

            NewTitle = "";
            NewDifficulty = 1;
            NewDueDate = SelectedDate; // Wraca do aktualnie wybranego dnia w kalendarzu
            _touched = false;
            OnPropertyChanged(nameof(TitleError));
            OnPropertyChanged(nameof(DueDateError));
            RefreshStats();
        }

        private async Task DeleteTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            var r = MessageBox.Show($"Usunąć zadanie \"{task.Title}\"?",
                "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;
            await _db.DeleteTaskAsync(task.Id);
            Tasks.Remove(task);
            RefreshStats();
        }

        private async Task ToggleCompleteAsync(TaskItem? task)
        {
            if (task == null) return;
            task.IsCompleted = !task.IsCompleted;
            task.UpdateOverdue();
            await _db.UpdateTaskAsync(task);
            if (task.IsCompleted) PointsEarned?.Invoke(task.Points);
            RefreshStats();
            _tasksView.Refresh();
        }

        private void StartEdit(TaskItem? task)
        {
            if (task == null) return;
            EditingTask = new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Difficulty = task.Difficulty,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                TimeSpentSeconds = task.TimeSpentSeconds
            };
        }

        private async Task SaveEditAsync()
        {
            if (EditingTask == null) return;
            if (string.IsNullOrWhiteSpace(EditingTask.Title))
            {
                MessageBox.Show("Tytuł nie może być pusty.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditingTask.Difficulty < 1 || EditingTask.Difficulty > 3)
            {
                MessageBox.Show("Trudność musi być wartością 1, 2 lub 3.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _db.UpdateTaskAsync(EditingTask);
            var existing = Tasks.FirstOrDefault(t => t.Id == EditingTask.Id);
            if (existing != null)
            {
                existing.Title = EditingTask.Title;
                existing.Difficulty = EditingTask.Difficulty;
                existing.DueDate = EditingTask.DueDate;
                existing.UpdateOverdue();
            }
            EditingTask = null;
            RefreshStats();
            _tasksView.Refresh();
        }

        private void StopFocus()
        {
            if (TimerRunning) ToggleTimer();
            FocusTask = null;
            _timerSeconds = 0;
            TimerDisplay = "00:00";
        }

        private void ToggleTimer()
        {
            if (TimerRunning)
            {
                _timerCts?.Cancel();
                TimerRunning = false;
            }
            else
            {
                TimerRunning = true;
                _timerCts = new CancellationTokenSource();
                _ = RunTimerAsync(_timerCts.Token);
            }
        }

        private async Task RunTimerAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(1000, ct).ContinueWith(_ => { });
                if (ct.IsCancellationRequested) break;
                _timerSeconds++;
                var ts = TimeSpan.FromSeconds(_timerSeconds);
                TimerDisplay = ts.Hours > 0
                    ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                    : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }

        private async Task SaveTimerAsync()
        {
            if (FocusTask == null) return;
            if (TimerRunning) ToggleTimer();
            FocusTask.TimeSpentSeconds += _timerSeconds;
            await _db.UpdateTaskAsync(FocusTask);
            MessageBox.Show($"Zapisano {TimeSpan.FromSeconds(_timerSeconds):mm\\:ss} do zadania \"{FocusTask.Title}\".",
                "Czas zapisany", MessageBoxButton.OK, MessageBoxImage.Information);
            _timerSeconds = 0;
            TimerDisplay = "00:00";
        }

        private bool FilterTask(object obj)
        {
            if (obj is not TaskItem t) return false;

            var matchText = string.IsNullOrWhiteSpace(FilterText) ||
                t.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase);

            var matchStatus = FilterStatus switch
            {
                "Aktywne" => !t.IsCompleted && !t.IsOverdue,
                "Zaległe" => t.IsOverdue,
                "Ukończone" => t.IsCompleted,
                _ => true
            };

            // LOGIKA KALENDARZA
            // Pokazujemy zadanie, jeśli jego data to dokładnie wybrany dzień w kalendarzu
            // LUB jeśli zadanie jest nieukończone (zaległe) i jego oryginalna data jest w przeszłości względem wybranego dnia
            bool matchDate = t.DueDate.Date == SelectedDate.Date ||
                             (!t.IsCompleted && t.DueDate.Date < SelectedDate.Date);

            return matchText && matchStatus && matchDate;
        }

        private void ApplySort()
        {
            _tasksView.SortDescriptions.Clear();
            switch (SortBy)
            {
                case "Data":
                    _tasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.DueDate), ListSortDirection.Ascending));
                    break;
                case "Trudność":
                    _tasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.Difficulty), ListSortDirection.Descending));
                    break;
                case "Nazwa":
                    _tasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.Title), ListSortDirection.Ascending));
                    break;
                case "Ukończone":
                    _tasksView.SortDescriptions.Add(new SortDescription(nameof(TaskItem.IsCompleted), ListSortDirection.Ascending));
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}