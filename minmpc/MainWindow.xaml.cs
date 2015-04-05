using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Autofac.AttributedComponent;
using minmpc.ViewModel;

namespace minmpc {
    [Component(Scope = ComponentScope.SingleInstance)]
    internal partial class MainWindow : Window {

        [Resource]
        private MainWindowViewModel viewModel;
        
        private HotkeyManager hotkeyManager;

        public MainWindow() {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            this.Top = SystemParameters.MaximizedPrimaryScreenHeight - this.ActualHeight - 20;
            this.Left = SystemParameters.MaximizedPrimaryScreenWidth - this.ActualWidth - 20;

            DataContext = viewModel;

            hotkeyManager = new HotkeyManager(this);
            hotkeyManager.Register(ModifierKeys.None, Key.MediaPlayPause, OnHotKeyPlayPause);
            hotkeyManager.Register(ModifierKeys.None, Key.MediaNextTrack, OnHotKeyNext);
            hotkeyManager.Register(ModifierKeys.None, Key.MediaPreviousTrack, OnHotKeyPrevious);
            hotkeyManager.Register(ModifierKeys.None, Key.MediaStop, OnHotKeyStop);
        }

        private void MainWindow_OnContentRendered(object sender, EventArgs e) {
            viewModel.SongId
                .DistinctUntilChanged()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => {
                    computeTextScroll(SongTextArea, SongTextBlock);
                    computeTextScroll(AlbumTextArea, AlbumTextBlock);
                });
        }

        private void computeTextScroll(Grid area, TextBlock text) {
            var ft = new FormattedText(
                text.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch),
                text.FontSize,
                text.Foreground);
            if (text.HasAnimatedProperties) {
                text.BeginAnimation(MarginProperty, null);
            }
            if (area.ActualWidth < ft.Width) {
                var animation = new ThicknessAnimationUsingKeyFrames();
                animation.KeyFrames.Add(new LinearThicknessKeyFrame(
                    new Thickness(0),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
                animation.KeyFrames.Add(new LinearThicknessKeyFrame(
                    new Thickness(0),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
                animation.KeyFrames.Add(new LinearThicknessKeyFrame(
                    new Thickness(area.ActualWidth - ft.Width - 20, 0, 0, 0),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ft.Width * 10))));
                animation.KeyFrames.Add(new LinearThicknessKeyFrame(
                    new Thickness(area.ActualWidth - ft.Width - 20, 0, 0, 0),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ft.Width * 10 + 2000))));
                animation.RepeatBehavior = RepeatBehavior.Forever;
                text.BeginAnimation(MarginProperty, animation);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            hotkeyManager.Unregister();
        }

        private void QuitButton_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void VolumeSlider_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                if (viewModel.Volume.Value < 100) {
                    viewModel.Volume.Value += 1;
                }
            } else {
                if (viewModel.Volume.Value > 0) {
                    viewModel.Volume.Value -= 1;
                }
            }
        }

        #region hotkey
        public void OnHotKeyPlayPause(object sender, EventArgs e) {
            if (viewModel.PlayPauseCommand.CanExecute()) {
                viewModel.PlayPauseCommand.Execute();
            }
        }

        public void OnHotKeyNext(object sender, EventArgs e) {
            if (viewModel.NextCommand.CanExecute()) {
                viewModel.NextCommand.Execute();
            }
        }

        public void OnHotKeyPrevious(object sender, EventArgs e) {
            if (viewModel.PreviousCommand.CanExecute()) {
                viewModel.PreviousCommand.Execute();
            }
        }

        public void OnHotKeyStop(object sender, EventArgs e) {
            if (viewModel.StopCommand.CanExecute()) {
                viewModel.StopCommand.Execute();
            }
        }
        #endregion

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e) {
            viewModel.IsVisible.Value = true;
        }

        private void PlaylistButton_OnClick(object sender, RoutedEventArgs e) {
            var playlistWindow = new PlaylistWindow();
            playlistWindow.Owner = this;
            playlistWindow.ShowDialog();
            viewModel.IsVisible.Value = true;
        }

        private void OptionsButton_OnClick(object sender, RoutedEventArgs e) {
            var optionsWindow = new OptionsWindow();
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();
            viewModel.IsVisible.Value = true;
        }
    }
}
