using System.Windows;
using MyWay.Views;

namespace MyWay
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var login = new LoginWindow();
            login.ShowDialog();

            if (!login.LoginSuccessful)
            {
                Shutdown();
                return;
            }

            var main = new MainWindow();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
    }
}
