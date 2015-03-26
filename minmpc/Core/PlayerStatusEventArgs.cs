using System;

namespace minmpc.Core {
    internal class PlayerStatusEventArgs : EventArgs {
        public RequestMethods RequestMethod;
        public PlayerStatus Status { get; private set; }

        public PlayerStatusEventArgs(RequestMethods requestMethod, PlayerStatus status) {
            RequestMethod = requestMethod;
            Status = status;
        }
    }
}
