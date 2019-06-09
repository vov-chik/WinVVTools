// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using System;
using System.IO;
using WinVVTools.InternalShared.Behaviour;
using WinVVTools.InternalShared.Helpers;

namespace WinVVTools.Models
{
    internal class SettingsManager : ISettingsManager
    {
        public string FolderPath { get; } = @"\";
        public string SettingsFile { get; } = @"settings.json";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void SetDefault(IModuleSettings settings)
        {
            var moduleSettings = settings as ModuleSettings;
            if (moduleSettings == null)
                throw new ArgumentNullException(nameof(moduleSettings));

            moduleSettings.AppTheme = "BaseLight";
            moduleSettings.AppAccent = "Amber";
            //moduleSettings.Language = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (IModuleSettings settings, bool isError, string errorMessage) ReadSettingsFromFile()
        {
            string path = Directory.GetCurrentDirectory() + FolderPath + SettingsFile;

            if (!File.Exists(path))
                return (null, false, $"Файл '{path}' не найден!"); //Not Error!

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string json = sr.ReadToEnd();
                    var settings = JsonConvert.DeserializeObject<ModuleSettings>(json);
                    
                    return (settings, false, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (null, true, ex.GetFullMessage());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public (bool isError, string errorMessage) WriteSettingsToFile(IModuleSettings settings)
        {
            bool isError = false;
            string errorMessage = string.Empty;

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                using (StreamWriter sw = new StreamWriter(folder + SettingsFile, false))
                {
                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    sw.Write(json);
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }

            return (isError, errorMessage);
        }
    }
}
