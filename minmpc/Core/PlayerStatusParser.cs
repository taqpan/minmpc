using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace minmpc.Core {
    internal static class PlayerStatusParser {
        private static Dictionary<string, Action<PlayerStatus, string>> parsers;

        public static PlayerStatus Parse(string statusText) {
            initParsers();

            var status = new PlayerStatus();

            var lines = statusText.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                if (line.StartsWith("ACK")) {
                    Debug.WriteLine(line);
                    return null;
                } else if (line.StartsWith("OK")) {
                    break;
                } else {
                    var entities = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                    if (entities.Length == 2) {
                        if (parsers.ContainsKey(entities[0])) {
                            parsers[entities[0]](status, entities[1]);
                        }
                    }
                }
            }

            return status;
        }

        private static void initParsers() {
            if (parsers == null) {
                parsers = new Dictionary<string, Action<PlayerStatus, string>>();

                parsers.Add("Title", (s, x) => { s.Title = x; });
                parsers.Add("Artist", (s, x) => { s.Artist = x; });
                parsers.Add("Album", (s, x) => { s.Album = x; });
                parsers.Add("AlbumArtist", (s, x) => { s.AlbumArtist = x; });
                parsers.Add("Time", (s, x) => { s.Duration = parseIntOrDefault(x); });
                parsers.Add("time", (s, x) => { s.Elapsed = parseIntOrDefault(x.Split(new[] {':'}, 2)[0]); });
                parsers.Add("songid", (s, x) => { s.SongId = parseIntOrDefault(x); });
                parsers.Add("volume", (s, x) => { s.Volume = parseIntOrDefault(x); });
                parsers.Add("repeat", (s, x) => { s.Repeat = parseIntOrDefault(x) != 0; });
                parsers.Add("random", (s, x) => { s.Random = parseIntOrDefault(x) != 0; });
                parsers.Add("single", (s, x) => { s.Single = parseIntOrDefault(x) != 0; });
                parsers.Add("consume", (s, x) => { s.Consume = parseIntOrDefault(x) != 0; });
                parsers.Add("state", (s, x) => {
                    PlaybackStatus status;
                    Enum.TryParse(x, true, out status);
                    s.PlaybackStatus = status;
                });
                parsers.Add("error", (s, x) => { s.Error = x; });
            }
        }

        private static int parseIntOrDefault(string text) {
            int i;
            int.TryParse(text, out i);
            return i;
        }
    }
}
