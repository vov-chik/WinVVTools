// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System.Threading.Tasks;

namespace WinVVTools.InternalShared.Interactions
{
    public interface IMessageDialog
    {
        /// <summary>
        /// Current dialog state.
        /// </summary>
        MessageDialogState State { get; }

        /// <summary>
        /// Show dialog as message dialog.
        /// </summary>
        Task<MessageDialogResult> ShowMessageBox(MessageDialogSettings settings);

        /// <summary>
        /// Show dialog as input dialog.
        /// </summary>
        Task<(MessageDialogResult result, string inputText)> ShowInputDialog(MessageDialogSettings settings);

        /// <summary>
        /// Close dialog.
        /// </summary>
        void CloseDialog();
    }
}
