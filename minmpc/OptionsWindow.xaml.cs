using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autofac;
using minmpc.Core;
using minmpc.ViewModel;

namespace minmpc {
    public partial class OptionsWindow : Window {

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
