using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace WMS.Application.Services
{
    // Единый формат выгрузки таблиц в CSV: разделитель «;» и UTF-8 с BOM, чтобы Excel сразу открывал файл с русскими буквами.
    // Заголовки колонок задаются атрибутом [Name] в DTO.
    public static class CsvExporter
    {
        private static readonly CsvConfiguration Config = new(CultureInfo.CurrentCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };

        // Сначала пишет во временный файл и только после успешной записи заменяет им целевой.
        // Если выгрузка оборвалась (ошибка БД, отмена), целевой файл не будет обрезанным или пустым.
        public static async Task WriteAsync<T>(string filePath, IAsyncEnumerable<T> records, CancellationToken ct = default)
        {
            var tempPath = filePath + ".tmp";

            try
            {
                await using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
                await using (var csv = new CsvWriter(writer, Config))
                {
                    await csv.WriteRecordsAsync(records, ct);
                }

                File.Move(tempPath, filePath, overwrite: true);
            }
            catch
            {
                TryDelete(tempPath);
                throw;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // Не удалось убрать временный файл — исходная ошибка важнее
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
