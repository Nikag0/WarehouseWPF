using System.Windows;
using WMS.Desktop.Models;

namespace WMS.Desktop.Views;

public partial class IssueQuantityWarning : Window
{
    public IssueQuantityWarning(IReadOnlyList<IssueItemViewModel> items)
    {
        InitializeComponent();
        DataContext = string.Join("\n\n", items.Select(item =>
            $"{item.Stock.ComponentName}\nСтеллаж {item.Stock.RackCodeDisplay} · ячейка {item.Stock.CellCodeDisplay}"));
    }
}
