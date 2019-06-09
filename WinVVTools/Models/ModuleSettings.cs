// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using Prism.Mvvm;
using WinVVTools.InternalShared.Behaviour;

namespace WinVVTools.Models
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
        public string AppTheme
        {
            get { return _appTheme; }
            set { SetProperty(ref _appTheme, value); }
        }
        private string _appTheme;

        [JsonProperty]
        public string AppAccent
        {
            get { return _appAccent; }
            set { SetProperty(ref _appAccent, value); }
        }
        private string _appAccent;

        //[JsonProperty]
        //public string Language
        //{
        //    get { return _language; }
        //    set { SetProperty(ref _language, value); }
        //}
        //private string _language;
        

        public ModuleSettings()
        {

        }

    }
}
