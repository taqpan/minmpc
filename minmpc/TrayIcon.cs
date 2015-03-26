using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.AttributedComponent;
using minmpc.Properties;

namespace minmpc {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class TrayIcon : IDisposable {
        [Resource]
        private MainWindow mainWindow;

        private System.Windows.Forms.NotifyIcon icon { get; set; }

        public void Initialize() {
            var quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            quitMenuItem.Text = "Quit";
            quitMenuItem.Click += (sender, args) => System.Windows.Application.Current.Shutdown();

            var contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            contextMenuStrip.Items.Add(quitMenuItem);

            icon = new System.Windows.Forms.NotifyIcon();
            icon.Click += (sender, args) => mainWindow.Activate();
            icon.Visible = true;
            icon.ContextMenuStrip = contextMenuStrip;
            icon.Icon = Resources.minmpc;
        }

        public void Dispose() {
            icon.Dispose();
        }
    }
}
