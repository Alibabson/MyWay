using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MYWAY.ViewModels;

namespace MYWAY.Views
{
    public partial class TasksView : UserControl
    {
        private MainViewModel? _viewModel;

        public TasksView()
        {
            InitializeComponent();
            DataContextChanged += TasksView_DataContextChanged;
        }

        private void TasksView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel is not null)
            {
                _viewModel.Tasks.CollectionChanged -= Tasks_CollectionChanged;
                _viewModel.ExtraActivities.CollectionChanged -= Tasks_CollectionChanged;
            }

            _viewModel = DataContext as MainViewModel;

            if (_viewModel is not null)
            {
                _viewModel.Tasks.CollectionChanged += Tasks_CollectionChanged;
                _viewModel.ExtraActivities.CollectionChanged += Tasks_CollectionChanged;
            }
        }

        private void Tasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshCalendarMarkers();
        }

        private void CalendarDayButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not CalendarDayButton button)
                return;

            UpdateDayMarker(button);
        }

        private void UpdateDayMarker(CalendarDayButton button)
        {
            if (_viewModel is null)
                return;

            DateTime? date = button.DataContext as DateTime?;
            if (date is null && button.Content is string content && int.TryParse(content, out int dayNumber))
            {
                date = CalculateButtonDate(button, dayNumber);
            }

            if (date is null)
                return;

            bool hasTasks = _viewModel.Tasks.Any(t => t.DueDate.Date == date.Value.Date)
                            || _viewModel.ExtraActivities.Any(a => a.Date.Date == date.Value.Date);
            if (button.Template.FindName("DayMarker", button) is FrameworkElement marker)
            {
                marker.Visibility = hasTasks ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private DateTime? CalculateButtonDate(CalendarDayButton button, int day)
        {
            var calendar = FindParent<Calendar>(button);
            if (calendar is null)
                return null;

            var displayDate = calendar.DisplayDate;
            var date = new DateTime(displayDate.Year, displayDate.Month, day, 0, 0, 0);

            if (button.IsInactive)
            {
                if (day > 15)
                {
                    return date.AddMonths(-1);
                }
                return date.AddMonths(1);
            }

            return date;
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? current = child;
            while (current != null)
            {
                if (current is T parent)
                {
                    return parent;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void RefreshCalendarMarkers()
        {
            foreach (var button in FindVisualChildren<CalendarDayButton>(TaskCalendar))
            {
                UpdateDayMarker(button);
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                {
                    yield return t;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
