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
    public partial class ReceiptView : UserControl
    {
        public ReceiptView()
        {
            InitializeComponent();
            this.Loaded += ReceiptViewLoaded;
        }

        private async void ReceiptViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReceiptViewModel viewModel)
            {
                await viewModel.RefreshAsync();
                await viewModel.LoadOperators();
                await viewModel.LoadRacks();
            }
        }
        private void RootGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Убираем фокус с любого TextBox
            Keyboard.ClearFocus();
        }

        private void RackBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RackViewModel rackVm)
            {
                var vm = (ReceiptViewModel)DataContext;
                vm.SelectedRackView = rackVm;
                int row = rackVm.Row;
                int column = rackVm.RackNum;

                if (row != 5 && column == 2)
                {
                    Row1Grid.Visibility = Visibility.Visible;
                    Row2Grid.Visibility = Visibility.Collapsed;
                    Row3Grid.Visibility = Visibility.Collapsed;
                }
                else if (row != 5 && (column == 1 || column == 3 || column == 4))
                {
                    Row1Grid.Visibility = Visibility.Collapsed;
                    Row2Grid.Visibility = Visibility.Visible;
                    Row3Grid.Visibility = Visibility.Collapsed;
                }
                else if (row == 5)
                {
                    Row1Grid.Visibility = Visibility.Collapsed;
                    Row2Grid.Visibility = Visibility.Collapsed;
                    Row3Grid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
