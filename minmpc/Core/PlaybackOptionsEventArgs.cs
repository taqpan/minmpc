using System;
using System.Collections.Generic;

namespace minmpc.Core {
    internal class PlaybackOptionsEventArgs : MpdEventArgs {
        public int Volume { get; set; }
        public bool Repeat { get; set; }
        public bool Random { get; set; }
        public bool Single { get; set; }
        public bool Consume { get; set; }

        private static Dictionary<string, Action<PlaybackOptionsEventArgs, string>> parser;

        public static PlaybackOptionsEventArgs FromServerResponse(string response) {
            initParser();
            return ParseServerResponse(response, parser);
        }

        private static void initParser() {
            if (parser == null) {
                parser = new Dictionary<string, Action<PlaybackOptionsEventArgs, string>>();
                parser.Add("volume", (e, x) => { e.Volume = ParseIntOrDefault(x); });
                parser.Add("repeat", (e, x) => { e.Repeat = ParseIntOrDefault(x) != 0; });
                parser.Add("random", (e, x) => { e.Random = ParseIntOrDefault(x) != 0; });
                parser.Add("single", (e, x) => { e.Single = ParseIntOrDefault(x) != 0; });
                parser.Add("consume", (e, x) => { e.Consume = ParseIntOrDefault(x) != 0; });
            }
        }
    }
}
