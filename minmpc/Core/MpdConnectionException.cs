using System;

namespace minmpc.Core {
    internal class MpdConnectionException : Exception {
        public MpdConnectionException(string message)
            : base(message) {
        }

        public MpdConnectionException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
