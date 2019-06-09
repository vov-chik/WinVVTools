// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    internal class AnalyseStateDescriptionConverter : ConverterBase<AnalyseStateDescriptionConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((AnalyseState)value)
            {
                case AnalyseState.Off:
                    return "<Ожидается>";
                case AnalyseState.Started:
                    return "";
                case AnalyseState.Processing:
                    return "<Выполняется>";
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
