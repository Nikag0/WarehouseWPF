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
using WMS.Domain;

namespace WMS.Desktop.Views
{
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
                await viewModel.LoadWindow();
            }
        }

        private void SearchRack_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DataContext is ReceiptViewModel vm)
            {
                vm.IsRackPopupOpen = true;
            }
        }

        private void SearchCell_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DataContext is ReceiptViewModel vm)
            {
                vm.IsCellPopupOpen = true;
            }
        }

        private void SearchRack_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ReceiptViewModel vm)
            {
                PopupHelper.HandleEscape(e, () =>
                {
                    vm.IsRackPopupOpen = false;
                    Keyboard.ClearFocus();
                });
            }
        }

        private void SearchCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ReceiptViewModel vm)
            {
                PopupHelper.HandleEscape(e, () =>
                {
                    vm.IsCellPopupOpen = false;
                    Keyboard.ClearFocus();
                });
            }
        }

        private void Root_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ReceiptViewModel vm)
                return;

            PopupHelper.HandleOutsideClick(
                sender,
                e,
                RackPopup,
                SearchRack,
                () =>
                {
                     vm.IsRackPopupOpen = false;
                     Keyboard.ClearFocus();
                });

            PopupHelper.HandleOutsideClick(
                sender,
                e,
                CellPopup,
                SearchCell,
                () =>
                {
                    vm.IsCellPopupOpen = false;
                    Keyboard.ClearFocus();
                });
        }
    }
}
