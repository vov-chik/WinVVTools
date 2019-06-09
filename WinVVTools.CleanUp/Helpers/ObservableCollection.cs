// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WinVVTools.CleanUp.Helpers
{
    internal static class ObservableCollection
    {
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector, bool byDescending = false)
        {
            if (!byDescending)
            {
                List<TSource> sortedList = source.OrderBy(keySelector).ToList();
                source.Clear();
                foreach (var sortedItem in sortedList)
                {
                    source.Add(sortedItem);
                }
            }
            else
            {
                List<TSource> sortedList = source.OrderByDescending(keySelector).ToList();
                source.Clear();
                foreach (var sortedItem in sortedList)
                {
                    source.Add(sortedItem);
                }
            }
        }
    }
}
