using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace TextPipeline.Converters
{
    /// <summary>
    /// Конвертер для текста рядом с индикатором.
    /// parameter — подпись ("Предусловие" или "Постусловие").
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var label = parameter as string ?? "";
            var ok = value is bool b && b;
            return $"{label}: {(ok ? "выполнено" : "не выполнено")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
