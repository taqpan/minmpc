using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using Autofac;
using Autofac.AttributedComponent;
using minmpc.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace minmpc.ViewModel {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MainWindowViewModel : ViewModelBase {
        private const int ReplayThreashold = 3;

        [Resource]
        private ILifetimeScope CurrentScope;

        [Resource]
        private MpdClient mpdClient;

        public ReactiveProperty<int> SongId { get; private set; }
        public ReactiveProperty<string> Title { get; private set; }
        public ReactiveProperty<string> Artist { get; private set; }
        public ReactiveProperty<string> Album { get; private set; }
        public ReactiveProperty<string> AlbumArtist { get; private set; }
        public ReactiveProperty<TimeSpan> Elapsed { get; private set; }
        public ReactiveProperty<TimeSpan> Duration { get; private set; }
        public ReactiveProperty<double> Volume { get; private set; }
        public ReactiveProperty<bool> Repeat { get; private set; }
        public ReactiveProperty<bool> Random { get; private set; }
        public ReactiveProperty<bool> Single { get; private set; }
        public ReactiveProperty<bool> Consume { get; private set; }
        public ReactiveProperty<PlaybackStatus> PlaybackStatus { get; private set; }
        public ReactiveProperty<string> ErrorMessage { get; private set; }

        public ReactiveCommand RefreshCommand { get; private set; }
        public ReactiveCommand PlayCommand { get; private set; }
        public ReactiveCommand PauseCommand { get; private set; }
        public ReactiveCommand PlayPauseCommand { get; private set; }
        public ReactiveCommand PreviousCommand { get; private set; }
        public ReactiveCommand NextCommand { get; private set; }
        public ReactiveCommand StopCommand { get; private set; }

        public MainWindowViewModel(MpdClient mpdClient) {
            this.mpdClient = mpdClient;

            var mode = ReactivePropertyMode.DistinctUntilChanged;

            SongId = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.SongId)
                .ToReactiveProperty(Disposable, mode);
            SongId.Subscribe(_ => {
            });

            Title = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.Title)
                .ToReactiveProperty(Disposable, mode);

            Artist = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.Artist)
                .ToReactiveProperty(Disposable, mode);

            Album = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.Album)
                .ToReactiveProperty(Disposable, mode);

            AlbumArtist = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.AlbumArtist)
                .ToReactiveProperty(Disposable, mode);

            Elapsed = mpdClient.PlayerStatusAsObservable()
                .Select(_ => TimeSpan.FromSeconds(_.Status.Elapsed))
                .ToReactiveProperty(Disposable, mode);

            Duration = mpdClient.PlayerStatusAsObservable()
                .Select(_ => TimeSpan.FromSeconds(_.Status.Duration))
                .ToReactiveProperty(Disposable, mode);

            Volume = mpdClient.PlayerStatusAsObservable()
                .Where(_ => _.RequestMethod != RequestMethods.Volume)
                .Select(_ => (double)_.Status.Volume)
                .ToReactiveProperty(Disposable, mode);
            Volume.Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => mpdClient.Volume((int)_));

            PlaybackStatus = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.PlaybackStatus)
                .ToReactiveProperty(Disposable, mode);

            ErrorMessage = mpdClient.PlayerStatusAsObservable()
                .Select(_ => _.Status.Error)
                .ToReactiveProperty(Disposable, mode);

            Repeat = mpdClient.PlayerStatusAsObservable()
                .Where(_ => _.RequestMethod != RequestMethods.Repeat)
                .Select(_ => _.Status.Repeat)
                .ToReactiveProperty(Disposable, mode);
            Repeat.Select(_ => _)
                .Subscribe(_ => mpdClient.Repeat(Repeat.Value));

            Random = mpdClient.PlayerStatusAsObservable()
                .Where(_ => _.RequestMethod != RequestMethods.Random)
                .Select(_ => _.Status.Random)
                .ToReactiveProperty(Disposable, mode);
            Random.Select(_ => _)
                .Subscribe(_ => mpdClient.Random(Random.Value));

            PlayCommand = new ReactiveCommand();
            PlayCommand.Subscribe(_ => mpdClient.Play());

            PauseCommand = new ReactiveCommand();
            PauseCommand.Subscribe(_ => mpdClient.Pause(PlaybackStatus.Value != Core.PlaybackStatus.Pause));

            PlayPauseCommand = new ReactiveCommand();
            PlayPauseCommand.Subscribe(_ => {
                if (PlaybackStatus.Value == Core.PlaybackStatus.Play) {
                    mpdClient.Pause(true);
                } else {
                    mpdClient.Play();
                }
            });

            PreviousCommand = new ReactiveCommand();
            PreviousCommand.Subscribe(_ => {
                if (Elapsed.Value.TotalSeconds <= ReplayThreashold) {
                    mpdClient.Previous();
                } else {
                    mpdClient.Restart();
                }
            });

            NextCommand = new ReactiveCommand();
            NextCommand.Subscribe(_ => mpdClient.Next());

            StopCommand = new ReactiveCommand();
            StopCommand.Subscribe(_ => mpdClient.Stop());

            Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Where(_ => true)
                .Subscribe(_ => mpdClient.Refresh());

            mpdClient.Refresh();
        }
    }
}
