using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyWay.Views
{
    public partial class MainWindow : Window
    {
        private Button? _activeNav;

        public MainWindow()
        {
            InitializeComponent();
            SetActiveNav(BtnDashboard);

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        // Obs³uga przesuwania okna oraz maksymalizacji przez podwójne klikniêcie
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    ToggleMaximize();
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        // Logika przycisku maksymalizacji
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "?"; // Zmiana ikonki na "okno w oknie"
            }
            else
            {
                WindowState = WindowState.Normal;
                BtnMaximize.Content = "?"; // Powrót do pe³nego kwadratu
            }
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            SetActiveNav(btn);

            var tag = btn.Tag?.ToString();
            PageDashboard.Visibility = Visibility.Collapsed;
            PageTasks.Visibility = Visibility.Collapsed;
            PageStats.Visibility = Visibility.Collapsed;

            switch (tag)
            {
                case "0": PageDashboard.Visibility = Visibility.Visible; break;
                case "1": PageTasks.Visibility = Visibility.Visible; break;
                case "3": PageStats.Visibility = Visibility.Visible; break;
            }
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeNav != null)
                _activeNav.Background = System.Windows.Media.Brushes.Transparent;
            _activeNav = btn;
            btn.Background = (System.Windows.Media.Brush)FindResource("AccentPurpleBrush");
        }
    }
}