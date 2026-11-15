namespace WMS.Application.Services
{
    // Результат отправки зон подсветки контроллерам
    public enum ControllersInitResult
    {
        // Все контроллеры приняли зоны подсветки
        Success,

        // В базе нет ни одной зоны подсветки — контроллерам нечего отправлять
        NotConfigured,

        // Хотя бы один контроллер не ответил или отклонил команду
        Failed
    }
}
