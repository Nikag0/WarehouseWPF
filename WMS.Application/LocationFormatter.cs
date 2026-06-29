using System.Reflection.Emit;
using WMS.Domain;

public static class LocationFormatter
{
    public static string CodeToDisplay(int column, int row)
    {
        return $"{column}-{NumToLetter(row)}";
    }

    public static string NumToLetter(int number)
    {
        return number switch
        {
            1 => "А",
            2 => "Б",
            3 => "В",
            4 => "Г",
            5 => "Д",
            6 => "Е",
            7 => "Ж",
            8 => "З",
            _ => number.ToString()
        };
    }

    public static string NumToOperation(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Receipt => "Приёмка",
            OperationType.Issue => "Выдача",
            _ => operationType.ToString()
        };
    }
}