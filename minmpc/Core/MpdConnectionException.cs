using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
