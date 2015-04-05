using System;
using System.Collections.Generic;

namespace minmpc.Core {
    internal class PlayerErrorEventArgs : MpdEventArgs {
        public string Error { get; set; }

        private static readonly Dictionary<string, Action<SongEventArgs, string>> parser =
            new Dictionary<string, Action<SongEventArgs, string>>();

        public static PlayerErrorEventArgs FromServerResponse(string response) {
            try {
                ParseServerResponse(response, parser);
                return null;
            } catch (MpdServerException e) {
                return new PlayerErrorEventArgs {Error = e.Message};
            }
        }
    }
}
