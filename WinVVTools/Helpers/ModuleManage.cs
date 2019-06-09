// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using WinVVTools.InternalShared.Behaviour;
using WinVVTools.Modules.CleanUp.ViewModels;
using WinVVTools.Modules.CleanUp.Views;
using WinVVTools.Modules.DimensionControl.ViewModels;
using WinVVTools.Modules.DimensionControl.Views;

namespace WinVVTools.Helpers
{
    internal class ModuleManage : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _container;

        public ModuleManage(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            var viewCU = _container.Resolve<CleanUpTabView>();
            viewCU.DataContext = _container.Resolve<CleanUpTabViewModel>();
            _regionManager.Regions[RegionNames.ModuleTabsRegion].Add(viewCU);

            var viewDC = _container.Resolve<DimensionControlTabView>();
            viewDC.DataContext = _container.Resolve<DimensionControlTabViewModel>();
            _regionManager.Regions[RegionNames.ModuleTabsRegion].Add(viewDC);
        }
    }
}