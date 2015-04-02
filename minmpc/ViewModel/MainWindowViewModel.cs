using System;
using System.Reactive.Linq;
using Autofac.AttributedComponent;
using minmpc.Core;
using Reactive.Bindings;

namespace minmpc.ViewModel {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal class MainWindowViewModel : ViewModelBase {
        private const int ReplayThreashold = 3;

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

        public ReactiveProperty<bool> IsVisible { get; private set; }

        public MainWindowViewModel(MpdClient mpdClient) {
            var mode = ReactivePropertyMode.DistinctUntilChanged;

            SongId = new ReactiveProperty<int>();
            Title = new ReactiveProperty<string>();
            Artist = new ReactiveProperty<string>();
            Album = new ReactiveProperty<string>();
            AlbumArtist = new ReactiveProperty<string>();
            mpdClient.PlayerStatusAsObservable()
                .DistinctUntilChanged(_ => _.Status.SongId)
                .Subscribe(_ => {
                    Title.Value = _.Status.Title;
                    Artist.Value = _.Status.Artist;
                    Album.Value = _.Status.Album;
                    AlbumArtist.Value = _.Status.AlbumArtist;
                    SongId.Value = _.Status.SongId;
                });

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

            IsVisible = new ReactiveProperty<bool>();

            SongId
                .DistinctUntilChanged()
                .Subscribe(_ => IsVisible.Value = true);
            PlaybackStatus
                .DistinctUntilChanged()
                .Where(_ => _ != Core.PlaybackStatus.Stop)
                .Subscribe(_ => IsVisible.Value = true);

            Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Subscribe(_ => {
                    IsVisible.Value = false;
                    mpdClient.Refresh();
                });

            mpdClient.Refresh();
        }
    }
}
