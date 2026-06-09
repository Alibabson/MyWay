using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyWay.Services;

namespace MyWay.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TasksViewModel Tasks { get; }
        public DashboardViewModel Dashboard { get; }
        public ProfileViewModel Profile { get; }   // ← NOWE

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

            Tasks     = new TasksViewModel(db);
            Dashboard = new DashboardViewModel(db, quoteService, pdfService);
            Profile   = new ProfileViewModel(db);   // ← NOWE

            // Wire up: gdy zadanie zostaje ukończone, dodaj punkty do rekordu dnia
            Tasks.PointsEarned += async pts =>
            {
                await Dashboard.AddTaskPointsAsync(pts);
                await Profile.RefreshStatsAsync();
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
