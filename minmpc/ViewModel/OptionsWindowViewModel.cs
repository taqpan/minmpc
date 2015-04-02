using minmpc.Properties;
using Reactive.Bindings;

namespace minmpc.ViewModel {
    internal class OptionsWindowViewModel : ViewModelBase {
        public ReactiveProperty<string> Host { get; private set; }
        public ReactiveProperty<int> Port { get; private set; }

        public OptionsWindowViewModel() {
            Host = new ReactiveProperty<string>(Settings.Default.Host);
            Port = new ReactiveProperty<int>(Settings.Default.Port);
        }

        public void Save() {
            Settings.Default.Host = Host.Value;
            Settings.Default.Port = Port.Value;
            Settings.Default.Save();
        }
    }
}
