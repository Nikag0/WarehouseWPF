public static class LocationFormatter
{
    public static string ToRackCode(int row, int rackNum)
    {
        return $"{ToLetter(row)}-{rackNum}";
    }

    public static string ToCellCode(int line, int column)
    {
        return $"{ToLetter(line)}-{column}";
    }

    private static string ToLetter(int number)
    {
        return number switch
        {
            1 => "A",
            2 => "B",
            3 => "C",
            4 => "D",
            5 => "E",
            6 => "F",
            7 => "G",
            8 => "H",
            _ => number.ToString()
        };
    }
}