using System;
using System.Reactive.Linq;

namespace minmpc.Core {
    public class EventAsObservable<TEventArgs> {
        public event Action<TEventArgs> Changed;
        public void OnChanged(TEventArgs e) {
            if (Changed != null) {
                Changed(e);
            }
        }
        public IObservable<TEventArgs> AsObservable() {
            return Observable.FromEvent<Action<TEventArgs>, TEventArgs>(
                h => h,
                h => Changed += h,
                h => Changed -= h);
        }
    }
}
