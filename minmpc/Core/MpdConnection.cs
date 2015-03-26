using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Autofac.AttributedComponent;
using minmpc.Properties;

namespace minmpc.Core {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MpdConnection {

        private TcpClient tcpClient;
        private Encoding encoding;
        public string ServerVersion { get; private set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public void Initialize() {
            Host = Settings.Default.IpAddr;
            Port = Settings.Default.Port;
            encoding = Encoding.UTF8;
        }

        public void Connect(CancellationToken ct) {
            tcpClient = new TcpClient(Host, Port);

            ServerVersion = receive(ct);
        }

        public void Close() {
            if (tcpClient != null && tcpClient.Connected) {
                tcpClient.Close();
            }
        }

        public string Request(string command, CancellationToken ct) {
            Debug.WriteLine(command);

            var s = command.EndsWith("\n") ? command : command + "\n";
            var buffer = encoding.GetBytes(s);
            var t = tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length, ct);
            if (!t.Wait(Settings.Default.Timeout, ct)) {
                ct.ThrowIfCancellationRequested();
                throw new MpdConnectionException("Request timeout.");
            }
            if (t.IsFaulted) {
                throw new MpdConnectionException("Request faulted.", t.Exception.InnerExceptions.First());
            }
            ct.ThrowIfCancellationRequested();
            return receive(ct);
        }

        private string receive(CancellationToken ct) {
            var receivingStream = new MemoryStream();
            var buffer = new byte[1024];
            while (true) {
                var t = tcpClient.GetStream().ReadAsync(buffer, 0, 1024, ct);
                if (!t.Wait(Settings.Default.Timeout, ct)) {
                    ct.ThrowIfCancellationRequested();
                    throw new MpdConnectionException("Response timeout.");
                }
                if (t.IsFaulted) {
                    throw new MpdConnectionException("Response faulted.", t.Exception.InnerExceptions.First());
                }
                var size = t.Result;
                ct.ThrowIfCancellationRequested();

                receivingStream.WriteAsync(buffer, 0, size, ct).Wait();
                ct.ThrowIfCancellationRequested();

                // the last line starts with "OK", all response is received.
                var received = encoding.GetString(receivingStream.GetBuffer(), 0, size);
                if (received.EndsWith("\n")) {
                    var n = received.LastIndexOf("\n", received.Length - 2, StringComparison.Ordinal);
                    if (n < 0 || received.Substring(n + 1).StartsWith("OK")) {
                        return received;
                    }
                }
            }
        }
    }
}
