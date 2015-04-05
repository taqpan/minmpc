using System;

namespace minmpc.Core {
    internal class MpdServerException : Exception {
        public MpdServerException(string errorLine)
            : base(errorLine.StartsWith("ACK ") ? errorLine.Substring(4) : errorLine) {
        }
    }
}
