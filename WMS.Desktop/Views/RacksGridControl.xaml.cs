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
using WMS.Domain;

namespace WMS.Desktop.Views
{
    public partial class RacksGridControl : UserControl
    {
        public RacksGridControl()
        {
            InitializeComponent();
        }

        public event Action<RackViewModel>? RackClicked;

        private void RackBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RackViewModel rackVm)
            {
                RackClicked?.Invoke(rackVm);
            }
        }

        public ObservableCollection<RackViewModel> Racks
        {
            get => (ObservableCollection<RackViewModel>)GetValue(RacksProperty);
            set => SetValue(RacksProperty, value);
        }

        public static readonly DependencyProperty RacksProperty =
        DependencyProperty.Register
            (nameof(Racks),
            typeof(ObservableCollection<RackViewModel>),
            typeof(RacksGridControl));

        public RackViewModel SelectedRack
        {
            get => (RackViewModel)GetValue(SelectedRackProperty);
            set => SetValue(SelectedRackProperty, value);
        }

        public static readonly DependencyProperty SelectedRackProperty =
           DependencyProperty.Register(
               nameof(SelectedRack),
               typeof(RackViewModel),
               typeof(RacksGridControl),
               new PropertyMetadata(null));

    }
}
