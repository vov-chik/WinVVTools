// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;

namespace WinVVTools.InternalShared.Helpers.Converters
{
    public class ReverseBoolConverter : ConverterBase<ReverseBoolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
