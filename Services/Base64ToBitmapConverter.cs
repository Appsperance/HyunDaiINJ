using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HyunDaiINJ.Services
{
    public class Base64ToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value가 Base64 문자열이라 가정
            if (value is string base64 && !string.IsNullOrEmpty(base64))
            {
                return Base64ImageHelper.ConvertBase64ToBitmapImage(base64);
            }
            return null;
        }

        // 이미지 → Base64 역변환은 사용하지 않으므로 미구현
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
