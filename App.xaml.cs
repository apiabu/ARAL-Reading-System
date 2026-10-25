using ARALReadingSystem.Data;
using System.Windows;

namespace ARALReadingSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(
            StartupEventArgs e)
        {
            DatabaseService.InitializeDatabase();

            base.OnStartup(e);
        }
    }
}