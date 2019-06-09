// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    internal class AnalyseTypeToVisibilityConverter : ConverterBase<AnalyseTypeToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (AnalyseType)value == AnalyseType.Off ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            
            return (AnalyseType)value == (AnalyseType)parameter ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}