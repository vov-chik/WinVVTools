// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Mvvm;
using System;

namespace WinVVTools.CleanUp.Models
{
    internal class Installation : BindableBase
    {
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;

        public DateTime? Time
        {
            get { return _time; }
            set { SetProperty(ref _time, value); }
        }
        private DateTime? _time;

        public Installation (string name, DateTime? time)
        {
            Name = name;
            Time = time;
        }

        public override string ToString()
        {
            return Name;
        }
    }


    internal class InstallationPath : BindableBase, ISelectedItem
    {
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }
        private string _path;

        /// <summary>
        /// true - if selected in the form list
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        private bool _isSelected;

        public InstallationPath(string path, bool isSelected = false)
        {
            Path = path;
            IsSelected = isSelected;
        }

        public override string ToString()
        {
            return Path;
        }
    }
    
}
