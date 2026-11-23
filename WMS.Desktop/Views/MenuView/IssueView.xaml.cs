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
using WMS.Desktop.ViewModels.MenuViewModels;
using WMS.Desktop.Models;
using Xceed.Wpf.Toolkit;
using WMS.Domain;

namespace WMS.Desktop.Views.MenuView
{
    /// <summary>
    /// Interaction logic for IssueViews.xaml
    /// </summary>
    public partial class IssueView : UserControl
    { 
        private IssueViewModel? _loadedViewModel;

        public IssueView()
        {
            InitializeComponent();
            this.Loaded += IssueViewLoaded;
            Unloaded += (_, _) =>
            {
                if (_loadedViewModel is not null)
                    _loadedViewModel.QuantityFocusRequested -= FocusQuantity;
                _loadedViewModel = null;
            };
        }

        private async void IssueViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IssueViewModel viewModel)
            {
                _loadedViewModel = viewModel;
                viewModel.QuantityFocusRequested += FocusQuantity;
                await viewModel.LoadDataAsync();
            }
        }

        private void FocusQuantity(IssueItemViewModel item)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
            {
                if (!IsLoaded || !IssueGrid.Items.Contains(item))
                    return;

                IssueGrid.ScrollIntoView(item, QuantityColumn);
                IssueGrid.UpdateLayout();
                if (QuantityColumn.GetCellContent(item) is DependencyObject content &&
                    FindQuantityEditor(content) is { } editor)
                {
                    editor.ApplyTemplate();
                    if (editor.Template?.FindName("PART_TextBox", editor) is TextBox input && input.Focus())
                        input.SelectAll();
                    else
                        editor.Focus();
                }
            }));
        }

        private static DecimalUpDown? FindQuantityEditor(DependencyObject element)
        {
            if (element is DecimalUpDown editor)
                return editor;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                if (FindQuantityEditor(VisualTreeHelper.GetChild(element, i)) is { } childEditor)
                    return childEditor;
            }

            return null;
        }

        private void SearchToIssue_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (DataContext is IssueViewModel vm)
            {
                vm.IsIssuePopupOpen = vm.FilteredStocks.Count > 0;
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
