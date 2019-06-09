// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Newtonsoft.Json;
using Prism.Mvvm;
using System;

namespace WinVVTools.CleanUp.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class CheckPoint : BindableBase, IEquatable<CheckPoint>
    {
        /// <summary>
        /// Checkpoint identifier
        /// </summary>
        [JsonProperty]
        public string Id
        {
            get { return _id; }
            private set { SetProperty(ref _id, value); }
        }
        private string _id;

        /// <summary>
        /// The time the checkpoint was created in DateTime.Ticks
        /// </summary>
        [JsonProperty]
        public long TimeTicks
        {
            get { return _timeTicks; }
            private set { SetProperty(ref _timeTicks, value); }
        }
        private long _timeTicks;
        
        public int? FileCount
        {
            get { return _fileCount; }
            set { SetProperty(ref _fileCount, value); }
        }
        private int? _fileCount;
        
        public int? FolderCount
        {
            get { return _folderCount; }
            set { SetProperty(ref _folderCount, value); }
        }
        private int? _folderCount;
        
        public int? RegistryCount
        {
            get { return _registryCount; }
            set { SetProperty(ref _registryCount, value); }
        }
        private int? _registryCount;


        public CheckPoint()
        {
            Id = Guid.NewGuid().ToString("N");
            TimeTicks = DateTime.Now.Ticks;
        }

        public CheckPoint(string id, long timeticks)
        {
            Id = id;
            TimeTicks = timeticks;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as CheckPoint);
        }

        public bool Equals(CheckPoint other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            var hashCode = -1970771175;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return new DateTime(TimeTicks).ToString();
        }
    }
}
