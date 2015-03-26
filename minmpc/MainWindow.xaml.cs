using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using Autofac.AttributedComponent;
using minmpc.Core;
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

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            hotkeyManager.Unregister();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
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

        public void OnHotKeyPlayPause(object sender, EventArgs e) {
            var app = (App)Application.Current;
            if (viewModel.PlaybackStatus.Value == PlaybackStatus.Play) {
                app.Container.Resolve<MpdClient>().Pause(true);
            } else {
                app.Container.Resolve<MpdClient>().Play();
            }
        }

        public void OnHotKeyNext(object sender, EventArgs e) {
            var app = (App)Application.Current;
            app.Container.Resolve<MpdClient>().Next();
        }

        public void OnHotKeyPrevious(object sender, EventArgs e) {
            var app = (App)Application.Current;
            app.Container.Resolve<MpdClient>().Previous();
        }

        public void OnHotKeyStop(object sender, EventArgs e) {
            var app = (App)Application.Current;
            app.Container.Resolve<MpdClient>().Stop();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }
    }
}
