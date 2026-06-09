using System.Windows;
using System.Windows.Input;

namespace MyWay.Views
{
    public partial class LoginWindow : Window
    {
        private const string ValidEmail    = "user@myway.pl";
        private const string ValidPassword = "MyWay123";

        public bool LoginSuccessful { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e) => TryLogin();

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TryLogin();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void TryLogin()
        {
            var email    = TxtEmail.Text.Trim();
            var password = TxtPassword.Password;

            if (email == ValidEmail && password == ValidPassword)
            {
                LoginSuccessful = true;
                Close();
            }
            else
            {
                TxtError.Text       = "Nieprawidłowy e-mail lub hasło.";
                TxtError.Visibility = Visibility.Visible;
                TxtPassword.Clear();
                TxtPassword.Focus();
            }
        }
    }
}
