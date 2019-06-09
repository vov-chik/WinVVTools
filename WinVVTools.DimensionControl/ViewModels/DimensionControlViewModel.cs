// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Mvvm;
using WinVVTools.InternalShared.Interactions;

namespace WinVVTools.Modules.DimensionControl.ViewModels
{
    internal class DimensionControlViewModel : BindableBase
    {
        public IMessageDialog Dialog { get; }
        
        public DimensionControlViewModel(IMessageDialog dialog)
        {
            Dialog = dialog;
        }


    }
}
