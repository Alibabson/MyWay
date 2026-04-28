using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyWay.Services;

namespace MyWay.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TasksViewModel Tasks { get; }
        public DashboardViewModel Dashboard { get; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            var db = new DatabaseService();
            var quoteService = new QuoteService();
            var pdfService = new PdfExportService();

            Tasks = new TasksViewModel(db);
            Dashboard = new DashboardViewModel(db, quoteService, pdfService);

            // Wire up: when task is completed, add points to today's record
            Tasks.PointsEarned += async pts => await Dashboard.AddTaskPointsAsync(pts);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
