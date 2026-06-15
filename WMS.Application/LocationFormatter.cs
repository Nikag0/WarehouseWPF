public static class LocationFormatter
{
    public static string CodeToDisplay(int column, int row)
    {
        return $"{NumToLetter(column)}-{row}";
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

    public static string NumToOperation(int operationType)
    {
        return operationType switch
        {
            1 => "Приёмка",
            2 => "Выдача",
            3 => "Инвентаризация",
            _ => operationType.ToString()
        };
    }
}