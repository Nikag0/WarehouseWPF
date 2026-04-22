using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WMS.Desktop.Views
{
    public partial class CellsGridControl : UserControl
    {
        public CellsGridControl()
        {
            InitializeComponent();
        }

        public event Action<CellViewModel>? CellClicked;

        private void RackBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is CellViewModel cellVm)
            {
                CellClicked?.Invoke(cellVm);
            }
        }

        public ObservableCollection<CellViewModel> Cells
        {
            get => (ObservableCollection<CellViewModel>)GetValue(CellsProperty);
            set => SetValue(CellsProperty, value);
        }

        public static readonly DependencyProperty CellsProperty =
            DependencyProperty.Register(
                nameof(Cells),
                typeof(ObservableCollection<CellViewModel>),
                typeof(CellsGridControl),
                new PropertyMetadata(null));

        public RackType RackType
        {
            get => (RackType)GetValue(RackTypeProperty);
            set => SetValue(RackTypeProperty, value);
        }

        public static readonly DependencyProperty RackTypeProperty =
            DependencyProperty.Register(
                nameof(RackType),
                typeof(RackType),
                typeof(CellsGridControl),
                new PropertyMetadata(RackType.RackType1));

        public static readonly DependencyProperty SelectedCellProperty =
            DependencyProperty.Register(
                nameof(SelectedCell),
                typeof(CellViewModel),
                typeof(CellsGridControl),
                new PropertyMetadata(null));

        public CellViewModel SelectedCell
        {
            get => (CellViewModel)GetValue(SelectedCellProperty);
            set => SetValue(SelectedCellProperty, value);
        }
    }
}
