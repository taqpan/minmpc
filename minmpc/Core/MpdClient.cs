using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Autofac.AttributedComponent;

namespace minmpc.Core {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MpdClient {
        [Resource]
        private MpdRequestManager requestManager;

        private CancellationToken parentCancellationToken;

        public EventAsObservable<ConnectionEventArgs> ConnectionEvent = new EventAsObservable<ConnectionEventArgs>();
        public EventAsObservable<SongEventArgs> SongEvent = new EventAsObservable<SongEventArgs>();
        public EventAsObservable<PlaybackOptionsEventArgs> PlaybackOptionsEvent = new EventAsObservable<PlaybackOptionsEventArgs>();
        public EventAsObservable<PlayerErrorEventArgs> PlayerErrorEvent = new EventAsObservable<PlayerErrorEventArgs>();

        public MpdClient(MpdRequestManager requestManager) {
            requestManager.Connected += v => {
                Debug.WriteLine("MpdClient - Connected");
                ConnectionEvent.OnChanged(new ConnectionEventArgs(true, v));
            };
            requestManager.Disconnected += () => {
                Debug.WriteLine("MpdClient - Disconnected");
                ConnectionEvent.OnChanged(new ConnectionEventArgs(false));
            };
        }

        public void Start(CancellationToken ct) {
            parentCancellationToken = ct;
            Connect();
        }

        public void Connect() {
            if (requestManager.AccesptsRequest) {
                throw new InvalidOperationException("Already connected.");
            }
            requestManager.StartAsync(parentCancellationToken);
        }

        public bool Play() {
            return executePlaybackCommand("play");
        }

        public bool Pause(bool on) {
            return executePlaybackCommand("pause " + (on ? "1" : "0"));
        }

        public bool Next() {
            return executePlaybackCommand("next");
        }

        public bool Previous() {
            return executePlaybackCommand("previous");
        }

        public bool SeekWithId(int songId, int time) {
            return executePlaybackCommand(string.Format("seekid {0} {1}", songId, time));
        }

        public bool Stop() {
            return executePlaybackCommand("stop");
        }

        public bool Volume(int value) {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            return execute(string.Format("setvol {0}", value));
        }

        public bool Repeat(bool on) {
            return execute("repeat " + (on ? "1" : "0"));
        }

        public bool Random(bool on) {
            return execute("random " + (on ? "1" : "0"));
        }

        public bool Single(bool on) {
            return execute("single " + (on ? "1" : "0"));
        }

        public bool Consume(bool on) {
            return execute("consume " + (on ? "1" : "0"));
        }

        public bool RequestPlayerStatus() {
            return executePlaybackCommand(new string[] { });
        }

        private bool executePlaybackCommand(string command) {
            return executePlaybackCommand(new[] { command });
        }

        private bool executePlaybackCommand(IEnumerable<string> commands) {
            var cmds = new List<string>();

            cmds.Add("command_list_begin");
            cmds.AddRange(commands);
            cmds.Add("currentsong");
            cmds.Add("status");
            cmds.Add("command_list_end");

            return execute(String.Join("\n", cmds), response => {
                try {
                    SongEvent.OnChanged(
                        SongEventArgs.FromServerResponse(response));
                    PlaybackOptionsEvent.OnChanged(
                        PlaybackOptionsEventArgs.FromServerResponse(response));
                } catch (MpdServerException e) {
                    PlayerErrorEvent.OnChanged(new PlayerErrorEventArgs { Error = e.Message });
                }
            });
        }

        private bool execute(string command) {
            return execute(command, response => {
                var e = PlayerErrorEventArgs.FromServerResponse(response);
                if (e != null) {
                    PlayerErrorEvent.OnChanged(e);
                }
            });
        }

        private bool execute(string command, Action<string> onResponded) {
            if (!requestManager.AccesptsRequest) {
                Connect();
                Debug.WriteLine("RequestManager is not initialized. Trying to reconnect and a request has been ignored.");
                return false;
            }

            var req = new MpdRequest(command, onResponded);
            requestManager.EnqueueRequest(req);

            return true;
        }

    }
}
