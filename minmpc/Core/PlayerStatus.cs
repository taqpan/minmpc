namespace minmpc.Core {
    internal enum PlaybackStatus {
        Unknown, Play, Stop, Pause
    }

    internal class PlayerStatus {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public int Elapsed { get; set; }
        public int Duration { get; set; }
        public int Volume { get; set; }
        public bool Repeat { get; set; }
        public bool Random { get; set; }
        public bool Single { get; set; }
        public bool Consume { get; set; }
        public PlaybackStatus PlaybackStatus { get; set; }
        public string Error { get; set; }
    }
}
