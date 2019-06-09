// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using WinVVTools.InternalShared.Interactions;

namespace WinVVTools.InternalShared.Helpers.Converters
{
    public class DialogParentContentEnableConverter : ConverterBase<DialogParentContentEnableConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((MessageDialogState)value == MessageDialogState.Close);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
