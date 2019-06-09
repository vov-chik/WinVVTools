// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Modularity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using WinVVTools.CleanUp.Models;

namespace WinVVTools.Modules.CleanUp.Views
{
    /// <summary>
    /// Interaction Logic for CleanUpView.xaml
    /// </summary>
    public partial class CleanUpView : UserControl, IModule
    {
        public CleanUpView()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
        }

        private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //In the virtualization mode changes to the selected lines may not be sent to the ItemsSource. 
            //The selection must be processed independently.
            
            var dg = sender as DataGrid;

            Parallel.ForEach((IEnumerable<ISelectedItem>)dg.ItemsSource, s => { s.IsSelected = false; });
            Parallel.ForEach(dg.SelectedItems.Cast<ISelectedItem>(), s => { s.IsSelected = true; });
        }
    }
}
