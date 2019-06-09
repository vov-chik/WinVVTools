// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Modularity;
using System.Windows.Controls;

namespace WinVVTools.Modules.CleanUp.Views
{
    /// <summary>
    /// Interaction Logic for CleanUpTabView.xaml
    /// </summary>
    public partial class CleanUpTabView : UserControl, IModule
    {
        public CleanUpTabView()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
        }
    }
}
