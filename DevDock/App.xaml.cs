using DevDock;
using DevDock.Services;
using DevDock.ViewModels;
using DevDock.Views;
using System;
using System.Windows;

namespace DevDock
{
    public partial class App : Application
    {
        private TrayService? _trayService;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainViewModel = new MainViewModel();
            _mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            _trayService = new TrayService(_mainWindow, ExitApplication);
            _trayService.Initialize();

            // Start hidden in tray
            //_mainWindow.Hide();
        }

        private void ExitApplication()
        {
            _trayService?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayService?.Dispose();
            base.OnExit(e);
        }
    }
}