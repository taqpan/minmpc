using System;
using System.Windows;
using Autofac.AttributedComponent;
using minmpc.Properties;
using minmpc.ViewModel;

namespace minmpc {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class TrayIcon : IDisposable {
        [Resource]
        private MainWindow mainWindow;

        [Resource]
        private MainWindowViewModel mainWindowViewModel;

        private System.Windows.Forms.NotifyIcon icon { get; set; }

        public void Initialize() {
            var quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            quitMenuItem.Text = "Quit";
            quitMenuItem.Click += (sender, args) => Application.Current.Shutdown();

            var contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            contextMenuStrip.Items.Add(quitMenuItem);

            icon = new System.Windows.Forms.NotifyIcon();
            icon.Text = "minmpc";
            icon.Click += (sender, args) => {
                mainWindow.Activate();
                mainWindow.WindowState = WindowState.Normal;
                mainWindowViewModel.IsVisible.Value = true;
            };
            icon.Visible = true;
            icon.ContextMenuStrip = contextMenuStrip;
            icon.Icon = Resources.minmpc;
        }

        public void Dispose() {
            icon.Dispose();
        }
    }
}
