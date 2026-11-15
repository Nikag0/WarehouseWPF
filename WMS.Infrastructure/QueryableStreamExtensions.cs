using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace WMS.Infrastructure
{
    internal static class QueryableStreamExtensions
    {
        // Построчное чтение запроса (для выгрузки в CSV без загрузки всей таблицы в память) с поддержкой отмены.
        // Запрос строится сразу в репозитории, пока доступен контекст текущего UnitOfWork, а выполняется при чтении.
        public static async IAsyncEnumerable<T> StreamAsync<T>(
            this IQueryable<T> query,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var item in query.AsAsyncEnumerable().WithCancellation(ct))
                yield return item;
        }
    }
}
