// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Linq;
using System.Windows.Controls;
using WinVVTools.CleanUp.Models;
using WinVVTools.InternalShared.Behaviour;
using WinVVTools.Modules.CleanUp.Views;

namespace WinVVTools.Modules.CleanUp.ViewModels
{
    public class CleanUpTabViewModel : BindableBase
    {
        #region Private Variables

        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _aggregator;

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

        public CleanUpTabViewModel(IRegionManager regionMananger, IUnityContainer container)
        {
            _regionManager = regionMananger;
            _container = container;
            _aggregator = _container.Resolve<IEventAggregator>();

            SubscribeAnalyseEvents();
        }

        #endregion

        private void SubscribeAnalyseEvents()
        {
            _aggregator.GetEvent<AnalyseEvent>().Subscribe(OnAnalyseEvent, ThreadOption.UIThread, true);
        }

        private void OnAnalyseEvent(AnalyseEventMessage eventMessage)
        {
            IsActive = (eventMessage.Type != AnalyseType.Off);

            switch (eventMessage.State)
            {
                case AnalyseState.Started:
                    BadgeValue = "...";
                    break;
                case AnalyseState.Processing:
                    BadgeValue = $"{eventMessage.CurrentStep}/{eventMessage.Steps}";
                    break;
                case AnalyseState.Completed:
                    BadgeValue = "V";
                    break;
                case AnalyseState.Interrupted:
                    BadgeValue = "X";
                    break;
                default:
                    BadgeValue = string.Empty;
                    break;
            }
        }

        #region Commands

        private DelegateCommand _showModuleWorkspaceCommand;
        public DelegateCommand ShowModuleWorkspaceCommand
        {
            get { return _showModuleWorkspaceCommand ?? (_showModuleWorkspaceCommand = new DelegateCommand(ShowModuleWorkspaceView)); }
        }
        private void ShowModuleWorkspaceView()
        {
            var view = _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Views.FirstOrDefault(r => r is CleanUpView);
            if (view == null)
            {
                view = _container.Resolve<CleanUpView>();
                ((UserControl)view).DataContext = _container.Resolve<CleanUpViewModel>();
                _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Add(view);
            }
            _regionManager.Regions[RegionNames.ModuleWorkspaceRegion].Activate(view);
        }

        #endregion
        
    }
}
