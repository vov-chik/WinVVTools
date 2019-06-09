// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace WinVVTools.Models
{
    internal class AppAccent : BindableBase, IEquatable<AppAccent>, IComparable<AppAccent>
    {
        /// <summary>
        /// Representation according to the chosen culture
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;

        /// <summary>
        /// Value in resource file
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        private string _value;


        public AppAccent(string name, string value)
        {
            Name = name;
            Value = value;
        }
        

        public override bool Equals(object obj)
        {
            return Equals(obj as AppAccent);
        }

        public bool Equals(AppAccent other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = 2061584987;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(AppAccent other)
        {
            return String.Compare(Name, other.Name);
        }
    }
}
