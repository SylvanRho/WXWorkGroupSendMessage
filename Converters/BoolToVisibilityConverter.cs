using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WXWorkGroupSendMessage.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 转换函数
        /// </summary>
        /// <param name="parameter">只要有值就会被反转 - 相当于取反</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return ConvertFun(Visibility.Visible, parameter);
            }
            return ConvertFun(Visibility.Collapsed, parameter);
        }

        public object ConvertFun(Visibility visibility, object parameter)
        {
            //有command参数 取反值
            if (parameter is string)//取反值
            {
                if (visibility == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
