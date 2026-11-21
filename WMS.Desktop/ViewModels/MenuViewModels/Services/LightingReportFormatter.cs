using System.Text;
using WMS.Application.Services;

namespace WMS.Desktop.Services
{
    // Текст отчёта об ошибках инициализации подсветки для окна сообщения.
    // Один формат для запуска программы и для кнопки «Перезагрузить контроллеры».
    public static class LightingReportFormatter
    {
        public static string Describe(LightingInitReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Подсветка инициализирована с ошибками. Ряды с ошибкой горят красным.");
            sb.AppendLine("Проверьте, что платы включены и подключены к сети.");
            sb.AppendLine();

            foreach (var error in report.Errors)
                sb.AppendLine($"• {error}");

            return sb.ToString().TrimEnd();
        }
    }
}
