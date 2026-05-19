using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WMS.Application.Services;
using WMS.Domain;
using WMS.Domain.ExceptionControl;

namespace WMS.Desktop.ViewModels
{
    public partial class ComponentEditViewModel : ObservableObject
    {
        private readonly ComponentService _componentService;
        private readonly ComponentDTO _currentComponentDto;
        private readonly bool _isEditMode;

        [ObservableProperty] private string _windowTitle;
        [ObservableProperty] private string _article = string.Empty;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _manufacturer = string.Empty;

        public ComponentEditViewModel(ComponentService componentService, ComponentDTO componentDto = null)
        {
            _componentService = componentService;
            _currentComponentDto = componentDto;
            _isEditMode = componentDto != null;

            WindowTitle = _isEditMode ? "Редактирование компонента" : "Добавление компонента";

            if (_isEditMode)
            {
                Article = componentDto.Article;
                Name = componentDto.Name;
                Manufacturer = componentDto.Manufacturer;
            }
        }

        [RelayCommand]
        private async Task Save(Window window)
        {
            try
            {
                if (_isEditMode)
                {
                    var entity = await _componentService.GetByIdAsync(_currentComponentDto.Id);
                    entity.SetArticle(Article);
                    entity.SetName(Name);
                    entity.SetManufacturer(Manufacturer);
                    await _componentService.UpdateAsync(entity);
                }
                else
                {
                    // Для создания используем фабричный метод модели. 
                    // (Срок годности ставим null, минимальное количество 0 — подставьте нужные дефолтные значения)
                    var newComponent = Component.Create(Article, Name, Manufacturer, null, 0);

                    // Передаем готовую валидную доменную сущность в БД
                    await _componentService.AddAsync(
                        newComponent.Article,
                        newComponent.Name,
                        newComponent.Manufacturer);
                }

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (OverallDomainException domainEx)
            {
                MessageBox.Show(domainEx.Message, "Ошибка заполнения полей", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
