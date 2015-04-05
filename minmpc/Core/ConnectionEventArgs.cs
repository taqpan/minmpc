using System;

namespace minmpc.Core {
    internal class ConnectionEventArgs : EventArgs {
        public bool IsConnecting { get; private set; }
        public string ServerMessage { get; private set; }

        public ConnectionEventArgs(bool isConnecting, string serverMessage = null) {
            IsConnecting = isConnecting;
            ServerMessage = serverMessage;
        }
    }
}
