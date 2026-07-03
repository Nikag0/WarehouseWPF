using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using WMS.Application.Abstractions;
using WMS.Domain;
using WMS.Domain.ExceptionControl;
namespace WMS.Application.Services
{
    public class NotificationService
    {
        private readonly IStockRepository _stockRepository;

        public NotificationService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IReadOnlyList<NotificationItem>> GetStockWithMinQuantity()
        {
            var deficitStocks = await _stockRepository.GetMinQuantityAsync();

            var notifications = deficitStocks
            .GroupBy(s => s.ComponentId)
            .Select(group =>
            {
                var firstStock = group.First();

                int totalQuantity = group.Sum(s => s.Quantity);

                var locations = group.Select(s =>
                    $"ст. {LocationFormatter.CodeToDisplay(s.Rack.Column, s.Rack.Row)} " +
                    $"яч. {LocationFormatter.CodeToDisplay(s.Cell.Column, s.Cell.Row)} ({s.Quantity} шт.)");

                return new NotificationItem(
                    title: $"Критический остаток: {firstStock.Component.Name}.",
                    description: $"Общий остаток: {totalQuantity} шт. (Минимум: {firstStock.Component.MinQuantity} шт.). " +
                                    $"Размещение: {string.Join("; ", locations)}",
                    type: NotificationType.LowStock
                );
            })
            .ToList();

            return notifications;
        }
    }
}
