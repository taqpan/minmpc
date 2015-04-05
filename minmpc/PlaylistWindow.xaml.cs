using System.Windows;
using System.Windows.Input;
using minmpc.ViewModel;

namespace minmpc {
    internal partial class PlaylistWindow : Window {

        private PlaylistWindowViewModel viewModel;

        public PlaylistWindow() {
            InitializeComponent();
        }

        private void PlaylistWindow_OnLoaded(object sender, RoutedEventArgs e) {
            viewModel = new PlaylistWindowViewModel();
            DataContext = viewModel;
        }

        private void PlaylistList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            viewModel.SelectCommand.Execute();
            Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
