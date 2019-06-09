// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using WinVVTools.CleanUp.Helpers;
using WinVVTools.InternalShared.Helpers;

namespace WinVVTools.CleanUp.Models
{
    internal class AnalyseLogic
    {
        private bool _isInterrupted;

        #region Constructor

        public AnalyseLogic()
        {

        }

        #endregion


        public void InitializeAnalyse()
        {
            _isInterrupted = false;
        }

        public void InterruptAnalyse()
        {
            _isInterrupted = true;
        }


        private static IComparer<string> SortPathAscending()
        {
            return new PathComparer();
        }

        
        public IEnumerable<string> ScanFoldersAndFiles(AnalyseProcessActivity activity, FolderStructure folderStructure)
        {
            activity.SearchState = AnalyseState.Processing;

            var files = Enumerable.Empty<string>();
            try
            {
                files = GetAllFiles(activity, $"{folderStructure.PathPart}\\", folderStructure.IsAnalyzed, folderStructure.Childrens, "*.*")
                            .ToArray();
            }
            catch (Exception ex)
            {
                activity.IsError = true;
                activity.ErrorMessage = $"Ошибка сканирования диска '{folderStructure.PathPart}\\': {ex.GetFullMessage()}";
                InterruptAnalyse();
            }
            
            if (_isInterrupted)
            {
                activity.SearchState = AnalyseState.Interrupted;
            }
            else
            {
                //files.OrderBy(f => f, SortPathAscending());
                activity.SearchState = AnalyseState.Completed;
            }
            
            return files;
        }
        
        private IEnumerable<string> GetAllFiles(AnalyseProcessActivity activity, 
                                                string path, 
                                                bool analyzed, 
                                                IEnumerable<FolderStructure> childrensRules, 
                                                string searchPattern)
        {
            if (_isInterrupted)
                return Enumerable.Empty<string>();

            try
            {
                var files = (new string[] { path })
                            .Union(Directory.EnumerateFiles(path, searchPattern))
                            .Where(f => analyzed);

                activity.SearchCount = activity.SearchCount + files.Count();

                files = files.Union(Directory.EnumerateDirectories(path)
                                            .SelectMany(f =>
                                            {
                                                var cr = childrensRules?.FirstOrDefault(c => f.EndsWith($"\\{c.PathPart}"));
                                                //If there are child folders, it means that 
                                                //the last nested of them must be analyzed.
                                                if (cr == null)
                                                {
                                                    if (analyzed)
                                                        return GetAllFiles(activity, $"{f}\\", analyzed, null, searchPattern);
                                                    else
                                                        return Enumerable.Empty<string>();
                                                }
                                                return GetAllFiles(activity, $"{f}\\", cr.IsAnalyzed, cr.Childrens, searchPattern);
                                            }));

                return files;
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }

        private RegistryHive? GetRegistryRootByName(string name)
        {
            switch (name)
            {
                case "HKEY_CLASSES_ROOT":
                case "HCR":
                    return RegistryHive.ClassesRoot;
                case "HKEY_CURRENT_USER":
                case "HCU":
                    return RegistryHive.CurrentUser;
                case "HKEY_LOCAL_MACHINE":
                case "HLM":
                    return RegistryHive.LocalMachine;
                case "HKEY_USERS":
                case "HU":
                    return RegistryHive.Users;
                case "HKEY_CURRENT_CONFIG":
                case "HCC":
                    return RegistryHive.CurrentConfig;
                default:
                    return null;
            }
        }

        public string ShortRegistryRootName(string name)
        {
            if (GetRegistryRootByName(name) == null)
                return null;

            if (name.Contains('_'))
                return String.Concat(name.Split('_').Select(c => c.First()));

            return name;
        }

        public string LongRegistryRootName(string name)
        {
            switch (name)
            {
                case "HCR":
                    return "HKEY_CLASSES_ROOT";
                case "HCU":
                    return "HKEY_CURRENT_USER";
                case "HLM":
                    return "HKEY_LOCAL_MACHINE";
                case "HU":
                    return "HKEY_USERS";
                case "HCC":
                    return "HKEY_CURRENT_CONFIG";
                default:
                    return name;
            }
        }

        public (RegistryHive? root, string key, string value) ParsRegistryKey(string path)
        {
            int openBracket = 0;
            int closeBracket = 0;
            int closeBracketIndex = 0;
            int slashIndex = 0;
            
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == '[')
                    openBracket++;
                else if (path[i] == ']')
                    closeBracket++;
                else if (path[i] == '\\' && slashIndex == 0)
                    slashIndex = i;

                if (openBracket == closeBracket)
                {
                    closeBracketIndex = i;
                    break;
                }
            }

            if (!path.StartsWith("[") || closeBracketIndex == 0)
                throw new ArgumentException($"Не удалось разобрать строку на составляющие записи ключа реестра.");

            //In this case, the string will contain only the root without slash.
            if (slashIndex == 0) slashIndex = closeBracketIndex;
            
            string root = path.Substring(1, slashIndex - 1);
            string key = (closeBracketIndex - slashIndex <= 1) ? null : path.Substring(slashIndex + 1, closeBracketIndex - 2 - root.Length);
            string value = (path.Length - closeBracketIndex <= 1) ? null : path.Substring(closeBracketIndex + 1);

            return (GetRegistryRootByName(root), key, value);
        }


        public IEnumerable<string> ScanRegistry(AnalyseProcessActivity activity, FolderStructure regStructure)
        {
            activity.SearchState = AnalyseState.Processing;

            var keys = Enumerable.Empty<string>();
            try
            {
                var regView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var root = RegistryKey.OpenBaseKey((RegistryHive)GetRegistryRootByName(regStructure.PathPart), regView))
                {
                    keys = GetAllRegistryKeys(activity, root, regStructure.IsAnalyzed, regStructure.Childrens)
                                .ToArray();
                };
            }
            catch (Exception ex)
            {
                activity.IsError = true;
                activity.ErrorMessage = $"Ошибка сканирования ветки реестра '{LongRegistryRootName(regStructure.PathPart)}': {ex.GetFullMessage()}";
                InterruptAnalyse();
            }
            
            if (_isInterrupted)
            {
                activity.SearchState = AnalyseState.Interrupted;
            }
            else
            {
                //keys.OrderBy(k => k, SortPathAscending());
                activity.SearchState = AnalyseState.Completed;
            }

            return keys;
        }

        private IEnumerable<string> GetAllRegistryKeys(AnalyseProcessActivity activity,
                                                       RegistryKey currentKey,
                                                       bool analyzed,
                                                       IEnumerable<FolderStructure> childrensRules)
        {
            if (_isInterrupted)
                return Enumerable.Empty<string>();
            
            try
            {
                string currentKeyName = currentKey.Name.Contains('\\') ? 
                                        currentKey.Name.Remove(0, currentKey.Name.IndexOf('\\') + 1) :
                                        string.Empty;
                var keys = (new string[] { $"[{currentKeyName}]" })
                            .Union(currentKey.GetValueNames()
                                             .Select(n => $"[{currentKeyName}]{n}"))
                            .Where(f => analyzed);

                activity.SearchCount = activity.SearchCount + keys.Count();

                keys = keys.Union(currentKey.GetSubKeyNames()
                                            .SelectMany(k =>
                                            {
                                                var cr = childrensRules?.FirstOrDefault(c => c.PathPart == k);
                                                //If there are child folders, it means that 
                                                //the last nested of them must be analyzed.
                                                if (cr == null && !analyzed)
                                                    return Enumerable.Empty<string>();

                                                RegistryKey childKey = null;
                                                try
                                                {
                                                    childKey = currentKey.OpenSubKey(k);
                                                }
                                                catch (SecurityException)
                                                {
                                                    //If do not have enough access rights, skip the records.
                                                    return Enumerable.Empty<string>();
                                                    //In case of other errors, scanning is interrupted.
                                                }

                                                if (cr == null)
                                                    return GetAllRegistryKeys(activity, childKey, analyzed, null);
                                                else
                                                    return GetAllRegistryKeys(activity, childKey, cr.IsAnalyzed, cr.Childrens);
                                            }));

                return keys;
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }

    }
}
