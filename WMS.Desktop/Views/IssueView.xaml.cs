using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using WMS.Domain;

namespace WMS.Desktop.Views
{
    /// <summary>
    /// Interaction logic for IssueViews.xaml
    /// </summary>
    public partial class IssueView : UserControl
    { 

        public IssueView()
        {
            InitializeComponent();
            this.Loaded += UsersViewLoaded;
        }

        private async void UsersViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IssueViewModel viewModel)
            {
                await viewModel.RefreshAsync();
                await viewModel.LoadOperators();
                await viewModel.LoadRacks();
                await viewModel.LoadCells();
            }
        }

        private void VisibleRow1(object sender, MouseButtonEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Visible;
            Row2Grid.Visibility = Visibility.Collapsed;
            Row3Grid.Visibility = Visibility.Collapsed;
        }
        private void VisibleRow2(object sender, MouseButtonEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Collapsed;
            Row2Grid.Visibility = Visibility.Visible;
            Row3Grid.Visibility = Visibility.Collapsed;
        }

        private void VisibleRow3(object sender, MouseButtonEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Collapsed;
            Row2Grid.Visibility = Visibility.Collapsed;
            Row3Grid.Visibility = Visibility.Visible;
        }

        private void RackBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RackViewModel rackVm)
            {
                var vm = (IssueViewModel)DataContext;
                vm.SelectedRack = rackVm;
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
