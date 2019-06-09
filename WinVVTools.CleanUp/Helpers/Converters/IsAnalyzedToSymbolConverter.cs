// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    /// <summary>
    /// The analyse/ignore object flag mapping converter
    /// </summary>
    internal class IsAnalyzedToSymbolConverter : ConverterBase<IsAnalyzedToSymbolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/WinVVTools.InternalShared;component/Resources/Icons.xaml", UriKind.Absolute);
            
            var symbol = (bool)value == true ? dict["plus"] : dict["minus"];
            return symbol;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
