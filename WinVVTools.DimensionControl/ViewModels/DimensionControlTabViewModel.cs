// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Linq;
using System.Windows.Controls;
using WinVVTools.InternalShared.Behaviour;
using WinVVTools.Modules.DimensionControl.Views;

namespace WinVVTools.Modules.DimensionControl.ViewModels
{
    public class DimensionControlTabViewModel : BindableBase
    {
        #region Private Variables

        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _container;

        #endregion

        #region Fields

        public string BadgeValue
        {
            get { return _badgeValue; }
            private set { SetProperty(ref _badgeValue, value); }
        }
        private string _badgeValue = string.Empty;

        public bool IsActive
        {
            get { return _isActive; }
            private set { SetProperty(ref _isActive, value); }
        }
        private bool _isActive = false;

        #endregion

        #region Constructor

        public DimensionControlTabViewModel(IRegionManager regionMananger, IUnityContainer container)
        {
            _regionManager = regionMananger;
            _container = container;
        }

        #endregion


        #region Commands

        private DelegateCommand _showModuleWorkspaceCommand;
        public DelegateCommand ShowModuleWorkspaceCommand
        {
            get { return _showModuleWorkspaceCommand ?? (_showModuleWorkspaceCommand = new DelegateCommand(ShowModuleWorkspaceView)); }
        }
        private void ShowModuleWorkspaceView()
        {
            var view = _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Views.FirstOrDefault(r => r is DimensionControlView);
            if (view == null)
            {
                view = _container.Resolve<DimensionControlView>();
                ((UserControl)view).DataContext = _container.Resolve<DimensionControlViewModel>();
                _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Add(view);
            }
            _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Activate(view);
        }

        #endregion

    }
}
