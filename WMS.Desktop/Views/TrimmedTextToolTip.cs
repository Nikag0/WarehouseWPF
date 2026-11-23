using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WMS.Desktop.Views;

public static class TrimmedTextToolTip
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TrimmedTextToolTip),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not TextBlock textBlock)
            return;

        if (e.NewValue is false)
        {
            textBlock.ToolTipOpening -= OnToolTipOpening;
            textBlock.ClearValue(FrameworkElement.ToolTipProperty);
            return;
        }

        var content = new TextBlock { TextWrapping = TextWrapping.Wrap, MaxWidth = 420 };
        content.SetBinding(TextBlock.TextProperty, new Binding(nameof(TextBlock.Text)) { Source = textBlock });
        textBlock.ToolTip = new ToolTip { Content = content };
        textBlock.ToolTipOpening += OnToolTipOpening;
    }

    private static void OnToolTipOpening(object sender, ToolTipEventArgs e)
    {
        var textBlock = (TextBlock)sender;
        if (string.IsNullOrEmpty(textBlock.Text) || textBlock.ActualWidth <= 0)
        {
            e.Handled = true;
            return;
        }

        var text = new FormattedText(textBlock.Text, textBlock.Language.GetSpecificCulture(),
            textBlock.FlowDirection,
            new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
            textBlock.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

        var padding = textBlock.Padding;
        var availableWidth = textBlock.ActualWidth - padding.Left - padding.Right;
        var availableHeight = textBlock.ActualHeight - padding.Top - padding.Bottom;
        e.Handled = text.Width <= availableWidth + 0.5 && text.Height <= availableHeight + 0.5;
    }
}
