// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using WinVVTools.CleanUp.Helpers;
using WinVVTools.InternalShared.Behaviour;

namespace WinVVTools.CleanUp.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class ModuleSettings : BindableBase, IModuleSettings
    {
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { SetProperty(ref _isInitialized, value); }
        }
        private bool _isInitialized;

        [JsonProperty]
        public CheckPoint CheckPoint
        {
            get { return _checkPoint; }
            set { SetProperty(ref _checkPoint, value); }
        }
        private CheckPoint _checkPoint;

        [JsonProperty]
        public ObservableCollection<AnalyseObject> AnalysedDisks
        {
            get { return _analysedDisks; }
            set { SetProperty(ref _analysedDisks, value); }
        }
        private ObservableCollection<AnalyseObject> _analysedDisks;

        [JsonProperty]
        public ObservableCollection<AnalyseObject> AnalysedFolders
        {
            get { return _analysedFolders; }
            set { SetProperty(ref _analysedFolders, value); }
        }
        private ObservableCollection<AnalyseObject> _analysedFolders;

        [JsonProperty]
        public ObservableCollection<AnalyseObject> AnalysedRegistries
        {
            get { return _analysedRegistries; }
            set { SetProperty(ref _analysedRegistries, value); }
        }
        private ObservableCollection<AnalyseObject> _analysedRegistries;

        public ModuleSettings()
        {
            AnalysedDisks = new ObservableCollection<AnalyseObject>();
            AnalysedFolders = new ObservableCollection<AnalyseObject>();
            AnalysedRegistries = new ObservableCollection<AnalyseObject>();
        }

        public void AddWithSortAnalysedFolder(AnalyseObject obj)
        {
            AnalysedFolders.Add(obj);
            AnalysedFolders.Sort(f => f);
        }

        public void AddWithSortAnalysedRegistry(AnalyseObject obj)
        {
            AnalysedRegistries.Add(obj);
            AnalysedRegistries.Sort(f => f);
        }

    }
}
