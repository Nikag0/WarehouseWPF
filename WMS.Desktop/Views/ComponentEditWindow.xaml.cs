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
using System.Windows.Shapes;
using WMS.Desktop.ViewModels;

namespace WMS.Desktop.Views
{
    /// <summary>
    /// Логика взаимодействия для ComponentEditWindow.xaml
    /// </summary>
    public partial class ComponentEditWindow : Window
    {
        public ComponentEditWindow(ComponentEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }
    }
}
