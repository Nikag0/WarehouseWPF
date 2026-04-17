using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WMS.Desktop.Views
{
    public static class PopupHelper
    {
        public static void HandleOutsideClick(
             object sender,
             MouseButtonEventArgs e,
             Popup popup,
             FrameworkElement target,
             Action closeAction)
        {
            if (!popup.IsOpen)
                return;

            var clickedElement = e.OriginalSource as DependencyObject;

            // 1. Клик внутри TextBox
            if (IsDescendantOf(clickedElement, target))
                return;

            // 2. Клик внутри нашего Popup
            if (popup.Child != null && IsDescendantOf(clickedElement, popup.Child))
                return;

            // 3. Клик внутри любого Popup (ComboBox и др.)
            if (IsInsideAnyPopup(clickedElement))
                return;

            // иначе — закрываем
            closeAction();
        }
        private static bool IsInsideAnyPopup(DependencyObject element)
        {
            while (element != null)
            {
                if (element is System.Windows.Controls.Primitives.Popup)
                    return true;

                element = VisualTreeHelper.GetParent(element);
            }

            return false;
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject parent)
        {
            while (child != null)
            {
                if (child == parent)
                    return true;

                child = VisualTreeHelper.GetParent(child);
            }
            return false;
        }

        public static void HandleEscape(KeyEventArgs e, Action closeAction)
        {
            if (e.Key == Key.Escape)
            {
                closeAction();
                e.Handled = true;
            }
        }

    }
}
