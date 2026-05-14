using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.Abstractions
{
    public interface IWmsDataStore
    {
        ObservableCollection <ComponentDTO> Components { get; }

        Task InitializeAsync();
        Task RefreshComponentsAsync();
    }
}
