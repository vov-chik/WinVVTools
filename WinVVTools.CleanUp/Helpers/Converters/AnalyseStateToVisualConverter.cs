// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    internal class AnalyseStateToVisualConverter : ConverterBase<AnalyseStateToVisualConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/WinVVTools.InternalShared;component/Resources/Icons.xaml", UriKind.Absolute);
            
            switch ((AnalyseState)value)
            {
                case AnalyseState.Completed:
                    return dict["check"];
                case AnalyseState.Interrupted:
                    return dict["close"];
                default:
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
