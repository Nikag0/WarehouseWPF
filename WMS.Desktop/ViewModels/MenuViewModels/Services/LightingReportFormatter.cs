using WMS.Application.Services;

namespace WMS.Desktop.Services
{
    // Текст отчёта об ошибках инициализации подсветки для окна сообщения.
    // Один формат для запуска программы и для кнопки «Перезагрузить контроллеры».
    public static class LightingReportFormatter
    {
        public static string Describe(LightingInitReport report)
        {
            return $"Не удалось инициализировать контроллеры ({report.Errors.Count}).\n" +
                   "Проверьте питание и подключение к сети.";
        }
    }
}
