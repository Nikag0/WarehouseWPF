using CommunityToolkit.Mvvm.ComponentModel;
using WMS.Application.DTO;

namespace WMS.Desktop.Models;

public sealed partial class IssueItemViewModel : ObservableObject
{
    [ObservableProperty] private StockViewDTO _stock;
    [ObservableProperty] private decimal _operationQuantity;

    public IssueItemViewModel(StockViewDTO stock)
    {
        _stock = stock;
    }
}
