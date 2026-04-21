using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace DevDock.Services
{
    /// <summary>
    /// Manages the system tray icon and tray interactions for showing or hiding the app window.
    /// </summary>
    public class TrayService : IDisposable
    {
        private readonly Window _mainWindow;
        private readonly Action _exitAction;
        private NotifyIcon? _notifyIcon;

        public TrayService(Window mainWindow, Action exitAction)
        {
            _mainWindow = mainWindow;
            _exitAction = exitAction;
        }

        /// <summary>
        /// Creates the tray icon, assigns the context menu, and wires tray click behavior.
        /// </summary>
        public void Initialize()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "FocusTray",
                Visible = true
            };

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "tray.ico");
            if (File.Exists(iconPath))
            {
                _notifyIcon.Icon = new Icon(iconPath);
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }

            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (_, _) => ShowWindow();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) => _exitAction();

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.MouseClick += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ToggleWindow();
                }
            };
        }

        /// <summary>
        /// Toggles visibility of the main window from the tray icon.
        /// Dispatcher is used because tray events may not arrive on the WPF UI thread.
        /// </summary>
        private void ToggleWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_mainWindow.IsVisible)
                {
                    _mainWindow.Hide();
                }
                else
                {
                    ShowWindow();
                }
            });
        }

        /// <summary>
        /// Shows the window in the bottom-right corner and brings it to the foreground.
        /// </summary>
        private void ShowWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PositionWindowBottomRight();

                _mainWindow.Show();
                _mainWindow.Activate();
                _mainWindow.Topmost = true;
                _mainWindow.Topmost = false;
                _mainWindow.Focus();
            });
        }

        /// <summary>
        /// Positions the app like a tray companion window near the bottom-right working area.
        /// </summary>
        private void PositionWindowBottomRight()
        {
            const double margin = 12;

            var workArea = SystemParameters.WorkArea;
            _mainWindow.Left = workArea.Right - _mainWindow.Width - margin;
            _mainWindow.Top = workArea.Bottom - _mainWindow.Height - margin;
        }

        /// <summary>
        /// Cleans up the tray icon so it does not remain orphaned in the system tray.
        /// </summary>
        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}