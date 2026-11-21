using WMS.Domain.ExceptionControl;

namespace WMS.Domain;

// Общие правила для всех количеств (остаток, приёмка, выдача, история, минимальный остаток).
// Количество хранится как decimal: double даёт ошибки округления (1.1 - 1.0 = 0.10000000000000009).
// Precision и Scale используются в конфигурациях EF, чтобы колонки в БД были numeric(18,3).
public static class QuantityRules
{
    public const int Precision = 18;
    public const int Scale = 3;

    // Максимум, который помещается в numeric(18,3)
    public const decimal MaxValue = 999_999_999_999_999.999m;

    // Формат для вывода: до трёх знаков после запятой, без лишних нулей (5.000 -> "5", 1.250 -> "1,25")
    public const string DisplayFormat = "0.###";

    public static void EnsureValid(decimal quantity)
    {
        if (decimal.Round(quantity, Scale) != quantity)
            throw new BusinessException($"Количество может содержать не более {Scale} знаков после запятой");

        if (Math.Abs(quantity) > MaxValue)
            throw new BusinessException("Количество слишком большое");
    }
}
