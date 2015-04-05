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
            mpdClient.SongEvent.AsObservable()
                .DistinctUntilChanged(_ => _.SongId)
                .Subscribe(_ => {
                    Title.Value = _.Title;
                    Artist.Value = _.Artist;
                    Album.Value = _.Album;
                    AlbumArtist.Value = _.AlbumArtist;
                    SongId.Value = _.SongId;
                });

            Elapsed = mpdClient.SongEvent.AsObservable()
                .Select(_ => TimeSpan.FromSeconds(_.Elapsed))
                .ToReactiveProperty(Disposable, mode);

            Duration = mpdClient.SongEvent.AsObservable()
                .Select(_ => TimeSpan.FromSeconds(_.Duration))
                .ToReactiveProperty(Disposable, mode);

            PlaybackStatus = mpdClient.SongEvent.AsObservable()
                .Select(_ => _.PlaybackStatus)
                .ToReactiveProperty(Disposable, mode);

            Volume = mpdClient.PlaybackOptionsEvent.AsObservable()
                .Select(_ => (double)_.Volume)
                .ToReactiveProperty(Disposable, mode);
            Volume.Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => mpdClient.Volume((int)_));

            Repeat = mpdClient.PlaybackOptionsEvent.AsObservable()
                .Select(_ => _.Repeat)
                .ToReactiveProperty(Disposable, mode);
            Repeat
                .Subscribe(_ => mpdClient.Repeat(Repeat.Value));

            Random = mpdClient.PlaybackOptionsEvent.AsObservable()
                .Select(_ => _.Random)
                .ToReactiveProperty(Disposable, mode);
            Random
                .Subscribe(_ => mpdClient.Random(Random.Value));

            ErrorMessage = mpdClient.PlayerErrorEvent.AsObservable()
                .Select(_ => _.Error)
                .ToReactiveProperty(Disposable, mode);

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
                    mpdClient.SeekWithId(SongId.Value, 0);
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
                    mpdClient.RequestPlayerStatus();
                });

            mpdClient.RequestPlayerStatus();
        }
    }
}
