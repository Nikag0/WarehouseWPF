using WMS.Domain.ExceptionControl;

namespace WMS.Domain
{
    public class OperationHistory
    {
        // Ограничение длины комментария (OperationHistoryConfiguration берёт его отсюда)
        public const int CommentMaxLength = 500;

        // Данные самой операции
        public Guid Id { get; private set; }
        public OperationType Type { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public string Operator { get; private set; } = null!;
        public string? Comment { get; private set; }

        // Данные компонента и адресация хранятся строками, чтобы история не зависела от других сущностей
        public string ComponentName { get; private set; } = null!;
        public string RackName { get; private set; } = null!;
        public string CellName { get; private set; } = null!;

        // Какое количество было до проведения операции
        public int PreviousQuantity { get; private set; }
        public int QuantityChanged { get; private set; }
        public int NewQuantity { get; private set; }

        private OperationHistory() { }

        private OperationHistory(
            OperationType type,
            string operatorName,
            string? comment,
            string componentName,
            string rackName,
            string cellName,
            int previousQuantity,
            int quantityChanged,
            int newQuantity)
        {
            Id = Guid.NewGuid();
            Type = type;
            OccurredAt = DateTime.UtcNow;
            Operator = operatorName;
            Comment = comment;

            ComponentName = componentName;
            RackName = rackName;
            CellName = cellName;

            PreviousQuantity = previousQuantity;
            QuantityChanged = quantityChanged;
            NewQuantity = newQuantity;
        }

        public static OperationHistory Create(
            OperationType type,
            string operatorName,
            string? comment,
            string componentName,
            string rackName,
            string cellName,
            int previousQuantity,
            int quantityChanged,
            int newQuantity)
        {
            if (!Enum.IsDefined(type))
                throw new BusinessException("Неизвестный тип операции");

            operatorName = RequireText(operatorName, "Оператор не указан");
            comment = NormalizeComment(comment);

            componentName = RequireText(componentName, "Название компонента не указано");
            rackName = RequireText(rackName, "Название стеллажа не указано");
            cellName = RequireText(cellName, "Название ячейки не указано");

            ValidateQuantities(type, previousQuantity, quantityChanged, newQuantity);

            return new OperationHistory(
                type,
                operatorName,
                comment,
                componentName,
                rackName,
                cellName,
                previousQuantity,
                quantityChanged,
                newQuantity
            );
        }

        // -------- Business rules --------

        private static string RequireText(string value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException(message);

            return value.Trim();
        }

        // Пустой комментарий храним как null
        private static string? NormalizeComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return null;

            comment = comment.Trim();

            if (comment.Length > CommentMaxLength)
                throw new BusinessException($"Комментарий не может быть длиннее {CommentMaxLength} символов");

            return comment;
        }

        private static void ValidateQuantities(OperationType type, int previousQuantity, int quantityChanged, int newQuantity)
        {
            if (previousQuantity < 0 || newQuantity < 0)
                throw new BusinessException("Количество не может быть отрицательным");

            if (quantityChanged <= 0)
                throw new BusinessException("Изменение количества должно быть больше нуля");

            var expectedQuantity = type == OperationType.Receipt
                ? previousQuantity + quantityChanged
                : previousQuantity - quantityChanged;

            if (newQuantity != expectedQuantity)
                throw new BusinessException("Количество после операции не сходится с изменением");
        }
    }
}
