using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac.AttributedComponent;

namespace minmpc.Core {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MpdRequestManager {
        [Resource]
        private MpdConnection connection;

        private CancellationToken parentCancellationToken = CancellationToken.None;

        private BlockingCollection<MpdRequest> queue =
            new BlockingCollection<MpdRequest>(new ConcurrentQueue<MpdRequest>());

        public bool AccesptsRequest { get; private set; }
        public event Action<string> Connected;
        public event Action Disconnected;

        public void StartAsync(CancellationToken ct) {
            AccesptsRequest = false;
            connection.Initialize();

            Task.Factory.StartNew(() => {
                try {
                    start(ct);
                } catch {
                    AccesptsRequest = false;
                    connection.Close();
                    if (Disconnected != null) Disconnected();
                }
            }, ct);
        }

        private void start(CancellationToken cancellationToken) {
            parentCancellationToken = cancellationToken;

            connection.Connect(parentCancellationToken);
            AccesptsRequest = true;
            if (Connected != null) Connected(connection.ServerVersion);

            parentCancellationToken.ThrowIfCancellationRequested();

            var mpdCommandSemaphore = new SemaphoreSlim(1, 1);

            while (true) {
                parentCancellationToken.ThrowIfCancellationRequested();

                var req = queue.Take(parentCancellationToken);

                if (req.CancellationTokenSource.IsCancellationRequested) continue;

                mpdCommandSemaphore.Wait(parentCancellationToken);

                if (req.CancellationTokenSource.IsCancellationRequested) continue;

                try {
                    var result = connection.Request(req.Command, req.CancellationTokenSource.Token);
                    req.OnResponsed(result);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.ToString());
                    if (ex.InnerException is OperationCanceledException) {
                        req.CancellationTokenSource.Cancel();
                    } else {
                        throw;
                    }
                }

                mpdCommandSemaphore.Release();
            }
        }

        public void EnqueueRequest(MpdRequest request) {
            if (!AccesptsRequest) {
                throw new InvalidOperationException("Start the worker before requesting.");
            }

            request.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentCancellationToken);
            queue.Add(request);
        }
    }
}
