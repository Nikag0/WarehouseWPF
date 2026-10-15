using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application
{
    public static class MappingExtensionsDomain
    {
        public static string CodeToDisplay(int column, int row)
        {
            return $"{NumToLetter(row)}-{column}";
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
    }
}
