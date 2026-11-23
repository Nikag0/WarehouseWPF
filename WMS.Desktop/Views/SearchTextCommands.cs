using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace WMS.Desktop.Views;

public static class SearchTextCommands
{
    public static ICommand Clear { get; } = new RelayCommand<TextBox>(field =>
    {
        if (field is null)
            return;

        field.Focus();
        field.SetCurrentValue(TextBox.TextProperty, string.Empty);
        field.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    });
}
