// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

namespace WinVVTools.InternalShared.Behaviour
{
    public interface ISettingsManager
    {
        /// <summary>
        /// Relative folder path to settings file.
        /// </summary>
        string FolderPath { get; }

        /// <summary>
        /// Setting file name with extension.
        /// </summary>
        string SettingsFile { get; }

        /// <summary>
        /// Define default settings.
        /// </summary>
        void SetDefault(IModuleSettings settings);

        /// <summary>
        /// Get setting by FilePath 
        /// </summary>
        (IModuleSettings settings, bool isError, string errorMessage) ReadSettingsFromFile();

        /// <summary>
        /// Save setting by FilePath
        /// </summary>
        (bool isError, string errorMessage) WriteSettingsToFile(IModuleSettings settings);
    }
}
