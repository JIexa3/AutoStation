using System;
using System.Windows;
using AutoStation.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoStation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new AutoStationContext())
                {
                    context.EnsureDatabaseCreated();
                }

                var mainWindow = new Views.LoginWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}
