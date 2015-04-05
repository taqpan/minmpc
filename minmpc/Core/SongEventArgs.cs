using System;
using System.Collections.Generic;

namespace minmpc.Core {
    internal class SongEventArgs : MpdEventArgs {
        public int SongId { get; set; }
        public string File { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public int Elapsed { get; set; }
        public int Duration { get; set; }
        public PlaybackStatus PlaybackStatus { get; set; }

        private static Dictionary<string, Action<SongEventArgs, string>> parser;

        public static SongEventArgs FromServerResponse(string response) {
            initParser();
            return ParseServerResponse(response, parser);
        }

        private static void initParser() {
            if (parser == null) {
                parser = new Dictionary<string, Action<SongEventArgs, string>>();
                parser.Add("file", (e, x) => { e.File = x; });
                parser.Add("Title", (e, x) => { e.Title = x; });
                parser.Add("Artist", (e, x) => { e.Artist = x; });
                parser.Add("Album", (e, x) => { e.Album = x; });
                parser.Add("AlbumArtist", (e, x) => { e.AlbumArtist = x; });
                parser.Add("Time", (e, x) => { e.Duration = ParseIntOrDefault(x); });
                parser.Add("time", (e, x) => { e.Elapsed = ParseIntOrDefault(x.Split(new[] { ':' }, 2)[0]); });
                parser.Add("songid", (e, x) => { e.SongId = ParseIntOrDefault(x); });
                parser.Add("state", (s, x) => {
                    PlaybackStatus status;
                    Enum.TryParse(x, true, out status);
                    s.PlaybackStatus = status;
                });
            }
        }
    }
}
