// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System.Collections.Generic;

namespace WinVVTools.CleanUp.Models
{
    internal class FolderStructure
    {
        public string PathPart { get; set; }
        public bool IsAnalyzed { get; set; }

        public IList<FolderStructure> Childrens { get; set; }

        public FolderStructure()
        {
            Childrens = new List<FolderStructure>();
        }

        public FolderStructure(string pathPart, bool isAnalyzed) : this ()
        {
            PathPart = pathPart;
            IsAnalyzed = isAnalyzed;
        }
    }
}
