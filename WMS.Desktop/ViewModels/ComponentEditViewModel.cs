using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Desktop.ViewModels
{
    public partial class ComponentEditViewModel : ObservableObject
    {
        private readonly ComponentService _componentService;
        private readonly DialogService _dialogService;
        private readonly ComponentDTO _currentComponentDto;
        private readonly bool _isEditMode;

        [ObservableProperty] private string _windowTitle;
        [ObservableProperty] private string _article = string.Empty;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _manufacturer = string.Empty;
        //[ObservableProperty] private DateTime expirationDate;
        [ObservableProperty] private int _minQuantity;

        public ComponentEditViewModel(
            ComponentService componentService, 
            DialogService dialogService,
            ComponentDTO componentDto = null)
        {
            _componentService = componentService;
            _currentComponentDto = componentDto;
            _dialogService = dialogService;
            _isEditMode = componentDto != null;

            WindowTitle = _isEditMode ? "Редактирование компонента" : "Добавление компонента";

            if (_isEditMode)
            {
                Article = componentDto.Article;
                Name = componentDto.Name;
                Manufacturer = componentDto.Manufacturer;
                //ExpirationDate = componentDto.ExpirationDate.ToDateTime(TimeOnly.MinValue);
                MinQuantity = componentDto.MinQuantity;
            }
        }

        [RelayCommand]
        private async Task Save(Window window)
        {
            try
            {
                Result result;

                if (_isEditMode)
                {
                    result = await _componentService.UpdateAsync(
                    new ComponentDTO(
                        _currentComponentDto.Id,
                        Name,
                        Article,
                        Manufacturer,
                        null, /*DateOnly.FromDateTime(ExpirationDate)*/
                        MinQuantity
                    ));
                }
                else
                {
                    result = await _componentService.AddAsync(
                        Name,
                        Article,
                        Manufacturer,
                        null, /*DateOnly.FromDateTime(ExpirationDate)*/
                        MinQuantity
                    );
                }

                if (result.IsSuccess)
                {
                    _dialogService.ShowInfo("Данные успешно сохранены.");
                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    _dialogService.ShowWarning(result.Error, "Предупреждение");
                }
            }
            catch (BusinessException domainEx)
            {
                _dialogService.ShowWarning(domainEx.Message, "Ошибка заполнения полей");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Критическая ошибка сохранения: {ex.Message}", "Ошибка системы");
            }
        }
    }
}
