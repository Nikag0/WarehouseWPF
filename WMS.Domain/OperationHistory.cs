using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain.ExceptionControl;

namespace WMS.Domain
{
    public class OperationHistory
    {
        // Данные самой операции
        public Guid Id { get; private set; }
        public OperationType Type { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public string Operator { get; private set; } = null!;
        public string? Comment { get; private set; }

        // Данные конкретного компонента
        public Guid? ComponentId { get; private set; }
        public string ComponentName { get; private set; } = null!;

        // Адресация
        public Guid RackId { get; private set; }
        public string RackName { get; private set; } = null!;

        public Guid CellId { get; private set; }
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
            Guid componentId,
            string componentName,
            Guid rackId,
            string rackName,
            Guid cellId,
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

            ComponentId = componentId;
            ComponentName = componentName;

            RackId = rackId;
            RackName = rackName;

            CellId = cellId;
            CellName = cellName;

            PreviousQuantity = previousQuantity;
            QuantityChanged = quantityChanged;
            NewQuantity = newQuantity;
        }

        public static OperationHistory Create(
            OperationType type,
            string operatorName,
            string? comment,
            Guid Idcomponent,
            string componentName,
            Guid rackId,
            string rackName,
            Guid cellId,
            string cellName,
            int previousQuantity,
            int quantityChanged,
            int newQuantity)
        {

            return new OperationHistory(
                type,
                operatorName,
                comment,
                Idcomponent,
                componentName,
                rackId,
                rackName,
                cellId,
                cellName,
                previousQuantity,
                quantityChanged,
                newQuantity
            );
        }
    }
}
