// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;

namespace WinVVTools.CleanUp.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class AnalyseObject : BindableBase, ISelectedItem, IEquatable<AnalyseObject>, IComparable<AnalyseObject>
    {
        public AnalyseObjectType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
        private AnalyseObjectType _type;

        [JsonProperty]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;

        /// <summary>
        /// true - checked analize, false - checked ignore
        /// </summary>
        [JsonProperty]
        public bool IsAnalyzed
        {
            get { return _isAnalyzed; }
            set { SetProperty(ref _isAnalyzed, value); }
        }
        private bool _isAnalyzed;

        /// <summary>
        /// true - if selected in the form list
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        private bool _isSelected;

        [JsonConstructor]
        public AnalyseObject(string name, bool isAnalyzed)
        {
            Type = AnalyseObjectType.Unknown;
            Name = name;
            IsAnalyzed = isAnalyzed;
        }

        public AnalyseObject(AnalyseObjectType type, string name, bool isAnalyzed)
        {
            Type = type;
            Name = name;
            IsAnalyzed = isAnalyzed;
        }

        public AnalyseObject(DriveType driveType, string name, bool isAnalyzed)
        {
            Type = ConvertDriveType(driveType, name);
            Name = name;
            IsAnalyzed = isAnalyzed;
        }

        private AnalyseObjectType ConvertDriveType(DriveType driveType, string name)
        {
            switch (driveType)
            {
                case DriveType.Removable:
                    return AnalyseObjectType.RemovableDisk;
                case DriveType.Fixed:
                    {
                        string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
                        string path = Path.GetPathRoot(system);
                        return name == path ? AnalyseObjectType.OsDisk : AnalyseObjectType.HardDisk;
                    }
                case DriveType.Network:
                    return AnalyseObjectType.NetworkDisk;
                case DriveType.CDRom:
                    return AnalyseObjectType.CDRom;
                default:
                    return AnalyseObjectType.Unknown;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AnalyseObject);
        }

        public bool Equals(AnalyseObject other)
        {
            return other != null &&
                   Name == other.Name &&
                   IsAnalyzed == other.IsAnalyzed;
        }

        public override int GetHashCode()
        {
            var hashCode = 2061584987;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + IsAnalyzed.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Type} '{Name}'";
        }
        
        public int CompareTo(AnalyseObject other)
        {
            if (this.Type < other.Type)
                return -1;
            if (this.Type > other.Type)
                return 1;

            if (this.Name == other.Name)
                return 0;

            int thisNameLength = this.Name.Length;
            int otherNameLength = other.Name.Length;
            int maxIndex = Math.Min(thisNameLength - 1, otherNameLength - 1);
            for (int i = 0; i <= maxIndex; i++)
            {
                if (this.Name[i] == other.Name[i]) continue;
                if (this.Name[i] == '\\') return -1;
                if (other.Name[i] == '\\') return 1;
                return this.Name[i] < other.Name[i] ? -1 : 1;
            }
            return thisNameLength < otherNameLength ? -1 : 1;
        }
    }
}
