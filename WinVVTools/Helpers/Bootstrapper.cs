// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using WinVVTools.InternalShared.Interactions;
using WinVVTools.Views;

namespace WinVVTools.Helpers
{
    internal class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            
            Container.RegisterType<IMessageDialog, MessageDialogViewModel>();
            //The interactions of weakly coupled modules are made through the Publisher-Subscriber pattern.
            //The Aggregator is created in a single copy for the entire application.
            Container.RegisterInstance<IEventAggregator>(new EventAggregator(), new ContainerControlledLifetimeManager());
        }

        protected override DependencyObject CreateShell()
        {
            return Container.TryResolve<MainShell>(); //Unity
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
        
        protected override void InitializeModules()
        {
            IModule mainModule = Container.TryResolve<ModuleManage>();
            mainModule.Initialize();
        }
    }
}
