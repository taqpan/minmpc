using System;

namespace minmpc.Core {
    internal class ConnectionStatusEventArgs : EventArgs {
        public bool IsConnecting { get; private set; }
        public string ServerMessage { get; private set; }

        public ConnectionStatusEventArgs(bool isConnecting, string serverMessage = null) {
            IsConnecting = isConnecting;
            ServerMessage = serverMessage;
        }
    }
}
