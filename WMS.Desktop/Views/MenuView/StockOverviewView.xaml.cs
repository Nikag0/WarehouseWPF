using System.Windows;
using System.Windows.Controls;
using WMS.Desktop.ViewModels.MenuViewModels;

namespace WMS.Desktop.Views.MenuView;

public partial class StockOverviewView : UserControl
{
    public StockOverviewView() => InitializeComponent();

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is StockOverviewViewModel viewModel)
            await viewModel.LoadAsync();
    }
}
