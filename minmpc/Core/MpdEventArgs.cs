using System;
using System.Collections.Generic;

namespace minmpc.Core {
    internal abstract class MpdEventArgs : EventArgs {

        protected static TEventArgs ParseServerResponse<TEventArgs>(
            string serverResponse,
            Dictionary<string, Action<TEventArgs, string>> parser) where TEventArgs : new() {

            var instance = new TEventArgs();

            var lines = serverResponse.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                if (line.StartsWith("ACK")) {
                    throw new MpdServerException(line);
                }

                if (line.StartsWith("OK")) {
                    break;
                }

                var entities = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                if (entities.Length == 2) {
                    if (parser.ContainsKey(entities[0])) {
                        parser[entities[0]](instance, entities[1]);
                    }
                }
            }

            return instance;
        }

        protected static int ParseIntOrDefault(string text) {
            int i;
            int.TryParse(text, out i);
            return i;
        }
    }
}
