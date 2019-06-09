// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinVVTools.CleanUp.Helpers;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Helpers;
using WinVVTools.InternalShared.Interactions;

namespace WinVVTools.Modules.CleanUp.ViewModels
{
    internal class CleanUpViewModel : BindableBase
    {
        #region Fields
        
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _aggregator;
        private readonly SettingsManager _settingsManager;
        private readonly AnalyseLogic _analyseLogic;

        private readonly byte _maxErrorCount = 100;

        private int _analyseStepsCount;
        private int _analyseCompleteStep;
        private bool _analyseInterrupt;
        private bool _analyseError;
        private string _analyseErrorMessage;
        private int _analyseFolderCount;
        private int _analyseFileCount;
        private int _analyseRegistryCount;
        
        public IMessageDialog Dialog { get; }


        public ModuleSettings ModuleSettings
        {
            get { return _moduleSettings; }
            set { SetProperty(ref _moduleSettings, value); }
        }
        private ModuleSettings _moduleSettings;

        public bool IsSettingsChanged
        {
            get { return _isSettingsChanged; }
            private set { SetProperty(ref _isSettingsChanged, value); }
        }
        private bool _isSettingsChanged;


        public AnalyseType CurrentAnalyseType
        {
            get { return _currentAnalyseType; }
            private set { SetProperty(ref _currentAnalyseType, value); }
        }
        private AnalyseType _currentAnalyseType;

        public ObservableCollection<AnalyseProcessActivity> AnalyseActivities
        {
            get { return _analyseActivities; }
            set { SetProperty(ref _analyseActivities, value); }
        }
        private ObservableCollection<AnalyseProcessActivity> _analyseActivities;

        
        public bool IsInstallationsChange
        {
            get { return _isInstallationsChange; }
            private set { SetProperty(ref _isInstallationsChange, value); }
        }
        private bool _isInstallationsChange;

        public int InstallationChangeProgress
        {
            get { return _installationChangeProgress; }
            private set { SetProperty(ref _installationChangeProgress, value); }
        }
        private int _installationChangeProgress;

        public ObservableCollection<Installation> Installations
        {
            get { return _installations; }
            set { SetProperty(ref _installations, value); }
        }
        private ObservableCollection<Installation> _installations;

        public Installation SelectedInstallation
        {
            get { return _selectedInstallation; }
            set { SetProperty(ref _selectedInstallation, value); UpdateInstallationPathes(); }
        }
        private Installation _selectedInstallation;

        public ObservableCollection<InstallationPath> InstallationFiles
        {
            get { return _installationFiles; }
            set { SetProperty(ref _installationFiles, value); }
        }
        private ObservableCollection<InstallationPath> _installationFiles;

        public ObservableCollection<InstallationPath> InstallationRegistries
        {
            get { return _installationRegistries; }
            set { SetProperty(ref _installationRegistries, value); }
        }
        private ObservableCollection<InstallationPath> _installationRegistries;

        #endregion


        #region constructor

        public CleanUpViewModel(IUnityContainer container, IMessageDialog dialog)
        {
            Dialog = dialog;
            _container = container;
            _aggregator = _container.Resolve<IEventAggregator>();
            _settingsManager = _container.Resolve<SettingsManager>();
            //_settingsManager = _container.Resolve<SettingsManager>(new ResolverOverride[]
            //{
            //    new ParameterOverride("dialog", Dialog)
            //});

            AnalyseActivities = new ObservableCollection<AnalyseProcessActivity>();
            _analyseLogic = _container.Resolve<AnalyseLogic>();

            InitializeCurrentSettings();

            Installations = new ObservableCollection<Installation>();
            InstallationFiles = new ObservableCollection<InstallationPath>();
            InstallationRegistries = new ObservableCollection<InstallationPath>();
            UpdateInstallationList();
        }

        #endregion


        #region Initialization

        private void PublishAnalyseEvent(AnalyseEventMessage eventMessage)
        {
            _aggregator.GetEvent<AnalyseEvent>().Publish(eventMessage);
        }

        private async void InitializeCurrentSettings()
        {
            InitializeAnalyse();
            await Task.Run(() =>
            {
                var (settings, isError, errorMessage) = _settingsManager.ReadSettingsFromFile();
                ModuleSettings = settings as ModuleSettings;
                _analyseError = isError;
                _analyseErrorMessage = errorMessage;
            });

            if (_analyseError)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Ошибка чтения файла настроек",
                    MessageText = _analyseErrorMessage,
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
            }

            if (ModuleSettings == null)
            {
                ModuleSettings = _container.Resolve<ModuleSettings>();
                _settingsManager.SetDefault(ModuleSettings);
            }
            ModuleSettings.IsInitialized = true;

            if (ModuleSettings.CheckPoint != null)
            {
                await Task.Run(() =>
                {
                    var (isError, errorMessage) = _settingsManager.CountCheckPointObjects(ModuleSettings.CheckPoint);
                });
            }
        }

        private void InitializeAnalyse()
        {
            _analyseStepsCount = 0;
            _analyseCompleteStep = 0;
            _analyseInterrupt = false;
            _analyseError = false;
            _analyseErrorMessage = string.Empty;
            _analyseFolderCount = 0;
            _analyseFileCount = 0;
            _analyseRegistryCount = 0;

            _analyseLogic.InitializeAnalyse();
        }

        #endregion


        #region CheckPoint

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeleteCheckPointCommand
        {
            get { return _deleteCheckPointCommand ?? (_deleteCheckPointCommand = new DelegateCommand(DeleteCheckPoint)); }
        }
        private DelegateCommand _deleteCheckPointCommand;

        /// <summary>
        /// 
        /// </summary>
        private async void DeleteCheckPoint()
        {
            if (ModuleSettings.CheckPoint == null)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Удаление контрольной точки",
                    MessageText = $"Контрольная точка отсутствует.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "ОК",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Удаление контрольной точки",
                MessageText = $"Удалить контрольную точку '{ModuleSettings.CheckPoint}'?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);
            if (dialogResult == MessageDialogResult.Affirmative)
            {
                InitializeAnalyse();
                await Task.Run(() => DeleteCheckPointData());
                if (_analyseError)
                {
                    ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка удаления контрольной точки",
                        MessageText = _analyseErrorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteCheckPointData()
        {
            var cpTemp = ModuleSettings.CheckPoint;
            ModuleSettings.CheckPoint = null;
            var (isError, errorMessage) = _settingsManager.WriteSettingsToFile(ModuleSettings);
            if (isError)
            {
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = $"Ошибка записи файла настроек: {errorMessage}";
                ModuleSettings.CheckPoint = cpTemp;
                return;
            }

            var (isError2, errorMessage2) = _settingsManager.DeleteCheckPointDataFiles();
            if (isError2)
            {
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = $"Ошибка удаления файлов контрольной точки: {errorMessage}";
                return;
            }

            IsSettingsChanged = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand CreateNewCheckPointCommand
        {
            get { return _createNewCheckPointCommand ?? (_createNewCheckPointCommand = new DelegateCommand(CreateNewCheckPoint)); }
        }
        private DelegateCommand _createNewCheckPointCommand;

        /// <summary>
        /// 
        /// </summary>
        private async void CreateNewCheckPoint()
        {
            if (ModuleSettings.AnalysedDisks.Any(a => a.IsAnalyzed) == false &&
                ModuleSettings.AnalysedFolders.Any(a => a.IsAnalyzed) == false &&
                ModuleSettings.AnalysedRegistries.Any(a => a.IsAnalyzed) == false)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Создание контрольной точки",
                    MessageText = $"Необходимо задать настройки сканирования!",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "ОК",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Создание контрольной точки",
                MessageText = $"Создать новую контрольную точку?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                AnalyseActivities.Clear();
                CurrentAnalyseType = AnalyseType.SearchAllObjects;
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Started));
                InitializeAnalyse();

                var task01 = Task.Run(() => DeleteCheckPointData());

                FolderStructure scannedFolders = null;
                var task02 = Task.Run(() => { scannedFolders = GetScannedFolders(); });

                FolderStructure scannedRegistries = null;
                var task03 = Task.Run(() => { scannedRegistries = GetScannedRegistryKeys(); });

                await Task.WhenAll(task01, task02, task03);

                if (_analyseInterrupt)
                {
                    CreateNewCheckPointEnd(stopwatch);
                    return;
                }

                _analyseStepsCount = scannedFolders.Childrens.Count() * 2 +
                                     scannedRegistries.Childrens.Count() * 2;

                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));

                List<Task> tasks = new List<Task>();

                foreach (var disk in scannedFolders.Childrens)
                {
                    AnalyseObjectType diskType = ModuleSettings.AnalysedDisks.First(a => a.Name.Contains(disk.PathPart)).Type;
                    var activity = new AnalyseProcessActivity(new AnalyseObject(diskType, disk.PathPart, true));
                    AnalyseActivities.Add(activity);

                    var taskScanDisk = Task.Run(() => NewCheckPointScanDisk(activity, disk));
                    tasks.Add(taskScanDisk);
                }

                foreach (var reg in scannedRegistries.Childrens)
                {
                    var activity = new AnalyseProcessActivity(new AnalyseObject(AnalyseObjectType.Registry, reg.PathPart, true));
                    AnalyseActivities.Add(activity);

                    var taskScanRegistry = Task.Run(() => NewCheckPointScanRegistry(activity, reg));
                    tasks.Add(taskScanRegistry);
                }

                await Task.WhenAll(tasks);

                if (_analyseInterrupt)
                {
                    CreateNewCheckPointEnd(stopwatch);
                    return;
                }

                _moduleSettings.CheckPoint = new CheckPoint
                {
                    FolderCount = _analyseFolderCount,
                    FileCount = _analyseFileCount,
                    RegistryCount = _analyseRegistryCount
                };
                var (isError, errorMessage) = _settingsManager.WriteSettingsToFile(ModuleSettings);
                if (isError)
                {
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = $"Ошибка записи файла настроек: {errorMessage}";
                    ModuleSettings.CheckPoint = null;
                }

                CreateNewCheckPointEnd(stopwatch);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopwatch"></param>
        private async void CreateNewCheckPointEnd(Stopwatch stopwatch)
        {
            stopwatch.Stop();

            if (_analyseInterrupt)
            {
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Interrupted));
                if (_analyseError)
                {
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Ошибка создания контрольной точки",
                        MessageText = _analyseErrorMessage,
                        MessageTextVisible = true,
                        AffirmativeButtonText = "ОК",
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
                else
                {
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Создание контрольной точки",
                        MessageText = "Операция отменена",
                        MessageTextVisible = true,
                        AffirmativeButtonText = "ОК",
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
            }
            else
            {
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                                   ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Completed));

                StringBuilder sbMessage = new StringBuilder();
                string numberFormat = "{0:### ### ### ##0}";
                sbMessage.AppendLine($"Контрольная точка успешно создана.")
                         .AppendLine($"папок - {String.Format(numberFormat, _analyseFolderCount)}")
                         .AppendLine($"файлов - {String.Format(numberFormat, _analyseFileCount)}")
                         .AppendLine($"ключей реестра - {String.Format(numberFormat, _analyseRegistryCount)}")
                         .Append($"Время выполнения - {elapsedTime}.");

                var ms = new MessageDialogSettings()
                {
                    Title = "Создание контрольной точки",
                    MessageText = sbMessage.ToString(),
                    MessageTextVisible = true,
                    AffirmativeButtonText = "ОК",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
            }

            CurrentAnalyseType = AnalyseType.Off;
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Off));
            AnalyseActivities.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private FolderStructure GetScannedFolders()
        {
            FolderStructure folders = new FolderStructure();

            foreach (var disk in ModuleSettings.AnalysedDisks)
            {
                if (disk.IsAnalyzed)
                    folders.Childrens.Add(new FolderStructure(disk.Name.Replace("\\", ""), true));
            }

            foreach (var folder in ModuleSettings.AnalysedFolders)
            {
                bool foldertAnalyzed = false;
                var currentFolder = folders;

                var pathParts = folder.Name.Split('\\');
                int partCount = pathParts.Count();
                int currentPartNumber = 0;
                foreach (var part in pathParts)
                {
                    if (currentPartNumber == 0 && ModuleSettings.AnalysedDisks.Any(a => a.Name.Contains(part)) == false)
                        break;

                    currentPartNumber++;
                    if (currentPartNumber == partCount)
                        foldertAnalyzed = folder.IsAnalyzed;

                    var partFolder = currentFolder.Childrens.FirstOrDefault(i => i.PathPart == part);
                    if (partFolder == null)
                    {
                        if (folder.IsAnalyzed == currentFolder.IsAnalyzed)
                        {
                            break;
                        }
                        else
                        {
                            partFolder = new FolderStructure(part, foldertAnalyzed);
                            currentFolder.Childrens.Add(partFolder);
                            currentFolder = partFolder;
                        }
                    }
                    else
                    {
                        currentFolder = partFolder;
                        //Because all folders are initially sorted by nesting and can not be repeated,
                        //then the last part of each path is always null and created in the previous branch of the condition.
                        //This is where the attribute is set to intermediate folders, which depends on the parent folder.
                        foldertAnalyzed = currentFolder.IsAnalyzed;
                    }
                }
            }

            return folders;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private FolderStructure GetScannedRegistryKeys()
        {
            FolderStructure keys = new FolderStructure();

            foreach (var key in ModuleSettings.AnalysedRegistries)
            {
                var pathParts = key.Name.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                pathParts[0] = _analyseLogic.ShortRegistryRootName(pathParts[0]);
                if (pathParts[0] == null)
                    continue;

                bool keyAnalyzed = false;
                var currentKey = keys;

                int partCount = pathParts.Count();
                int currentPartNumber = 0;
                foreach (var part in pathParts)
                {
                    currentPartNumber++;
                    if (currentPartNumber == partCount)
                        keyAnalyzed = key.IsAnalyzed;

                    var partKey = currentKey.Childrens.FirstOrDefault(i => i.PathPart == part);
                    if (partKey == null)
                    {
                        if (key.IsAnalyzed == currentKey.IsAnalyzed)
                        {
                            break;
                        }
                        else
                        {
                            partKey = new FolderStructure(part, keyAnalyzed);
                            currentKey.Childrens.Add(partKey);
                            currentKey = partKey;
                        }
                    }
                    else
                    {
                        currentKey = partKey;
                        keyAnalyzed = currentKey.IsAnalyzed;
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="disk"></param>
        private void NewCheckPointScanDisk(AnalyseProcessActivity activity, FolderStructure disk)
        {
            if (_analyseInterrupt)
                return;

            var foldersAndFiles = _analyseLogic.ScanFoldersAndFiles(activity, disk);
            if (activity.IsError)
            {
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = activity.ErrorMessage;
                _analyseLogic.InterruptAnalyse();
                return;
            }

            if (_analyseInterrupt)
                return;

            int folderCount = foldersAndFiles.Count(f => f.EndsWith("\\"));
            Interlocked.Add(ref _analyseFolderCount, folderCount);
            int fileCount = foldersAndFiles.Count(f => !f.EndsWith("\\"));
            Interlocked.Add(ref _analyseFileCount, fileCount);

            Interlocked.Increment(ref _analyseCompleteStep);
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));

            activity.CompareState = AnalyseState.Processing;
            var (isError, errorMessage) = _settingsManager.SaveCheckPointObjectsToFile(activity.AnalyseObject, foldersAndFiles);
            if (isError)
            {
                activity.CompareState = AnalyseState.Interrupted;
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = errorMessage;
                _analyseLogic.InterruptAnalyse();
                return;
            }

            activity.CompareState = AnalyseState.Completed;
            Interlocked.Increment(ref _analyseCompleteStep);
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="registryKey"></param>
        private void NewCheckPointScanRegistry(AnalyseProcessActivity activity, FolderStructure registryKey)
        {
            if (_analyseInterrupt)
                return;

            var keys = _analyseLogic.ScanRegistry(activity, registryKey);
            if (activity.IsError)
            {
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = activity.ErrorMessage;
                _analyseLogic.InterruptAnalyse();
                return;
            }

            if (_analyseInterrupt)
                return;

            Interlocked.Add(ref _analyseRegistryCount, keys.Count());

            Interlocked.Increment(ref _analyseCompleteStep);
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));

            activity.CompareState = AnalyseState.Processing;
            var (isError, errorMessage) = _settingsManager.SaveCheckPointObjectsToFile(activity.AnalyseObject, keys);
            if (isError)
            {
                activity.CompareState = AnalyseState.Interrupted;
                _analyseInterrupt = true;
                _analyseError = true;
                _analyseErrorMessage = errorMessage;
                _analyseLogic.InterruptAnalyse();
                return;
            }

            activity.CompareState = AnalyseState.Completed;
            Interlocked.Increment(ref _analyseCompleteStep);
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand AnalysePointCommand
        {
            get { return _analysePointCommand ?? (_analysePointCommand = new DelegateCommand(AnalysePoint)); }
        }
        private DelegateCommand _analysePointCommand;

        /// <summary>
        /// 
        /// </summary>
        private async void AnalysePoint()
        {
            if (ModuleSettings.CheckPoint == null)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Поиск новых объектов",
                    MessageText = $"Необходимо создать контрольную точку!",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "ОК",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            if (ModuleSettings.AnalysedDisks.Any(a => a.IsAnalyzed) == false &&
                ModuleSettings.AnalysedFolders.Any(a => a.IsAnalyzed) == false &&
                ModuleSettings.AnalysedRegistries.Any(a => a.IsAnalyzed) == false)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Поиск новых объектов",
                    MessageText = $"Необходимо задать настройки сканирования!",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "ОК",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Поиск новых объектов",
                MessageText = IsSettingsChanged ? $"Настройки сканирования отличаются от настроек контрольной точки. Начать поиск?" : "Начать поиск?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                AnalyseActivities.Clear();
                CurrentAnalyseType = AnalyseType.SearchNewObjects;
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Started));
                InitializeAnalyse();

                FolderStructure scannedFolders = null;
                var task01 = Task.Run(() => { scannedFolders = GetScannedFolders(); });

                FolderStructure scannedRegistries = null;
                var task02 = Task.Run(() => { scannedRegistries = GetScannedRegistryKeys(); });

                await Task.WhenAll(task01, task02);

                if (_analyseInterrupt)
                {
                    AnalysePointEnd(stopwatch);
                    return;
                }

                _analyseStepsCount = scannedFolders.Childrens.Count() * 3 +
                                     scannedRegistries.Childrens.Count() * 3;

                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));

                List<Task> tasks = new List<Task>();

                foreach (var disk in scannedFolders.Childrens)
                {
                    AnalyseObjectType diskType = ModuleSettings.AnalysedDisks.First(a => a.Name.Contains(disk.PathPart)).Type;
                    var activity = new AnalyseProcessActivity(new AnalyseObject(diskType, disk.PathPart, true));
                    AnalyseActivities.Add(activity);

                    Task taskScanDisk = SearchNewObjectsScanDisk(activity, disk);
                    tasks.Add(taskScanDisk);
                }

                foreach (var reg in scannedRegistries.Childrens)
                {
                    var activity = new AnalyseProcessActivity(new AnalyseObject(AnalyseObjectType.Registry, reg.PathPart, true));
                    AnalyseActivities.Add(activity);

                    Task taskScanRegistry = SearchNewObjectsScanRegistry(activity, reg);
                    tasks.Add(taskScanRegistry);
                }

                await Task.WhenAll(tasks);

                AnalysePointEnd(stopwatch);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stopwatch"></param>
        private async void AnalysePointEnd(Stopwatch stopwatch)
        {
            stopwatch.Stop();

            if (!_analyseInterrupt)
            {
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                                   ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Completed));

                if (AnalyseActivities.Sum(a => a.CompareCount) <= 0)
                {
                    StringBuilder sbMessage = new StringBuilder();
                    sbMessage.AppendLine($"Новые объекты отсутствуют.")
                             .Append($"Время выполнения - {elapsedTime}.");
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Поиск новых объектов",
                        MessageText = sbMessage.ToString(),
                        MessageTextVisible = true,
                        AffirmativeButtonText = "ОК",
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
                else
                {
                    StringBuilder sbMessage = new StringBuilder();
                    string numberFormat = "{0:### ### ### ##0}";
                    sbMessage.AppendLine($"Время выполнения - {elapsedTime}. Найдено:")
                             .AppendLine($"папок - {String.Format(numberFormat, _analyseFolderCount)}")
                             .AppendLine($"файлов - {String.Format(numberFormat, _analyseFileCount)}")
                             .AppendLine($"ключей реестра - {String.Format(numberFormat, _analyseRegistryCount)}")
                             .AppendLine()
                             .Append($"Введите название инсталляции.");

                    var ims = new MessageDialogSettings()
                    {
                        Title = "Поиск новых объектов",
                        MessageText = sbMessage.ToString(),
                        MessageTextVisible = true,
                        AffirmativeButtonText = "Сохранить",
                        NegativeButtonText = "Отмена",
                        DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
                    };
                    var (dialogResult, dialogInputText) = await Dialog.ShowInputDialog(ims);

                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        await Task.Run(() =>
                        {
                            var filePathes = AnalyseActivities.Where(a => a.AnalyseObject.Type != AnalyseObjectType.Registry)
                                                              .SelectMany(a => a.CompareObjects);
                            var regKeys = AnalyseActivities.Where(a => a.AnalyseObject.Type == AnalyseObjectType.Registry)
                                                           .SelectMany(a => a.CompareObjects);

                            var (isError, errorMessage) = _settingsManager.SaveInstallationObjectsToFiles(dialogInputText, filePathes, regKeys);
                            if (isError)
                            {
                                _analyseInterrupt = true;
                                _analyseError = true;
                                _analyseErrorMessage = errorMessage;
                                _analyseLogic.InterruptAnalyse();
                            }
                        });

                        if (!_analyseInterrupt)
                        {
                            UpdateInstallationList();
                        }
                    }
                }
            }

            if (_analyseInterrupt)
            {
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Interrupted));
                if (_analyseError)
                {
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Ошибка поиска новых объектов",
                        MessageText = _analyseErrorMessage,
                        MessageTextVisible = true,
                        AffirmativeButtonText = "ОК",
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
                else
                {
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Поиск новых объектов",
                        MessageText = "Операция отменена",
                        MessageTextVisible = true,
                        AffirmativeButtonText = "ОК",
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
            }

            CurrentAnalyseType = AnalyseType.Off;
            PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Off));
            AnalyseActivities.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="disk"></param>
        /// <returns></returns>
        private async Task SearchNewObjectsScanDisk(AnalyseProcessActivity activity, FolderStructure disk)
        {
            if (_analyseInterrupt)
                return;

            IEnumerable<string> currentFoldersAndFiles = Enumerable.Empty<string>();
            var task01 = Task.Run(() =>
            {
                currentFoldersAndFiles = _analyseLogic.ScanFoldersAndFiles(activity, disk);
                if (activity.IsError)
                {
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = activity.ErrorMessage;
                    _analyseLogic.InterruptAnalyse();
                }
                else
                {
                    Interlocked.Increment(ref _analyseCompleteStep);
                    PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
                }
            });

            IEnumerable<string> checkpointFoldersAndFiles = Enumerable.Empty<string>();
            var task02 = Task.Run(() =>
            {
                activity.CheckPointState = AnalyseState.Processing;
                var (pathes, isError, errorMessage) = _settingsManager.GetCheckPointObjectPathes(activity.AnalyseObject);
                if (isError)
                {
                    activity.CheckPointState = AnalyseState.Interrupted;
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = errorMessage;
                    _analyseLogic.InterruptAnalyse();
                }
                else
                {
                    checkpointFoldersAndFiles = pathes;
                    activity.CheckPointCount = checkpointFoldersAndFiles.Count();
                    activity.CheckPointState = AnalyseState.Completed;

                    Interlocked.Increment(ref _analyseCompleteStep);
                    PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
                }
            });

            await Task.WhenAll(task01, task02);

            if (_analyseInterrupt)
                return;

            activity.CompareState = AnalyseState.Processing;
            await Task.Run(() =>
            {
                try
                {
                    activity.CompareObjects = currentFoldersAndFiles.Except(checkpointFoldersAndFiles)
                                                                    .ToArray();
                    activity.CompareCount = activity.CompareObjects.Count();

                    int folderCount = activity.CompareObjects.Count(f => f.EndsWith("\\"));
                    Interlocked.Add(ref _analyseFolderCount, folderCount);
                    int fileCount = activity.CompareObjects.Count(f => !f.EndsWith("\\"));
                    Interlocked.Add(ref _analyseFileCount, fileCount);
                }
                catch (Exception ex)
                {
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = ex.GetFullMessage();
                    _analyseLogic.InterruptAnalyse();
                }
            });

            if (_analyseInterrupt)
            {
                activity.CompareState = AnalyseState.Interrupted;
            }
            else
            {
                activity.CompareState = AnalyseState.Completed;
                Interlocked.Increment(ref _analyseCompleteStep);
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="registryKey"></param>
        /// <returns></returns>
        private async Task SearchNewObjectsScanRegistry(AnalyseProcessActivity activity, FolderStructure registryKey)
        {
            if (_analyseInterrupt)
                return;

            IEnumerable<string> currentKeys = Enumerable.Empty<string>();
            var task01 = Task.Run(() =>
            {
                currentKeys = _analyseLogic.ScanRegistry(activity, registryKey);
                if (activity.IsError)
                {
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = activity.ErrorMessage;
                    _analyseLogic.InterruptAnalyse();
                }
                else
                {
                    Interlocked.Increment(ref _analyseCompleteStep);
                    PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
                }
            });

            IEnumerable<string> checkpointKeys = Enumerable.Empty<string>();
            var task02 = Task.Run(() =>
            {
                activity.CheckPointState = AnalyseState.Processing;
                var (keys, isError, errorMessage) = _settingsManager.GetCheckPointObjectPathes(activity.AnalyseObject);
                if (isError)
                {
                    activity.CheckPointState = AnalyseState.Interrupted;
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = errorMessage;
                    _analyseLogic.InterruptAnalyse();
                }
                else
                {
                    checkpointKeys = keys;
                    activity.CheckPointCount = checkpointKeys.Count();
                    activity.CheckPointState = AnalyseState.Completed;

                    Interlocked.Increment(ref _analyseCompleteStep);
                    PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
                }
            });

            await Task.WhenAll(task01, task02);

            if (_analyseInterrupt)
                return;

            activity.CompareState = AnalyseState.Processing;
            await Task.Run(() =>
            {
                try
                {
                    string regRoot = "[" + _analyseLogic.LongRegistryRootName(activity.AnalyseObject.Name) + "\\";

                    activity.CompareObjects = currentKeys.Except(checkpointKeys)
                                                         .Select(k => regRoot + k.Substring(1))
                                                         .ToArray();
                    activity.CompareCount = activity.CompareObjects.Count();

                    Interlocked.Add(ref _analyseRegistryCount, (int)activity.CompareCount);
                }
                catch (Exception ex)
                {
                    _analyseInterrupt = true;
                    _analyseError = true;
                    _analyseErrorMessage = ex.GetFullMessage();
                    _analyseLogic.InterruptAnalyse();
                }
            });

            if (_analyseInterrupt)
            {
                activity.CompareState = AnalyseState.Interrupted;
            }
            else
            {
                activity.CompareState = AnalyseState.Completed;
                Interlocked.Increment(ref _analyseCompleteStep);
                PublishAnalyseEvent(new AnalyseEventMessage(CurrentAnalyseType, AnalyseState.Processing, _analyseStepsCount, _analyseCompleteStep));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand CancelActivityCommand
        {
            get { return _cancelActivityCommand ?? (_cancelActivityCommand = new DelegateCommand(CancelActivity)); }
        }
        private DelegateCommand _cancelActivityCommand;

        /// <summary>
        /// 
        /// </summary>
        private void CancelActivity()
        {
            _analyseInterrupt = true;
            _analyseLogic.InterruptAnalyse();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand RereadCheckPointSettingsCommand
        {
            get { return _rereadCheckPointSettingsCommand ?? (_rereadCheckPointSettingsCommand = new DelegateCommand(RereadCheckPointSettings)); }
        }
        private DelegateCommand _rereadCheckPointSettingsCommand;

        /// <summary>
        /// 
        /// </summary>
        private void RereadCheckPointSettings()
        {
            _moduleSettings.CheckPoint = null;
            InitializeCurrentSettings();
            IsSettingsChanged = false;
        }

        #endregion


        #region Installations
        
        /// <summary>
        /// 
        /// </summary>
        private async void UpdateInstallationList()
        {
            IsInstallationsChange = true;

            Installations.Clear();

            bool isError = false;
            string errorMessage = string.Empty;
            IEnumerable<Installation> installations = Enumerable.Empty<Installation>();

            await Task.Run(() =>
            {
                var (installations2, isError2, errorMessage2) = _settingsManager.GetInstallations();
                isError = isError2;
                errorMessage = errorMessage2;
                installations = installations2;
            });

            if (isError)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Ошибка обновления списка инсталляций",
                    MessageText = errorMessage,
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
            }
            else
            {
                foreach (var i in installations)
                    Installations.Add(i);
            }

            IsInstallationsChange = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task UpdateInstallationPathes()
        {
            IsInstallationsChange = true;

            InstallationFiles.Clear();
            InstallationRegistries.Clear();

            if (SelectedInstallation == null)
            {
                IsInstallationsChange = false;
                return;
            }

            bool isError = false;
            string errorMessage = string.Empty;
            var newInstallationFiles = new ObservableCollection<InstallationPath>();
            var newInstallationRegistries = new ObservableCollection<InstallationPath>();

            await Task.Run(() =>
            {
                var (files, keys, isError2, errorMessage2) = _settingsManager.GetInstallationObjects(SelectedInstallation.Name);
                isError = isError2;
                errorMessage = errorMessage2;

                if (!isError)
                {
                    foreach (var path in files)
                    {
                        if (!String.IsNullOrWhiteSpace(path))
                            newInstallationFiles.Add(new InstallationPath(path));
                    }
                    foreach (var path in keys)
                    {
                        if (!String.IsNullOrWhiteSpace(path))
                            newInstallationRegistries.Add(new InstallationPath(path));
                    }
                }
            });

            if (isError)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Ошибка обновления списка инсталляций",
                    MessageText = errorMessage,
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
            }
            else
            {
                InstallationFiles = newInstallationFiles;
                InstallationRegistries = newInstallationRegistries;
            }

            IsInstallationsChange = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand UninstalPointCommand
        {
            get { return _uninstalPointCommand ?? (_uninstalPointCommand = new DelegateCommand(UninstalPoint)); }
        }
        private DelegateCommand _uninstalPointCommand;
        private async void UninstalPoint()
        {
            if (SelectedInstallation == null)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Изменение точки инсталляции",
                    MessageText = "Необходимо выбрать точку инсталляции.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "OK",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = $"Произвести полное удаление точки '{SelectedInstallation.Name}' включая папки, файлы и записи реестра?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                int allObjectCount = InstallationFiles.Count() + InstallationRegistries.Count();
                int objectCount = 0;
                int errorCount = 0;
                int progress = 0;
                bool isFilesDeleted = false;

                //Delete files
                StringBuilder sbFileErrorMessage = new StringBuilder();
                var files = InstallationFiles.Reverse().ToList();
                var task01 = Task.Run(() =>
                {
                    int maxIndex = files.Count() - 1;
                    for (int i = 0; i <= maxIndex; i++)
                    {
                        var item = files.ElementAt(maxIndex - i);
                        try
                        {
                            FileSystemExtension.MoveFileToRecycleBin(item.Path);
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref errorCount);
                            if (errorCount < _maxErrorCount)
                            {
                                sbFileErrorMessage.AppendLine(item.Path)
                                                  .AppendLine(ex.GetFullMessage())
                                                  .AppendLine();
                            }
                            files.Remove(item);
                        }
                        Interlocked.Increment(ref objectCount);
                        SetInstallationChangeProgress(ref allObjectCount, ref objectCount, ref progress);
                    }
                    isFilesDeleted = true;
                });
                
                //Delete registry kies
                StringBuilder sbRegistryErrorMessage = new StringBuilder();
                var keys = InstallationRegistries.Reverse().ToList();
                var task02 = Task.Run(() =>
                {
                    var regView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                    int maxIndex = keys.Count() - 1;
                    for (int i = 0; i <= maxIndex; i++)
                    {
                        var item = keys.ElementAt(maxIndex - i);
                        try
                        {
                            DeleteRegistryKey(item.Path, regView);
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref errorCount);
                            if (errorCount < _maxErrorCount)
                            {
                                sbRegistryErrorMessage.AppendLine(item.Path)
                                                      .AppendLine(ex.GetFullMessage())
                                                      .AppendLine();
                            }
                            keys.Remove(item);
                        }
                        Interlocked.Increment(ref objectCount);
                        if (isFilesDeleted)
                            SetInstallationChangeProgress(ref allObjectCount, ref objectCount, ref progress);
                    }
                });
                
                await Task.WhenAll(task01, task02);

                if (errorCount == 0)
                {
                    await Task.Run(() =>
                    {
                        var (isError, errorMessage) = _settingsManager.DeleteInstallationData(SelectedInstallation.Name);
                        if (isError)
                        {
                            errorCount++;
                            sbRegistryErrorMessage.Append(errorMessage);
                        }
                    });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        if (files.Any())
                        {
                            InstallationFiles = new ObservableCollection<InstallationPath>(InstallationFiles.Except(files));
                            var (isError, errorMessage) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                         InstallationFiles.Select(p => p.Path));
                            if (isError)
                            {
                                errorCount++;
                                sbFileErrorMessage.AppendLine(errorMessage)
                                                  .AppendLine();
                            }
                        }

                        if (keys.Any())
                        {
                            InstallationRegistries = new ObservableCollection<InstallationPath>(InstallationRegistries.Except(keys));
                            var (isError, errorMessage) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                         null,
                                                                                                         InstallationRegistries.Select(p => p.Path));
                            if (isError)
                            {
                                errorCount++;
                                sbRegistryErrorMessage.AppendLine(errorMessage)
                                                      .AppendLine();
                            }
                        }
                    });
                }

                if (errorCount == 0)
                {
                    Installations.Remove(SelectedInstallation);
                }
                else
                {
                    if (errorCount >= _maxErrorCount)
                    {
                        sbRegistryErrorMessage.Append("...Количество выводимых ошибок превысило допустимый лимит...");
                    }

                    var ms = new MessageDialogSettings()
                    {
                        Title = "Ошибка удаления точки инсталляции",
                        MessageText = sbFileErrorMessage.ToString() + sbRegistryErrorMessage.ToString(),
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }

                InstallationChangeProgress = 0;
                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxIndex"></param>
        /// <param name="currentIndex"></param>
        /// <param name="progress"></param>
        private void SetInstallationChangeProgress(ref int maxIndex, ref int currentIndex, ref int progress)
        {
            progress = maxIndex == 0 ? 100 : currentIndex * 100 / maxIndex;
            if (InstallationChangeProgress < progress)
                InstallationChangeProgress = progress;
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeletePointCommand
        {
            get { return _deletePointCommand ?? (_deletePointCommand = new DelegateCommand(DeletePoint)); }
        }
        private DelegateCommand _deletePointCommand;
        private async void DeletePoint()
        {
            if (SelectedInstallation == null)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Изменение точки инсталляции",
                    MessageText = "Необходимо выбрать точку инсталляции.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "OK",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = $"Удалить точку инсталляции '{SelectedInstallation.Name}' из списка?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                bool isError = false;
                string errorMessage = string.Empty;

                await Task.Run(() =>
                {
                    var (isError2, errorMessage2) = _settingsManager.DeleteInstallationData(SelectedInstallation.Name);
                    isError = isError2;
                    errorMessage = errorMessage2;
                });
                
                if (isError)
                {
                    var ms = new MessageDialogSettings()
                    {
                        Title = "Ошибка изменения точки инсталляции",
                        MessageText = errorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms);
                }
                else
                {
                    Installations.Remove(SelectedInstallation);
                }

                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeletePointFileCommand
        {
            get { return _deletePointFileCommand ?? (_deletePointFileCommand = new DelegateCommand(DeletePointFile)); }
        }
        private DelegateCommand _deletePointFileCommand;
        private async void DeletePointFile()
        {
            var selected = InstallationFiles.Where(p => p.IsSelected)
                                            .Reverse() //Deletion is performed in the "For" loop from max index to 0. The root folders must be deleted first.
                                            .ToList();

            if (!selected.Any())
                return;

            bool withNested = false;
            await Task.Run(() =>
            {
                try
                {
                    withNested = selected.AsParallel()
                                         .Any(s => s.Path.Last().Equals('\\') &&
                                              Directory.Exists(s.Path) &&
                                              (Directory.GetDirectories(s.Path).Any() || Directory.GetFiles(s.Path).Any()));
                }
                catch (Exception)
                {
                    //In case of access errors to directories, a standard message will be displayed.
                }
            });

            StringBuilder sbMessageText = new StringBuilder();
            sbMessageText.AppendLine($"Выбрано {selected.Count()} папок/файлов.");
            if (withNested)
                sbMessageText.AppendLine($"Выбранные папки содержат вложенные папки/файлы.");
            sbMessageText.Append($"Произвести удаление?");

            var ms = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = sbMessageText.ToString(),
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;
                string errorMessage = String.Empty;

                await Task.Run(() =>
                {
                    byte errorCount = 0;
                    StringBuilder sbErrorMessage = new StringBuilder();
                    int maxIndex = selected.Count() - 1;
                    int progress = 0;
                    for (int i = 0; i <= maxIndex; i++)
                    {
                        var item = selected.ElementAt(maxIndex - i);
                        try
                        {
                            FileSystemExtension.MoveFileToRecycleBin(item.Path);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            if (errorCount < _maxErrorCount)
                            {
                                sbErrorMessage.AppendLine(item.Path)
                                              .AppendLine(ex.GetFullMessage())
                                              .AppendLine();
                            }
                            else if (errorCount == _maxErrorCount)
                            {
                                sbErrorMessage.AppendLine("...Количество выводимых ошибок превысило допустимый лимит...")
                                              .AppendLine();
                            }
                            selected.Remove(item);
                        }
                        SetInstallationChangeProgress(ref maxIndex, ref i, ref progress);
                    }

                    if (selected.Any())
                    {
                        InstallationFiles = new ObservableCollection<InstallationPath>(InstallationFiles.Except(selected));
                        var (isError, updateFileErrorMessage) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                               InstallationFiles.Select(p => p.Path));
                        if (isError)
                            sbErrorMessage.Append(updateFileErrorMessage);
                    }
                    
                    errorMessage = sbErrorMessage.ToString();
                });
                
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    var ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка изменения точки инсталляции",
                        MessageText = errorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }

                InstallationChangeProgress = 0;
                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand RemovePointFileLinkCommand
        {
            get { return _removePointFileLinkCommand ?? (_removePointFileLinkCommand = new DelegateCommand(RemovePointFileLink)); }
        }
        private DelegateCommand _removePointFileLinkCommand;
        private async void RemovePointFileLink()
        {
            var selected = InstallationFiles.Where(p => p.IsSelected);

            if (!selected.Any())
                return;

            var ms = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = $"Удалить выбранные папки и файлы из списка?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                bool isError = false;
                string errorMessage = string.Empty;

                await Task.Run(() =>
                {
                    InstallationFiles = new ObservableCollection<InstallationPath>(InstallationFiles.Except(selected));
                    
                    var (isError2, errorMessage2) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                   InstallationFiles.Select(p => p.Path));
                    isError = isError2;
                    errorMessage = errorMessage2;
                });

                if (isError)
                {
                    var ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка удаления папок и файлов из списка",
                        MessageText = errorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }
                
                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand OpenPointFileCommand
        {
            get { return _openPointFileCommand ?? (_openPointFileCommand = new DelegateCommand(OpenPointFile)); }
        }
        private DelegateCommand _openPointFileCommand;
        private async void OpenPointFile()
        {
            var selected = InstallationFiles.Where(p => p.IsSelected)
                                            .Select(p => p.Path);

            if (!selected.Any())
                return;

            if (selected.Count() > 10)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Просмотр файлов инсталляции",
                    MessageText = "За раз можно открыть не более 10 файлов.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "OK",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Просмотр файлов инсталляции",
                MessageText = $"Открыть выбранные папки / файлы?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                selected.AsParallel().ForAll(path =>
                {
                    try
                    {
                        FileSystemExtension.OpenFile(path);
                    }
                    catch (Exception)
                    { }
                });

                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ShowPointFilePropertiesCommand
        {
            get { return _showPointFilePropertiesCommand ?? (_showPointFilePropertiesCommand = new DelegateCommand(ShowPointFileProperties)); }
        }
        private DelegateCommand _showPointFilePropertiesCommand;
        private async void ShowPointFileProperties()
        {
            var selected = InstallationFiles.Where(p => p.IsSelected)
                                            .Select(p => p.Path);

            if (!selected.Any())
                return;

            if (selected.Count() > 10)
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Просмотр свойств файлов инсталляции",
                    MessageText = "За раз можно открыть не более 10 файлов.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "OK",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
                return;
            }

            var ms2 = new MessageDialogSettings()
            {
                Title = "Просмотр свойств файлов инсталляции",
                MessageText = $"Открыть свойства выбранных папок / файлов?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms2);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                selected.AsParallel().ForAll(path =>
                {
                    try
                    {
                        FileSystemExtension.ShowFileProperties(path);
                    }
                    catch (Exception)
                    { }
                });
                
                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeletePointRegistryCommand
        {
            get { return _deletePointRegistryCommand ?? (_deletePointRegistryCommand = new DelegateCommand(DeletePointRegistry)); }
        }
        private DelegateCommand _deletePointRegistryCommand;
        private async void DeletePointRegistry()
        {
            var selected = InstallationRegistries.Where(p => p.IsSelected)
                                                 .Reverse() //Deletion is performed in the "For" loop from max index to 0. The keys must first be deleted, then the values.
                                                 .ToList();

            if (!selected.Any())
                return;

            var ms = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = $"Записи реестра могут иметь значения. Удалить выбранные записи?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;
                string errorMessage = String.Empty;

                await Task.Run(() =>
                {
                    byte errorCount = 0;
                    StringBuilder sbErrorMessage = new StringBuilder();
                    int maxIndex = selected.Count() - 1;
                    int progress = 0;
                    var regView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;

                    for (int i = 0; i <= maxIndex; i++)
                    {
                        var item = selected.ElementAt(maxIndex - i);
                        try
                        {
                            DeleteRegistryKey(item.Path, regView);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            if (errorCount < _maxErrorCount)
                            {
                                sbErrorMessage.AppendLine(item.Path)
                                              .AppendLine(ex.GetFullMessage())
                                              .AppendLine();
                            }
                            else if (errorCount == _maxErrorCount)
                            {
                                sbErrorMessage.AppendLine("...Количество выводимых ошибок превысило допустимый лимит...")
                                              .AppendLine();
                            }
                            selected.Remove(item);
                        }
                        SetInstallationChangeProgress(ref maxIndex, ref i, ref progress);
                    }

                    if (selected.Any())
                    {
                        InstallationRegistries = new ObservableCollection<InstallationPath>(InstallationRegistries.Except(selected));
                        var (isError, updateFileErrorMessage) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                               null,
                                                                                                               InstallationRegistries.Select(p => p.Path));
                        if (isError)
                            sbErrorMessage.Append(updateFileErrorMessage);
                    }
                    
                    errorMessage = sbErrorMessage.ToString();
                });

                if (!String.IsNullOrEmpty(errorMessage))
                {
                    var ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка изменения точки инсталляции",
                        MessageText = errorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }

                InstallationChangeProgress = 0;
                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="registryView"></param>
        private void DeleteRegistryKey(string path, RegistryView registryView)
        {
            var (root, key, value) = _analyseLogic.ParsRegistryKey(path);

            if (root == null)
                throw new ArgumentException($"Не определен ключ верхнего уровня реестра.");

            if (key == null && value == null)
                return;
            
            using (var currentKey = RegistryKey.OpenBaseKey((RegistryHive)root, registryView))
            {
                if (value == null)
                {
                    currentKey.DeleteSubKeyTree(key, false);
                }
                else
                {
                    if (key != null)
                    {
                        using (var subKey = currentKey.OpenSubKey(key, true))
                        {
                            if (subKey != null)
                                subKey.DeleteValue(value, false);
                        }
                    }
                    else
                    {
                        currentKey.DeleteValue(value, false);
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand RemovePointRegistryLinkCommand
        {
            get { return _removePointRegistryLinkCommand ?? (_removePointRegistryLinkCommand = new DelegateCommand(RemovePointRegistryLink)); }
        }
        private DelegateCommand _removePointRegistryLinkCommand;
        private async void RemovePointRegistryLink()
        {
            var selected = InstallationRegistries.Where(p => p.IsSelected);

            if (!selected.Any())
                return;

            var ms = new MessageDialogSettings()
            {
                Title = "Изменение точки инсталляции",
                MessageText = $"Удалить выбранные записи реестра из списка?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;
                
                bool isError = false;
                string errorMessage = string.Empty;

                await Task.Run(() =>
                {
                    InstallationRegistries = new ObservableCollection<InstallationPath>(InstallationRegistries.Except(selected));

                    var (isError2, errorMessage2) = _settingsManager.UpdateInstallationObjectFiles(SelectedInstallation.Name,
                                                                                                   null,
                                                                                                   InstallationRegistries.Select(p => p.Path));
                    isError = isError2;
                    errorMessage = errorMessage2;
                });

                if (isError)
                {
                    var ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка удаления записей реестра из списка",
                        MessageText = errorMessage,
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }

                IsInstallationsChange = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand OpenPointRegEditCommand
        {
            get { return _openPointRegEditCommand ?? (_openPointRegEditCommand = new DelegateCommand(OpenPointRegEdit)); }
        }
        private DelegateCommand _openPointRegEditCommand;
        private async void OpenPointRegEdit()
        {
            var selected = InstallationRegistries.FirstOrDefault();
            
            var ms = new MessageDialogSettings()
            {
                Title = "Просмотр реестра",
                MessageText = $"Открыть реестр?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Ок",
                NegativeButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                IsInstallationsChange = true;

                try
                {
                    Process.Start("regedit");
                }
                catch (Exception ex)
                {
                    var ms2 = new MessageDialogSettings()
                    {
                        Title = "Ошибка открытия редактора реестра",
                        MessageText = ex.GetFullMessage(),
                        DialogtButtons = MessageDialogButtons.Affirmative
                    };
                    await Dialog.ShowMessageBox(ms2);
                }
                
                IsInstallationsChange = false;
            }
        }
        
        #endregion


        #region AnalyseSettings

        /// <summary>
        /// 
        /// </summary>
        private void SetSettingsChange()
        {
            if (ModuleSettings.CheckPoint != null)
                IsSettingsChanged = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand CheckCleanUpSettingsDiscsAnalyzedCommand
        {
            get { return _checkCleanUpSettingsDiscsAnalyzedCommand ?? (_checkCleanUpSettingsDiscsAnalyzedCommand = new DelegateCommand(CheckCleanUpSettingsDiscsAnalyzed)); }
        }
        private DelegateCommand _checkCleanUpSettingsDiscsAnalyzedCommand;
        private void CheckCleanUpSettingsDiscsAnalyzed()
        {
            SetSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand AddCleanUpSettingsFolderCommand
        {
            get { return _addCleanUpSettingsFolderCommand ?? (_addCleanUpSettingsFolderCommand = new DelegateCommand(AddCleanUpSettingsFolder)); }
        }
        private DelegateCommand _addCleanUpSettingsFolderCommand;
        private async void AddCleanUpSettingsFolder()
        {
            string path = string.Empty;

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    path = dialog.SelectedPath;
                else
                    return;
            }

            if (ModuleSettings.AnalysedFolders.Any(i => i.Name == path))
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Изменение контрольной точки",
                    MessageText = $"Папка '{path}' уже присутствует в списке.",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "OK",
                    DialogtButtons = MessageDialogButtons.Affirmative
                };
                await Dialog.ShowMessageBox(ms);
            }
            else
            {
                var ms = new MessageDialogSettings()
                {
                    Title = "Изменение контрольной точки",
                    MessageText = $"Какое действие необходимо применять к папке '{path}'?",
                    MessageTextVisible = true,
                    AffirmativeButtonText = "Анализировать",
                    NegativeButtonText = "Игнорировать",
                    FirstAuxiliaryButtonText = "Отмена",
                    DialogtButtons = MessageDialogButtons.AffirmativeAndNegativeAndSingleAuxiliary
                };
                var dialogResult = await Dialog.ShowMessageBox(ms);

                if (dialogResult == MessageDialogResult.Affirmative)
                {
                    ModuleSettings.AddWithSortAnalysedFolder(new AnalyseObject(AnalyseObjectType.Folder, path, true));
                    SetSettingsChange();
                }
                else if (dialogResult == MessageDialogResult.Negative)
                {
                    ModuleSettings.AddWithSortAnalysedFolder(new AnalyseObject(AnalyseObjectType.Folder, path, false));
                    SetSettingsChange();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeleteCleanUpSettingsFolderCommand
        {
            get { return _deleteCleanUpSettingsFolderCommand ?? (_deleteCleanUpSettingsFolderCommand = new DelegateCommand(DeleteCleanUpSettingsFolder)); }
        }
        private DelegateCommand _deleteCleanUpSettingsFolderCommand;
        private async void DeleteCleanUpSettingsFolder()
        {
            var ms = new MessageDialogSettings()
            {
                Title = "Изменение контрольной точки",
                MessageText = "Удалить выделенные папки?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Negative)
                return;

            int maxIndex = ModuleSettings.AnalysedFolders.Count - 1;
            int processedCount = 0;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (ModuleSettings.AnalysedFolders[maxIndex - i].IsSelected)
                {
                    ModuleSettings.AnalysedFolders.Remove(ModuleSettings.AnalysedFolders[maxIndex - i]);
                    processedCount++;
                }
            }

            if (processedCount > 0)
                SetSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ChangeCleanUpSettingsFolderCommand
        {
            get { return _changeCleanUpSettingsFolderCommand ?? (_changeCleanUpSettingsFolderCommand = new DelegateCommand(ChangeCleanUpSettingsFolder)); }
        }
        private DelegateCommand _changeCleanUpSettingsFolderCommand;
        private async void ChangeCleanUpSettingsFolder()
        {
            var ms = new MessageDialogSettings()
            {
                Title = "Изменение контрольной точки",
                MessageText = "Сменить настройку анализа выделенных папок?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Negative)
                return;

            int processedCount = 0;
            foreach (var item in ModuleSettings.AnalysedFolders)
            {
                if (item.IsSelected)
                {
                    item.IsAnalyzed = !item.IsAnalyzed;
                    processedCount++;
                }
            }

            if (processedCount > 0)
                SetSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand AddCleanUpSettingsRegisterKeyCommand
        {
            get { return _addCleanUpSettingsRegisterKeyCommand ?? (_addCleanUpSettingsRegisterKeyCommand = new DelegateCommand(AddCleanUpSettingsRegisterKey)); }
        }
        private DelegateCommand _addCleanUpSettingsRegisterKeyCommand;
        private async void AddCleanUpSettingsRegisterKey()
        {
            var ims = new MessageDialogSettings()
            {
                Title = "Изменение контрольной точки",
                MessageText = "Введите ключ реестра",
                MessageTextVisible = true,
                AffirmativeButtonText = "Анализировать",
                NegativeButtonText = "Игнорировать",
                FirstAuxiliaryButtonText = "Отмена",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegativeAndSingleAuxiliary
            };
            var (dialogResult, dialogInputText) = await Dialog.ShowInputDialog(ims);

            bool isAnalyze = true;

            if (dialogResult == MessageDialogResult.Negative)
                isAnalyze = false;
            else if (dialogResult != MessageDialogResult.Affirmative)
                return;

            string key = dialogInputText.Trim();

            if (String.IsNullOrEmpty(key))
                return;

            var existKey = ModuleSettings.AnalysedRegistries.FirstOrDefault(i => i.Name == key);
            if (existKey != null)
            {
                if (existKey.IsAnalyzed == isAnalyze)
                    return;

                existKey.IsAnalyzed = isAnalyze;
            }
            else
            {
                ModuleSettings.AddWithSortAnalysedRegistry(new AnalyseObject(AnalyseObjectType.Registry, key, isAnalyze));
            }

            SetSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand DeleteCleanUpSettingsRegisterKeyCommand
        {
            get { return _deleteCleanUpSettingsRegisterKeyCommand ?? (_deleteCleanUpSettingsRegisterKeyCommand = new DelegateCommand(DeleteCleanUpSettingsRegisterKey)); }
        }
        private DelegateCommand _deleteCleanUpSettingsRegisterKeyCommand;
        private async void DeleteCleanUpSettingsRegisterKey()
        {
            var ms = new MessageDialogSettings()
            {
                Title = "Изменение контрольной точки",
                MessageText = "Удалить выделенные ключи реестра?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Negative)
                return;

            int maxIndex = ModuleSettings.AnalysedRegistries.Count - 1;
            int processedCount = 0;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (ModuleSettings.AnalysedRegistries[maxIndex - i].IsSelected)
                {
                    ModuleSettings.AnalysedRegistries.Remove(ModuleSettings.AnalysedRegistries[maxIndex - i]);
                    processedCount++;
                }
            }

            if (processedCount > 0)
                SetSettingsChange();
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ChangeCleanUpSettingsRegistryCommand
        {
            get { return _changeCleanUpSettingsRegistryCommand ?? (_changeCleanUpSettingsRegistryCommand = new DelegateCommand(ChangeCleanUpSettingsRegistry)); }
        }
        private DelegateCommand _changeCleanUpSettingsRegistryCommand;
        private async void ChangeCleanUpSettingsRegistry()
        {
            var ms = new MessageDialogSettings()
            {
                Title = "Изменение контрольной точки",
                MessageText = "Сменить настройку анализа выделенных ключей реестра?",
                MessageTextVisible = true,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                DialogtButtons = MessageDialogButtons.AffirmativeAndNegative
            };
            var dialogResult = await Dialog.ShowMessageBox(ms);

            if (dialogResult == MessageDialogResult.Negative)
                return;

            int processedCount = 0;
            foreach (var item in ModuleSettings.AnalysedRegistries)
            {
                if (item.IsSelected)
                {
                    item.IsAnalyzed = !item.IsAnalyzed;
                    processedCount++;
                }
            }

            if (processedCount > 0)
                SetSettingsChange();
        }
        
        #endregion
        
    }
}
