using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using MYWAY.Models;

namespace MYWAY.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<Habit> Habits { get; set; } = new ObservableCollection<Habit>();
        public ObservableCollection<ExtraActivity> ExtraActivities { get; set; } = new ObservableCollection<ExtraActivity>();
        
        private DailyRecord _todayRecord = new DailyRecord();
        public DailyRecord TodayRecord
        {
            get => _todayRecord;
            set => SetProperty(ref _todayRecord, value);
        }

        private int _selectedViewIndex = 0;
        public int SelectedViewIndex
        {
            get => _selectedViewIndex;
            set => SetProperty(ref _selectedViewIndex, value);
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdatePointsAndState();
                }
            }
        }

        public IEnumerable<TaskItem> FilteredTasks => Tasks.Where(t => t.DueDate.Date == SelectedDate.Date).OrderBy(t => t.IsCompleted).ToList();
        public IEnumerable<ExtraActivity> FilteredExtraActivities => ExtraActivities.Where(e => e.Date.Date == SelectedDate.Date).ToList();

        // Add Task Form
        private string _newTaskTitle = string.Empty;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        private int _newTaskDifficulty = 1;
        public int NewTaskDifficulty
        {
            get => _newTaskDifficulty;
            set => SetProperty(ref _newTaskDifficulty, value);
        }

        // Add Extra Activity Form
        private string _newExtraTitle = string.Empty;
        public string NewExtraTitle
        {
            get => _newExtraTitle;
            set => SetProperty(ref _newExtraTitle, value);
        }
        
        private int _newExtraPoints = 3;
        public int NewExtraPoints
        {
            get => _newExtraPoints;
            set => SetProperty(ref _newExtraPoints, value);
        }

        private string _newHabitTitle = string.Empty;
        public string NewHabitTitle
        {
            get => _newHabitTitle;
            set => SetProperty(ref _newHabitTitle, value);
        }

        // Commands
        public ICommand AddTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand CompleteHabitCommand { get; }
        public ICommand AddHabitCommand { get; }
        public ICommand AddExtraActivityCommand { get; }
        public ICommand StartFocusTimerCommand { get; }
        public ICommand ChangeViewCommand { get; }

        public MainViewModel()
        {
            LoadStaticData();

            AddTaskCommand = new RelayCommand(ExecuteAddTask, CanExecuteAddTask);
            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask);
            CompleteHabitCommand = new RelayCommand(ExecuteCompleteHabit);
            AddHabitCommand = new RelayCommand(ExecuteAddHabit, CanExecuteAddHabit);
            AddExtraActivityCommand = new RelayCommand(ExecuteAddExtraActivity, CanExecuteAddExtraActivity);
            StartFocusTimerCommand = new RelayCommand(ExecuteStartFocusTimer);
            ChangeViewCommand = new RelayCommand(ExecuteChangeView);
        }

        private void LoadStaticData()
        {
            TodayRecord = new DailyRecord
            {
                Id = 1,
                Date = DateTime.Today,
                MoodScore = 3,
                ExtraPoints = 0, // Legacy
                QuoteOfTheDay = "\"Nigdy nie rezygnuj z celu tylko dlatego, że osiągnięcie go wymaga czasu. Czas i tak upłynie.\" – H. Jackson Brown Jr."
            };
            
            // Usunięto przykładowe dane początkowe zgodnie z prośbą
        }

        // Points logic correctly filters by SelectedDate now
        public int TotalPoints => Tasks.Where(t => t.IsCompleted && t.DueDate.Date == SelectedDate.Date).Sum(t => t.Difficulty) 
                                + ExtraActivities.Where(e => e.Date.Date == SelectedDate.Date).Sum(e => e.Points);
        
        public int CompletedTasksCount => Tasks.Count(t => t.IsCompleted && t.DueDate.Date == SelectedDate.Date);

        private void UpdatePointsAndState()
        {
            OnPropertyChanged(nameof(TotalPoints));
            OnPropertyChanged(nameof(CompletedTasksCount));
            OnPropertyChanged(nameof(Tasks)); 
            OnPropertyChanged(nameof(FilteredTasks)); 
            OnPropertyChanged(nameof(ExtraActivities));
            OnPropertyChanged(nameof(FilteredExtraActivities));
        }

        private void ExecuteChangeView(object? obj)
        {
            if (obj is string indexStr && int.TryParse(indexStr, out int index))
            {
                SelectedViewIndex = index;
            }
        }

        private bool CanExecuteAddTask(object? obj) => !string.IsNullOrWhiteSpace(NewTaskTitle);

        private void ExecuteAddTask(object? obj)
        {
            Tasks.Add(new TaskItem
            {
                Id = Tasks.Count > 0 ? Tasks.Max(t => t.Id) + 1 : 1,
                Title = NewTaskTitle,
                Difficulty = NewTaskDifficulty,
                DueDate = SelectedDate, // Add task to the currently selected date
                IsCompleted = false
            });
            NewTaskTitle = string.Empty;
            NewTaskDifficulty = 1;
            UpdatePointsAndState();
        }

        private void ExecuteCompleteTask(object? obj)
        {
            if (obj is TaskItem task)
            {
                task.IsCompleted = !task.IsCompleted;
                UpdatePointsAndState();
            }
        }

        private void ExecuteDeleteTask(object? obj)
        {
            if (obj is TaskItem task)
            {
                Tasks.Remove(task);
                UpdatePointsAndState();
            }
        }

        private void ExecuteCompleteHabit(object? obj)
        {
            if (obj is Habit habit)
            {
                if (!habit.IsCompletedToday)
                {
                    habit.LastCompletedDate = DateTime.Today;
                    habit.CurrentStreak++;
                    if (habit.CurrentStreak > habit.BestStreak)
                        habit.BestStreak = habit.CurrentStreak;
                }
                var index = Habits.IndexOf(habit);
                Habits[index] = new Habit { Id = habit.Id, Title = habit.Title, CurrentStreak = habit.CurrentStreak, BestStreak = habit.BestStreak, LastCompletedDate = habit.LastCompletedDate };
            }
        }

        private bool CanExecuteAddHabit(object? obj) => !string.IsNullOrWhiteSpace(NewHabitTitle);

        private void ExecuteAddHabit(object? obj)
        {
            Habits.Add(new Habit
            {
                Id = Habits.Count > 0 ? Habits.Max(h => h.Id) + 1 : 1,
                Title = NewHabitTitle,
                CurrentStreak = 0,
                BestStreak = 0,
                LastCompletedDate = null
            });
            NewHabitTitle = string.Empty;
        }

        private bool CanExecuteAddExtraActivity(object? obj) => !string.IsNullOrWhiteSpace(NewExtraTitle);

        private void ExecuteAddExtraActivity(object? obj)
        {
            ExtraActivities.Add(new ExtraActivity
            {
                Id = ExtraActivities.Count > 0 ? ExtraActivities.Max(e => e.Id) + 1 : 1,
                Title = NewExtraTitle,
                Points = NewExtraPoints,
                Date = SelectedDate // Bound to selected date
            });
            NewExtraTitle = string.Empty;
            NewExtraPoints = 3;
            UpdatePointsAndState();
        }

        private void ExecuteStartFocusTimer(object? obj)
        {
            if (obj is TaskItem task)
            {
                MessageBox.Show($"Tryb skupienia aktywowany dla: {task.Title}. Powodzenia!", "Focus");
            }
        }
    }
}
