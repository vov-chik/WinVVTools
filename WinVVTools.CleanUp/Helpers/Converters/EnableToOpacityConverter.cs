// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    internal class EnableToOpacityConverter : ConverterBase<EnableToOpacityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return 1;

            return (bool)value == true ? 1 : parameter;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}