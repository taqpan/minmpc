using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings;

namespace minmpc.ViewModel {
    internal class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo, IDisposable {
        public event PropertyChangedEventHandler PropertyChanged;

        protected CompositeDisposable Disposable { get; private set; }

        public ViewModelBase() {
            this.Disposable = new CompositeDisposable();
        }

        void IDisposable.Dispose() {
            this.Disposable.Dispose();
        }

        public virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Dictionary<string, string> errors = new Dictionary<string, string>();

        public string Error {
            get { return null; }
        }

        public string this[string columnName] {
            get {
                string error = null;
                errors.TryGetValue(columnName, out error);
                return error;
            }
        }

        protected void SetError(string columnName, string error) {
            if (string.IsNullOrEmpty(error)) {
                errors.Remove(columnName);
            } else {
                errors[columnName] = error;
            }
        }

        public bool HasErrors {
            get { return errors.Any(x => !string.IsNullOrEmpty(x.Value)); }
        }
    }

    internal static class ViewModelBaseExtensions {
        public static ReactiveProperty<T> ToReactiveProperty<T>(this IObservable<T> source,
                                                                CompositeDisposable disposable,
                                                                ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe) {
            var rp = source.ToReactiveProperty(mode: mode);
            disposable.Add(rp);
            return rp;
        }
    }
}
