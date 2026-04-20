public static class LocationFormatter
{
    public static string CodeToDisplay(int column, int row)
    {
        return $"{ToLetter(column)}-{row}";
    }

    private static string ToLetter(int number)
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
    public static string DataToCode(int column, int row)
    {
        return $"{ToCode(column)}-{row}";
    }
    private static int ToCode(int number)
    {
        return number switch
        {
            1 => 1,
            2 => 1,
            3 => 1,
            4 => 1,
            5 => 1
        };
    }
}