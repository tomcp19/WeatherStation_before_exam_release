using System.Windows;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplicationView app = new ApplicationView();


            app.Show();

        }
    }
}
