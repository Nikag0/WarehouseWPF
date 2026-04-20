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
            this.Loaded += IssueViewLoaded;
            RacksGrid.RackClicked += OnRackClicked;
        }

        private async void IssueViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IssueViewModel viewModel)
            {
                await viewModel.RefreshAsync();
                await viewModel.LoadOperators();
                await viewModel.LoadWarehouseVisualise();
            }
        }

        private void OnRackClicked(RackViewModel rackVm)
        {
            if (DataContext is not IssueViewModel vm)
                return;

            vm.SelectedRack = rackVm;

            int column = rackVm.Column;
            int row = rackVm.Row;

            if (column == 3 && row != 4)
                vm.CurrentCellType = CellsType.Cell1;

            else if ((column == 1 || column == 2 || column == 4) && row != 4)
                vm.CurrentCellType = CellsType.Cell2;

            else if (row == 4)
                vm.CurrentCellType = CellsType.Cell3;
        }

        private void SearchToIssue_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DataContext is IssueViewModel vm)
            {
                vm.IsIssuePopupOpen = true;
            }
        }

        private void SearchToIssue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is IssueViewModel vm)
            {
                PopupHelper.HandleEscape(e, () =>
                {
                    vm.IsIssuePopupOpen = false;
                    Keyboard.ClearFocus();
                });
            }
        }

        private void Root_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not IssueViewModel vm)
                return;

            PopupHelper.HandleOutsideClick(
                sender,
                e,
                IssuePopup,
                SearchToIssue,
                () =>
                {
                    vm.IsIssuePopupOpen = false;
                    Keyboard.ClearFocus();
                });
        }
    }
}
