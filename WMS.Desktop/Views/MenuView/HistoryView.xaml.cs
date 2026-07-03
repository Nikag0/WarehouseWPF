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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMS.Desktop.ViewModels;
using WMS.Desktop.ViewModels.MenuViewModels;

namespace WMS.Desktop.Views.MenuView
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
            this.Loaded += HistoryViewLoaded;
        }

        private async void HistoryViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is HistoryViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }
    }
}
