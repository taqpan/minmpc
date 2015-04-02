using System;
using System.Windows;
using minmpc.ViewModel;

namespace minmpc {
    internal partial class OptionsWindow : Window {

        private OptionsWindowViewModel viewModel;

        public OptionsWindow() {
            InitializeComponent();
        }

        private void OptionsWindow_OnLoaded(object sender, RoutedEventArgs e) {
            viewModel = new OptionsWindowViewModel();
            DataContext = viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void OptionsWindow_OnClosed(object sender, EventArgs e) {
            if (DialogResult.HasValue && DialogResult.Value) {
                viewModel.Save();
            }
        }
    }
}
