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

namespace WMS.Desktop.Views
{
    /// <summary>
    /// Interaction logic for StockView.xaml
    /// </summary>
    public partial class StockView : UserControl
    {
        public StockView()
        {
            InitializeComponent();
            this.Loaded += StockViewLoaded;
        }

        private async void StockViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is StockViewModel viewModel)
            {
                await viewModel.RefreshAsync();
                await viewModel.LoadLookupsAsync();
            }
        }
    }
}
