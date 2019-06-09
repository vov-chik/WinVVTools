// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using WinVVTools.InternalShared.Helpers;
using WinVVTools.Models;

namespace WinVVTools.ViewModels
{
    internal sealed class MainShellViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly SettingsManager _settingsManager;
        //private readonly IEventAggregator _aggregator;

        public ModuleSettings ModuleSettings
        {
            get { return _moduleSettings; }
            set { SetProperty(ref _moduleSettings, value); }
        }
        private ModuleSettings _moduleSettings;

        public string DisplayName
        {
            get { return _displayName; }
            private set { SetProperty(ref _displayName, value); }
        }
        private string _displayName;


        public bool ShowAbout
        {
            get { return _showAbout; }
            set { SetProperty(ref _showAbout, value); }
        }
        private bool _showAbout;

        public string ShellVersion
        {
            get { return _shellVersion; }
            private set { SetProperty(ref _shellVersion, value); }
        }
        private string _shellVersion;

        public string InternalSharedVersion
        {
            get { return _internalSharedVersion; }
            private set { SetProperty(ref _internalSharedVersion, value); }
        }
        private string _internalSharedVersion;

        public string ModuleCleanUpVersion
        {
            get { return _moduleCleanUpVersion; }
            private set { SetProperty(ref _moduleCleanUpVersion, value); }
        }
        private string _moduleCleanUpVersion;

        public string ModuleDimensionControlVersion
        {
            get { return _moduleDimensionControlVersion; }
            private set { SetProperty(ref _moduleDimensionControlVersion, value); }
        }
        private string _moduleDimensionControlVersion;


        //Required to change theme style.
        private MetroWindow _shellView;

        public ObservableCollection<AppAccent> AppAccents
        {
            get { return _appAccents; }
            set { SetProperty(ref _appAccents, value); }
        }
        private ObservableCollection<AppAccent> _appAccents;

        public ObservableCollection<AppAccent> AppThemes
        {
            get { return _appThemes; }
            set { SetProperty(ref _appThemes, value); }
        }
        private ObservableCollection<AppAccent> _appThemes;

        
        public MainShellViewModel (IUnityContainer container)
        {
            _container = container;
            _settingsManager = _container.Resolve<SettingsManager>();
            //_aggregator = _container.Resolve<IEventAggregator>();

            GetVersion();
            InitializeCurrentSettings();

            //Theme style menu items are updated after getting the settings to sort by language
            SetAppAccents();
        }


        private void GetVersion()
        {
            var shellAssembly = Assembly.GetExecutingAssembly();
            var shellVersion = shellAssembly.GetName().Version;
            ShellVersion = shellVersion.ToString();

            string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(
                                    shellAssembly, typeof(AssemblyProductAttribute)))
                                 .Product;
            StringBuilder sbDisplayName = new StringBuilder();
            sbDisplayName.Append(productName)
                         .Append(' ')
                         .Append(shellVersion.Major)
                         .Append('.')
                         .Append(shellVersion.Minor);
            DisplayName = sbDisplayName.ToString();
            
            try
            {
                InternalSharedVersion = AssemblyName.GetAssemblyName("WinVVTools.InternalShared.dll").Version.ToString();
            }
            catch (Exception)
            {
                InternalSharedVersion = "<не определено>";
            }

            try
            {
                ModuleCleanUpVersion = AssemblyName.GetAssemblyName("WinVVTools.CleanUp.dll").Version.ToString();
            }
            catch (Exception)
            {
                ModuleCleanUpVersion = "<не определено>";
            }

            try
            {
                ModuleDimensionControlVersion = AssemblyName.GetAssemblyName("WinVVTools.DimensionControl.dll").Version.ToString();
            }
            catch (Exception)
            {
                ModuleDimensionControlVersion = "<не определено>";
            }
        }

        private void InitializeCurrentSettings()
        {
            var (settings, isError, errorMessage) = _settingsManager.ReadSettingsFromFile();
            ModuleSettings = settings as ModuleSettings;
            
            if (ModuleSettings == null)
            {
                ModuleSettings = _container.Resolve<ModuleSettings>();
                _settingsManager.SetDefault(ModuleSettings);
            }
            ModuleSettings.IsInitialized = true;
        }

        private void SetAppAccents()
        {
            var appAccents = new List<AppAccent>
            {
                new AppAccent("Красный", "Red"),
                new AppAccent("Зеленый", "Green"),
                new AppAccent("Голубой", "Blue"),
                new AppAccent("Пурпурный", "Purple"),
                new AppAccent("Оранжевый", "Orange"),
                new AppAccent("Лайм", "Lime"),
                new AppAccent("Изумрудный", "Emerald"),
                new AppAccent("Бирюзовый", "Teal"),
                new AppAccent("Синий", "Cyan"),
                new AppAccent("Кобальт", "Cobalt"),
                new AppAccent("Индиго", "Indigo"),
                new AppAccent("Фиолетовый", "Violet"),
                new AppAccent("Розовый", "Pink"),
                new AppAccent("Магента", "Magenta"),
                new AppAccent("Малиновый", "Crimson"),
                new AppAccent("Янтарный", "Amber"),
                new AppAccent("Желтый", "Yellow"),
                new AppAccent("Коричневый", "Brown"),
                new AppAccent("Оливковый", "Olive"),
                new AppAccent("Стальной", "Steel"),
                new AppAccent("Лиловый", "Mauve"),
                new AppAccent("Серо-коричневый", "Taupe"),
                new AppAccent("Сиена", "Sienna")
            };
            appAccents.Sort();
            AppAccents = new ObservableCollection<AppAccent>();
            foreach (var item in appAccents)
                AppAccents.Add(item);

            var appThemes = new List<AppAccent>
            {
                new AppAccent("Светлая", "BaseLight"),
                new AppAccent("Темная", "BaseDark")
            };
            appThemes.Sort();
            AppThemes = new ObservableCollection<AppAccent>();
            foreach (var item in appThemes)
                AppThemes.Add(item);
        }
        
        private DelegateCommand<MetroWindow> _windowLoadedCommand;
        public DelegateCommand<MetroWindow> WindowLoadedCommand
        {
            get { return _windowLoadedCommand ?? (_windowLoadedCommand = new DelegateCommand<MetroWindow>(WindowLoaded)); }
        }
        private void WindowLoaded(MetroWindow view)
        {
            _shellView = view;
            ChangeAppStyle(ModuleSettings.AppAccent, ModuleSettings.AppTheme);
        }

        private async void ChangeAppStyle(string accent, string theme)
        {
            try
            {
                ThemeManager.ChangeAppStyle(_shellView,
                                            ThemeManager.GetAccent(accent),
                                            ThemeManager.GetAppTheme(theme));

                if (accent != ModuleSettings.AppAccent || theme != ModuleSettings.AppTheme)
                {
                    ModuleSettings.AppAccent = accent;
                    ModuleSettings.AppTheme = theme;

                    var (isError, errorMessage) = _settingsManager.WriteSettingsToFile(ModuleSettings);
                    if (isError)
                        throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                var ms = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "ОК",
                    OwnerCanCloseWithDialog = true
                };
                //var _shellView = (Application.Current.MainWindow as MetroWindow);
                await _shellView.ShowMessageAsync("Ошибка изменения темы оформления", ex.GetFullMessage(), MessageDialogStyle.Affirmative, ms);
            }
        }
        
        private DelegateCommand<AppAccent> _selectAppAccentCommand;
        public DelegateCommand<AppAccent> SelectAppAccentCommand
        {
            get { return _selectAppAccentCommand ?? (_selectAppAccentCommand = new DelegateCommand<AppAccent>(SelectAppAccent)); }
        }
        private void SelectAppAccent(AppAccent accent)
        {
            ChangeAppStyle(accent.Value, ModuleSettings.AppTheme);
        }

        private DelegateCommand<AppAccent> _selectAppThemeCommand;
        public DelegateCommand<AppAccent> SelectAppThemeCommand
        {
            get { return _selectAppThemeCommand ?? (_selectAppThemeCommand = new DelegateCommand<AppAccent>(SelectAppTheme)); }
        }
        private void SelectAppTheme(AppAccent accent)
        {
            ChangeAppStyle(ModuleSettings.AppAccent, accent.Value);
        }

        
        private DelegateCommand _aboutCommand;
        public DelegateCommand AboutCommand
        {
            get { return _aboutCommand ?? (_aboutCommand = new DelegateCommand(About)); }
        }
        private void About()
        {
            ShowAbout = true;
        }
        
        private DelegateCommand<Uri> _requestNavigateCommand;
        public DelegateCommand<Uri> RequestNavigateCommand
        {
            get { return _requestNavigateCommand ?? (_requestNavigateCommand = new DelegateCommand<Uri>(RequestNavigate)); }
        }
        private void RequestNavigate(Uri uri)
        {
            System.Diagnostics.Process.Start(uri.ToString());
        }
    }
}
