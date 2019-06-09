﻿// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using WinVVTools.InternalShared.Interactions;

namespace WinVVTools.InternalShared.Helpers.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public class DialogStateToVisibilityConverter : ConverterBase<DialogStateToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((MessageDialogState)value == MessageDialogState.Close)
                return System.Windows.Visibility.Collapsed;
            else
                return System.Windows.Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((System.Windows.Visibility)value == System.Windows.Visibility.Visible);
        }
    }
}
