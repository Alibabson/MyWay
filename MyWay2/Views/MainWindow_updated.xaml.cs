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
            this.MaxWidth  = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2) ToggleMaximize();
                else DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "❐";
            }
            else
            {
                WindowState = WindowState.Normal;
                BtnMaximize.Content = "☐";
            }
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            SetActiveNav(btn);

            // Ukryj wszystkie strony
            PageDashboard.Visibility = Visibility.Collapsed;
            PageTasks.Visibility     = Visibility.Collapsed;
            PageStats.Visibility     = Visibility.Collapsed;
            PageProfile.Visibility   = Visibility.Collapsed;   // ← NOWE

            switch (btn.Tag?.ToString())
            {
                case "0": PageDashboard.Visibility = Visibility.Visible; break;
                case "1": PageTasks.Visibility     = Visibility.Visible; break;
                case "3": PageStats.Visibility     = Visibility.Visible; break;
                case "4": PageProfile.Visibility   = Visibility.Visible; break;   // ← NOWE
            }
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeNav != null)
            {
                _activeNav.Background   = System.Windows.Media.Brushes.Transparent;
                _activeNav.BorderBrush  = System.Windows.Media.Brushes.Transparent;
                _activeNav.Foreground   = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush");
            }
            _activeNav = btn;
            btn.Background  = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(40, 123, 110, 245));
            btn.BorderBrush = (System.Windows.Media.Brush)FindResource("AccentPurpleBrush");
            btn.Foreground  = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush");
        }
    }
}
