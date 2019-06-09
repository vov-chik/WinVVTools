// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinVVTools.CleanUp.Helpers;
using WinVVTools.InternalShared.Behaviour;
using WinVVTools.InternalShared.Helpers;

namespace WinVVTools.CleanUp.Models
{
    internal class SettingsManager : ISettingsManager
    {
        public string FolderPath { get; } = @"\Data\CleanUp\";
        public string SettingsFile { get; } = @"settings.json";

        private const string _checkPointFileExtension = ".chpt";
        private const string _installationFileExtension = ".ipt";
        private const string _checkPointDiskFilePrefix = "Disk_";
        private const string _checkPointRegistryFilePrefix = "Reg_";

        /// <summary>
        /// 
        /// </summary>
        public SettingsManager()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void SetDefault(IModuleSettings settings)
        {
            var moduleSettings = settings as ModuleSettings;
            if (moduleSettings == null)
                throw new ArgumentNullException(nameof(moduleSettings));

            moduleSettings.CheckPoint = null;
            SetDefaultDisks(moduleSettings);
            SetDefaultFolders(moduleSettings);
            SetDefaultRegistries(moduleSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        private void SetDefaultDisks(ModuleSettings settings)
        {
            GetAccessibleDisks(settings);
            foreach (var analysedDisk in settings.AnalysedDisks)
            {
                if (analysedDisk.Type == AnalyseObjectType.OsDisk)
                    analysedDisk.IsAnalyzed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        private void GetAccessibleDisks(ModuleSettings settings)
        {
            settings.AnalysedDisks.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady)
                    settings.AnalysedDisks.Add(new AnalyseObject(d.DriveType, d.Name, false));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        private void SetDefaultFolders(ModuleSettings settings)
        {
            settings.AnalysedFolders.Clear();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            settings.AnalysedFolders.Add(new AnalyseObject(AnalyseObjectType.Folder, path, true));

            path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            settings.AnalysedFolders.Add(new AnalyseObject(AnalyseObjectType.Folder, path, true));

            path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            settings.AnalysedFolders.Add(new AnalyseObject(AnalyseObjectType.Folder, path, true));

            settings.AnalysedFolders.Sort(f => f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        private void SetDefaultRegistries(ModuleSettings settings)
        {
            settings.AnalysedRegistries.Clear();
            settings.AnalysedRegistries.Add(new AnalyseObject(AnalyseObjectType.Registry, @"HKEY_CLASSES_ROOT\", true));
            settings.AnalysedRegistries.Add(new AnalyseObject(AnalyseObjectType.Registry, @"HKEY_CURRENT_USER\Software\", true));
            settings.AnalysedRegistries.Add(new AnalyseObject(AnalyseObjectType.Registry, @"HKEY_LOCAL_MACHINE\Software\", true));
            settings.AnalysedRegistries.Add(new AnalyseObject(AnalyseObjectType.Registry, @"HKEY_USERS\.Default\", true));

            settings.AnalysedRegistries.Sort(f => f);
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

                    settings.AnalysedFolders.Sort(f => f);
                    settings.AnalysedRegistries.Sort(f => f);

                    //Fill in non-conserved settings

                    foreach (var f in settings.AnalysedFolders)
                        f.Type = AnalyseObjectType.Folder;

                    foreach (var r in settings.AnalysedRegistries)
                        r.Type = AnalyseObjectType.Registry;

                    var analysedDiskNames = settings.AnalysedDisks.Where(i => i.IsAnalyzed == true).Select(i => i.Name).ToArray();
                    settings.AnalysedDisks.Clear();
                    GetAccessibleDisks(settings);
                    var readyDisks = settings.AnalysedDisks.Where(i => analysedDiskNames.Contains(i.Name));
                    foreach (var d in readyDisks)
                        d.IsAnalyzed = true;

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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private string GetCheckPointFileFullName(AnalyseObjectType objectType, string objectName, string folder = null)
        {
            StringBuilder sbFileName = new StringBuilder();
            if (folder == null)
            {
                sbFileName.Append(Directory.GetCurrentDirectory()).Append(FolderPath);
            }
            else
            {
                sbFileName.Append(folder);
            }

            if (objectType == AnalyseObjectType.Registry)
            {
                sbFileName.Append(_checkPointRegistryFilePrefix).Append(objectName);
            }
            else
            {
                sbFileName.Append(_checkPointDiskFilePrefix).Append(objectName.FirstOrDefault());
            }
            sbFileName.Append(_checkPointFileExtension);

            return sbFileName.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpObject"></param>
        /// <param name="pathes"></param>
        /// <returns></returns>
        public (bool isError, string errorMessage) SaveCheckPointObjectsToFile(AnalyseObject cpObject, IEnumerable<string> pathes)
        {
            bool isError = false;
            string errorMessage = string.Empty;

            //Empty file is not saved
            if (!pathes.Any())
                return (isError, errorMessage);

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string file = GetCheckPointFileFullName(cpObject.Type, cpObject.Name, folder);
                File.WriteAllLines(file, pathes);
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }
            
            return (isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (bool isError, string errorMessage) DeleteCheckPointDataFiles()
        {
            bool isError = false;
            string errorMessage = string.Empty;

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath;
                if (!Directory.Exists(folder))
                    return (isError, errorMessage);

                string[] files = Directory.GetFiles(folder, $"*{_checkPointFileExtension}");
                foreach (string file in files)
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }
            return (isError, errorMessage);
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        public (bool isError, string errorMessage) CountCheckPointObjects(CheckPoint checkPoint)
        {
            bool isError = false;
            string errorMessage = string.Empty;

            int folderCount = 0;
            int fileCount = 0;
            int registryCount = 0;

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath;
                string[] files = Directory.GetFiles(folder, $"*{_checkPointFileExtension}");
                Parallel.ForEach(files, file =>
                {
                    using (StreamReader srFile = new StreamReader(file))
                    {
                        string line;

                        if (file.Split('\\').Last().StartsWith(_checkPointRegistryFilePrefix))
                        {
                            while ((line = srFile.ReadLine()) != null && !String.IsNullOrWhiteSpace(line))
                                Interlocked.Increment(ref registryCount);
                        }
                        else
                        {
                            while ((line = srFile.ReadLine()) != null && !String.IsNullOrWhiteSpace(line))
                            {
                                if (line.EndsWith("\\"))
                                    Interlocked.Increment(ref folderCount);
                                else
                                    Interlocked.Increment(ref fileCount);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }

            if (!isError)
            {
                checkPoint.FolderCount = folderCount;
                checkPoint.FileCount = fileCount;
                checkPoint.RegistryCount = registryCount;
            }
            return (isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpObject"></param>
        /// <returns></returns>
        public (IEnumerable<string> pathes, bool isError, string errorMessage) GetCheckPointObjectPathes(AnalyseObject cpObject)
        {
            bool isError = false;
            string errorMessage = string.Empty;
            List<string> pathes = new List<string>();

            try
            {
                string file = GetCheckPointFileFullName(cpObject.Type, cpObject.Name);
                if (File.Exists(file))
                    pathes = File.ReadAllLines(file).ToList();
                
                //If the checkpoint disk matches with the location of the programs, 
                //the checkpoint and settings files are added to the list of objects.
                if (cpObject.Type != AnalyseObjectType.Registry)
                {
                    var currentDirectory = Directory.GetCurrentDirectory();
                    if (cpObject.Name.FirstOrDefault() == currentDirectory.FirstOrDefault())
                    {
                        string folder = currentDirectory + FolderPath;
                        if (Directory.Exists(folder))
                        {
                            pathes.Add(folder + SettingsFile);

                            string[] files = Directory.GetFiles(folder, $"*{_checkPointFileExtension}");
                            foreach (string f in files)
                                pathes.Add(f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }

            return (pathes, isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private string GetInstallationFileFullName(string folder, AnalyseObjectType objectType)
        {
            StringBuilder sbFileName = new StringBuilder();
            sbFileName.Append(folder);

            if (objectType == AnalyseObjectType.Registry)
            {
                sbFileName.Append(_checkPointRegistryFilePrefix.Replace("_", ""));
            }
            else
            {
                sbFileName.Append(_checkPointDiskFilePrefix.Replace("_", ""));
            }
            sbFileName.Append(_installationFileExtension);

            return sbFileName.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="filePathes">list of folder and file pathes</param>
        /// <param name="regKeys">list of registry keys</param>
        /// <returns></returns>
        public (bool isError, string errorMessage) SaveInstallationObjectsToFiles(string installationName, IEnumerable<string> filePathes, IEnumerable<string> regKeys)
        {
            bool isError = false;
            string errorMessage = string.Empty;

            if (!filePathes.Any() && !regKeys.Any())
                return (isError, errorMessage);

            try
            {
                var ticks = DateTime.Now.Ticks;
                string installationFolder = String.Join("", installationName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                                                  .Trim();

                StringBuilder sbFolder = new StringBuilder();
                sbFolder.Append(Directory.GetCurrentDirectory())
                        .Append(FolderPath)
                        .Append(installationFolder)
                        .Append(String.IsNullOrWhiteSpace(installationFolder) ? "" : " - ")
                        .Append(ticks)
                        .Append('\\');
                string folder = sbFolder.ToString();

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                
                if (filePathes.Any())
                {
                    string file = GetInstallationFileFullName(folder, AnalyseObjectType.Folder);
                    File.WriteAllLines(file, filePathes);
                }

                if (regKeys.Any())
                {
                    string file = GetInstallationFileFullName(folder, AnalyseObjectType.Registry);
                    File.WriteAllLines(file, regKeys);
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }
            
            return (isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (IEnumerable<Installation> installations, bool isError, string errorMessage) GetInstallations()
        {
            bool isError = false;
            string errorMessage = string.Empty;
            var installations = new List<Installation>();

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath;
                if (!Directory.Exists(folder))
                    return (installations, isError, errorMessage);

                string[] directories = Directory.GetDirectories(folder);
                foreach (var directory in directories)
                {
                    var directoryName = directory.Split('\\').Last();

                    string tickStr = String.Join("", directoryName.TrimEnd()
                                                                  .Reverse()
                                                                  .TakeWhile(c => char.IsDigit(c))
                                                                  .Reverse());
                    DateTime? dt = null;
                    if (long.TryParse(tickStr, out long ticks))
                    {
                        try
                        {
                            dt = new DateTime(ticks);
                        }
                        catch (Exception)
                        { }
                    }
                    installations.Add(new Installation(directoryName, dt));
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }

            return (installations, isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="installationName"></param>
        /// <returns></returns>
        public (IEnumerable<string> files, IEnumerable<string> keys, bool isError, string errorMessage) GetInstallationObjects(string installationName)
        {
            bool isError = false;
            string errorMessage = string.Empty;
            var files = Enumerable.Empty<string>();
            var keys = Enumerable.Empty<string>();

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath + installationName + '\\';
                if (!Directory.Exists(folder))
                {
                    isError = true;
                    errorMessage = $"Отсутствует директория '{folder}'";
                    return (files, keys, isError, errorMessage);
                }
                
                string file = GetInstallationFileFullName(folder, AnalyseObjectType.Folder);
                if (File.Exists(file))
                    files = File.ReadLines(file);

                file = GetInstallationFileFullName(folder, AnalyseObjectType.Registry);
                if (File.Exists(file))
                    keys = File.ReadLines(file);
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }

            return (files, keys, isError, errorMessage);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="installationName"></param>
        /// <returns></returns>
        public (bool isError, string errorMessage) DeleteInstallationData(string installationName)
        {
            bool isError = false;
            string errorMessage = string.Empty;

            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath + installationName + '\\';
                if (Directory.Exists(folder))
                {
                    //remove bypassing the recycle bin
                    Directory.Delete(folder, true);
                }
            }
            catch (Exception ex)
            {
                isError = true;
                errorMessage = ex.GetFullMessage();
            }
            return (isError, errorMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="installationName"></param>
        /// <param name="filePathes"></param>
        /// <param name="regKeys"></param>
        /// <returns></returns>
        public (bool isError, string errorMessage) UpdateInstallationObjectFiles(string installationName, 
                                                                                 IEnumerable<string> filePathes = null, 
                                                                                 IEnumerable<string> regKeys = null)
        {
            bool isError = false;
            string errorMessage = string.Empty;
            
            try
            {
                string folder = Directory.GetCurrentDirectory() + FolderPath + installationName + '\\';
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                if (filePathes != null)
                {
                    string file = GetInstallationFileFullName(folder, AnalyseObjectType.Folder);
                    File.WriteAllLines(file, filePathes);
                }

                if (regKeys != null)
                {
                    string file = GetInstallationFileFullName(folder, AnalyseObjectType.Registry);
                    File.WriteAllLines(file, regKeys);
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
