using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyunDaiINJ.Services
{
    public class MaxDailyQuantityRule : ValidationRule
    {
        public int MaxAllowed { get; set; }  // 예: 800

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // TextBox에서 입력된 string → int 변환
            if (int.TryParse(value?.ToString(), out int newValue))
            {
                if (newValue > MaxAllowed)
                {
                    return new ValidationResult(false, $"최대 {MaxAllowed}까지 입력 가능합니다.");
                }
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "숫자가 아닙니다.");
        }
    }
}
