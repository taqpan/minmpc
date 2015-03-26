using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Threading;
using Autofac.AttributedComponent;

namespace minmpc.Core {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MpdClient {
        [Resource]
        private MpdRequestManager requestManager;

        private CancellationToken parentCancellationToken;

        public event Action<ConnectionStatusEventArgs> ConnectionStatusChanged;
        public IObservable<ConnectionStatusEventArgs> ConnectionStatusAsObservable() {
            return Observable.FromEvent<Action<ConnectionStatusEventArgs>, ConnectionStatusEventArgs>(
                h => h,
                h => ConnectionStatusChanged += h,
                h => ConnectionStatusChanged -= h);
        }

        public event Action<PlayerStatusEventArgs> PlayerStatusChanged;
        public IObservable<PlayerStatusEventArgs> PlayerStatusAsObservable() {
            return Observable.FromEvent<Action<PlayerStatusEventArgs>, PlayerStatusEventArgs>(
                h => h,
                h => PlayerStatusChanged += h,
                h => PlayerStatusChanged -= h);
        }

        public MpdClient(MpdRequestManager requestManager) {
            requestManager.Connected += v => {
                Debug.WriteLine("MpdClient - Connected");
                if (ConnectionStatusChanged != null) {
                    ConnectionStatusChanged(new ConnectionStatusEventArgs(true, v));
                }
            };
            requestManager.Disconnected += () => {
                Debug.WriteLine("MpdClient - Disconnected");
                if (ConnectionStatusChanged != null) {
                    ConnectionStatusChanged(new ConnectionStatusEventArgs(false));
                }
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

        public void Refresh() {
            execute(RequestMethods.Refresh, new string[] { });
        }

        public void Play() {
            execute(RequestMethods.Play, "play");
        }

        public void Pause(bool on) {
            execute(RequestMethods.Pause, "pause " + (on ? "1" : "0"));
        }

        public void Next() {
            execute(RequestMethods.Next, "next");
        }

        public void Previous() {
            execute(RequestMethods.Previous, "previous");
        }

        public void Restart() {
            execute(RequestMethods.Restart, new[] { "stop", "play" });
        }

        public void Stop() {
            execute(RequestMethods.Stop, "stop");
        }

        public void Volume(int value) {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            execute(RequestMethods.Volume, string.Format("setvol {0}", value));
        }

        public void Repeat(bool on) {
            execute(RequestMethods.Repeat, "repeat " + (on ? "1" : "0"));
        }

        public void Random(bool on) {
            execute(RequestMethods.Random, "random " + (on ? "1" : "0"));
        }

        public void Single(bool on) {
            execute(RequestMethods.Single, "single " + (on ? "1" : "0"));
        }

        public void Consume(bool on) {
            execute(RequestMethods.Consume, "consume " + (on ? "1" : "0"));
        }

        private void execute(RequestMethods requestMethod, string command) {
            execute(requestMethod, new[] { command });
        }

        private void execute(RequestMethods requestMethod, IEnumerable<string> commands) {
            if (!requestManager.AccesptsRequest) {
                Connect();
                Debug.WriteLine("RequestManager is not initialized. Trying to reconnect and a request has been ignored.");
                return;
            }

            var cmds = new List<string>();

            cmds.Add("command_list_begin");
            cmds.AddRange(commands);
            cmds.Add("currentsong");
            cmds.Add("status");
            cmds.Add("command_list_end");

            var req = new MpdRequest(String.Join("\n", cmds), s => onResponded(requestMethod, s));
            requestManager.EnqueueRequest(req);
        }

        private void onResponded(RequestMethods requestMethod, string response) {
            if (PlayerStatusChanged != null) {
                PlayerStatusChanged(new PlayerStatusEventArgs(requestMethod, PlayerStatusParser.Parse(response)));
            }
        }
    }
}
