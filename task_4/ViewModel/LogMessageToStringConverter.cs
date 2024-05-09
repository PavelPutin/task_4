using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using task_4.Model;

namespace task_4.ViewModel
{
    public class LogMessageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogMessage message = (LogMessage)value;
            if (message == null)
            {
                return "Сообщение неопределено";
            }
            string representation = "";
            representation += message.Timestamp + " ";
            representation += message.Sender + " ";
            representation += message.Message;
            return representation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
