using System;
using System.Threading;

namespace minmpc.Core {
    internal class MpdRequest {
        public string Command { get; set; }
        public Action<string> OnResponsed;
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public MpdRequest(string command, Action<string> onResponsed) {
            Command = command;
            OnResponsed = onResponsed;
        }
    }
}
