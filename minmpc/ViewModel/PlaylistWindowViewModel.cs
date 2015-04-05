using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using minmpc.Core;
using Reactive.Bindings;
using Autofac;

namespace minmpc.ViewModel {
    internal class PlaylistWindowViewModel : ViewModelBase {
        public ReactiveProperty<List<string>> Playlists { get; private set; }
        public ReactiveProperty<string> SelectedPlaylist { get; private set; }
        public ReactiveCommand SelectCommand { get; private set; }

        public PlaylistWindowViewModel() {
            var app = (App)Application.Current;
            var mpdClient = app.Container.Resolve<MpdClient>();

            Playlists = mpdClient.EnumPlaylistEvent.AsObservable()
                .Select(_ => _.Playlists)
                .ToReactiveProperty();

            SelectedPlaylist = new ReactiveProperty<string>();

            SelectCommand = SelectedPlaylist
                .Select(_ => _ != null)
                .ToReactiveCommand();
            SelectCommand
                .Subscribe(_ => mpdClient.SelectPlaylist(SelectedPlaylist.Value));

            mpdClient.ListPlaylists();
        }
    }
}
