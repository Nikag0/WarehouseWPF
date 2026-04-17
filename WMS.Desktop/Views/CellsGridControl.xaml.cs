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
    }
}
