// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Collections.Generic;

namespace WinVVTools.CleanUp.Helpers
{
    internal class PathComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y)
                return 0;

            int xLength = x.Length;
            int yLength = y.Length;
            int maxIndex = Math.Min(xLength - 1, yLength - 1);
            for (int i = 0; i <= maxIndex; i++)
            {
                if (x[i] == y[i]) continue;
                if (x[i] == '\\') return -1;
                if (y[i] == '\\') return 1;
                return x[i] < y[i] ? -1 : 1;
            }
            return xLength < yLength ? -1 : 1;
        }
    }
}