// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Helpers.Converters;

namespace WinVVTools.CleanUp.Helpers.Converters
{
    /// <summary>
    /// The analyse object type converter into an image from resources
    /// </summary>
    internal class AnalyseObjectTypeToVisualConverter : ConverterBase<AnalyseObjectTypeToVisualConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/WinVVTools.InternalShared;component/Resources/Icons.xaml", UriKind.Absolute);
            
            switch ((AnalyseObjectType)value)
            {
                case AnalyseObjectType.OsDisk:
                    return dict["os_windows"];
                case AnalyseObjectType.HardDisk:
                    return dict["harddisk"];
                case AnalyseObjectType.RemovableDisk:
                    return dict["appbar_usb"];
                case AnalyseObjectType.NetworkDisk:
                    return dict["network"];
                case AnalyseObjectType.CDRom:
                    return dict["disc_player"];
                case AnalyseObjectType.Folder:
                    return dict["folder"];
                case AnalyseObjectType.Registry:
                    return dict["appbar_tiles_nine"];
                case AnalyseObjectType.Unknown:
                    return dict["help_rhombus"];
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
