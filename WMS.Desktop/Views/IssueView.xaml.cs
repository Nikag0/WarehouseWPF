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
            this.Loaded += StockItemDtoViewLoaded;
            this.Loaded += UsersViewLoaded;
            
        }
        private async void StockItemDtoViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IssueViewModel viewModel)
                await viewModel.RefreshAsync();
        } 
        private async void UsersViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IssueViewModel viewModel)
                await viewModel.LoadUsers();
        }

        private void VisibleRow2(object sender, RoutedEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Collapsed;
            Row2Grid.Visibility = Visibility.Visible;
            Row3Grid.Visibility = Visibility.Collapsed;
        }

        private void VisibleRow3(object sender, RoutedEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Collapsed;
            Row2Grid.Visibility = Visibility.Collapsed;
            Row3Grid.Visibility = Visibility.Visible;
        }

        private void VisibleRow1(object sender, MouseButtonEventArgs e)
        {
            Row1Grid.Visibility = Visibility.Visible;
            Row2Grid.Visibility = Visibility.Collapsed;
            Row3Grid.Visibility = Visibility.Collapsed;
        }
    }
}
