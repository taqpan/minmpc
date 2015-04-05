using System;
using System.Collections.Generic;

namespace minmpc.Core {
    internal class EnumPlaylistEventArgs : MpdEventArgs {
        public List<string> Playlists { get; set; }

        public EnumPlaylistEventArgs() {
            Playlists = new List<string>();
        }

        private static Dictionary<string, Action<EnumPlaylistEventArgs, string>> parser;

        public static EnumPlaylistEventArgs FromServerResponse(string response) {
            initParser();
            return ParseServerResponse(response, parser);
        }

        private static void initParser() {
            if (parser == null) {
                parser = new Dictionary<string, Action<EnumPlaylistEventArgs, string>>();
                parser.Add("playlist", (e, x) => e.Playlists.Add(x));
            }
        }
    }
}
