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
using WMS.Desktop.ViewModels.MenuViewModels;

namespace WMS.Desktop.Views.MenuView
{
    /// <summary>
    /// Interaction logic for NotificationView.xaml
    /// </summary>
    public partial class NotificationView : System.Windows.Controls.UserControl
    {
        public NotificationView()
        {
            InitializeComponent();
            this.Loaded += NotificationViewLoaded;
        }

        private async void NotificationViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NotificationViewModel viewModel)
            {
                await viewModel.LoadNotification();
            }
        }
    }
}
