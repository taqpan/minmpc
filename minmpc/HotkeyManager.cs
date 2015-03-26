using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Interop;

namespace minmpc {
    internal class HotkeyManager {
        private const int WM_HOTKEY = 0x0312;

        private IntPtr hwnd;
        private Dictionary<int, EventHandler> hotkeyEvents;

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, int MOD_KEY, int VK);

        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyManager(MainWindow mainWindow) {
            var host = new WindowInteropHelper(mainWindow);
            hwnd = host.Handle;

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;

            hotkeyEvents = new Dictionary<int, EventHandler>();
        }

        public void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled) {
            if (msg.message != WM_HOTKEY) return;

            var hotkeyId = msg.wParam.ToInt32();
            if (hotkeyEvents.All(x => x.Key != hotkeyId)) return;

            new ThreadStart(() => hotkeyEvents[hotkeyId](this, EventArgs.Empty)).Invoke();
        }

        private int i = 0;
        public void Register(ModifierKeys modkey, Key trigger, EventHandler eh) {
            var imod = (Int32) modkey;
            var itrg = KeyInterop.VirtualKeyFromKey(trigger);

            // 0xc000 - 0xffff are reserved.
            while (++i < 0xc000) {
                if (RegisterHotKey(hwnd, i, imod, itrg) != 0) {
                    break;
                }
            }

            if (i < 0xc000) {
                hotkeyEvents.Add(i, eh);
            }
        }

        public void Unregister() {
            foreach (var hotkeyid in hotkeyEvents.Keys) {
                UnregisterHotKey(hwnd, hotkeyid);
            }
        }
    }
}