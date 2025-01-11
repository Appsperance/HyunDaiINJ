using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.Services
{
    public class StringToIntConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // int -> string 변환
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // string -> int 변환
            if (int.TryParse(value as string, out var result))
            {
                return result;
            }
            return 0; // 기본값
        }
    }
}
