using System;
using System.Globalization;
using System.Windows.Data;

namespace ARALReadingSystem.Converters
{
    public class CheckInProgressConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is not int lastCompletedSession)
            {
                return "0 / 4";
            }

            int completedCheckIns = 0;

            if (lastCompletedSession >= 15)
                completedCheckIns++;

            if (lastCompletedSession >= 24)
                completedCheckIns++;

            if (lastCompletedSession >= 31)
                completedCheckIns++;

            if (lastCompletedSession >= 36)
                completedCheckIns++;

            return $"{completedCheckIns} / 4";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}