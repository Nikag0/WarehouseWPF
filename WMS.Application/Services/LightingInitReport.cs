namespace WMS.Application.Services
{
    // Итог инициализации подсветки. Errors — понятные человеку строки: какой контроллер и на каком шаге не ответил.
    public sealed class LightingInitReport
    {
        public static LightingInitReport NotConfiguredReport { get; } = new(isConfigured: false, Array.Empty<string>());

        public LightingInitReport(bool isConfigured, IReadOnlyList<string> errors)
        {
            IsConfigured = isConfigured;
            Errors = errors;
        }

        // false — в базе нет ни одной зоны подсветки, контроллерам нечего отправлять
        public bool IsConfigured { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsSuccess => IsConfigured && Errors.Count == 0;
    }
}
