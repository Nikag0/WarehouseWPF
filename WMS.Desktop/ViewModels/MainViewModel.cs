using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMS.Application.Services;

namespace WMS.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IssueService _issueService;
        public ICommand IssueCommand { get; }

        public MainViewModel(IssueService issueService)
        {
            _issueService = issueService;
            IssueCommand = new RelayCommand(IssueTest);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task IssueTest()
        {
            await _issueService.IssueAsync(new[]
            {
            new StockOperationDto(
                ComponentId: Guid.Parse("..."),
                CellId: Guid.Parse("..."),
                Quantity: 2)
        });
        }
    }

}
