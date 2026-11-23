using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WMS.Desktop.ViewModels;
using WMS.Desktop.ViewModels.MenuViewModels;

namespace WMS.Desktop.Views.MenuView
{
    public partial class ComponentsView : UserControl
    {
        public ComponentsView()
        {
            InitializeComponent();
            BindingOperations.SetBinding(ActionsColumn, DataGridColumn.VisibilityProperty,
                new Binding("DataContext.IsEditing")
                {
                    Source = CatalogGrid,
                    Converter = new BooleanToVisibilityConverter()
                });
            this.Loaded += ComponentViewLoaded;
        }

        private async void ComponentViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ComponentsViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }
    }
}
