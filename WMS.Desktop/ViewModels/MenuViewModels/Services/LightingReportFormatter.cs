using WMS.Application.Services;

namespace WMS.Desktop.Services
{
    // Текст отчёта об ошибках инициализации подсветки для окна сообщения.
    // Один формат для запуска программы и для кнопки «Перезагрузить контроллеры».
    public static class LightingReportFormatter
    {
        public static string Describe(LightingInitReport report)
        {
            var affected = report.AffectedRows.Count == 0
                ? "Часть подсветки может не работать."
                : $"Подсветка может не работать в следующих рядах: {string.Join(", ", report.AffectedRows)}.";

            return affected + "\nПроверьте питание и подключение к сети.";
        }
    }
}
