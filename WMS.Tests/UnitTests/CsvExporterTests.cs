using CsvHelper.Configuration.Attributes;
using System.Text;
using WMS.Application.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace WMS.Test.UnitTests;

public class CsvExporterTests : IDisposable
{
    public record Row([property: Name("Наименование")] string Name, [property: Name("Количество")] int Quantity);

    private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"csv-exporter-{Guid.NewGuid()}.csv");

    public void Dispose()
    {
        File.Delete(_filePath);
        File.Delete(_filePath + ".tmp");
    }

    private static async IAsyncEnumerable<Row> Rows(params Row[] rows)
    {
        foreach (var row in rows)
        {
            await Task.Yield();
            yield return row;
        }
    }

    // Имитирует обрыв выгрузки (например, пропала связь с БД) после первой строки
    private static async IAsyncEnumerable<Row> FailingRows()
    {
        await Task.Yield();
        yield return new Row("Резистор", 1);
        throw new InvalidOperationException("БД недоступна");
    }

    [Fact]
    public async Task WriteAsync_WritesHeaderFromNameAttributesAndRows()
    {
        await CsvExporter.WriteAsync(_filePath, Rows(new Row("Резистор", 10), new Row("Диод", 3)));

        var lines = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8);
        Assert.Equal(new[] { "Наименование;Количество", "Резистор;10", "Диод;3" }, lines);
    }

    [Fact]
    public async Task WriteAsync_WritesUtf8Bom_SoExcelReadsCyrillic()
    {
        await CsvExporter.WriteAsync(_filePath, Rows(new Row("Резистор", 1)));

        var bytes = await File.ReadAllBytesAsync(_filePath);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3));
    }

    [Fact]
    public async Task WriteAsync_NoRows_WritesOnlyHeader()
    {
        await CsvExporter.WriteAsync(_filePath, Rows());

        Assert.Equal(new[] { "Наименование;Количество" }, await File.ReadAllLinesAsync(_filePath));
    }

    [Fact]
    public async Task WriteAsync_ReplacesExistingFile()
    {
        await File.WriteAllTextAsync(_filePath, "старое содержимое");

        await CsvExporter.WriteAsync(_filePath, Rows(new Row("Диод", 3)));

        Assert.Equal(new[] { "Наименование;Количество", "Диод;3" }, await File.ReadAllLinesAsync(_filePath));
    }

    [Fact]
    public async Task WriteAsync_SourceFails_KeepsExistingFileAndRemovesTemp()
    {
        await File.WriteAllTextAsync(_filePath, "старое содержимое");

        await AssertSourceErrorAsync(() => CsvExporter.WriteAsync(_filePath, FailingRows()));

        Assert.Equal("старое содержимое", await File.ReadAllTextAsync(_filePath));
        Assert.False(File.Exists(_filePath + ".tmp"));
    }

    [Fact]
    public async Task WriteAsync_SourceFails_DoesNotCreateFile()
    {
        await AssertSourceErrorAsync(() => CsvExporter.WriteAsync(_filePath, FailingRows()));

        Assert.False(File.Exists(_filePath));
    }

    // CsvHelper оборачивает ошибку источника в WriterException, исходная остаётся внутри
    private static async Task AssertSourceErrorAsync(Func<Task> action)
    {
        var ex = await Assert.ThrowsAnyAsync<Exception>(action);
        Assert.IsType<InvalidOperationException>(ex.GetBaseException());
    }

    [Fact]
    public async Task WriteAsync_TargetLockedByAnotherProgram_ThrowsFileAccessErrorAndKeepsFile()
    {
        await File.WriteAllTextAsync(_filePath, "старое содержимое");

        // Так файл держит открытым Excel. Windows сообщает об этом как IOException или UnauthorizedAccessException.
        using (new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => CsvExporter.WriteAsync(_filePath, Rows(new Row("Диод", 3))));
            Assert.True(ex is IOException or UnauthorizedAccessException, $"Неожиданная ошибка: {ex.GetType()}");
        }

        Assert.Equal("старое содержимое", await File.ReadAllTextAsync(_filePath));
        Assert.False(File.Exists(_filePath + ".tmp"));
    }
}
