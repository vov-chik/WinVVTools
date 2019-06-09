// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

namespace WinVVTools.InternalShared.Interactions
{
    /// <summary>
    /// A class that represents the settings used by Message Dialog.
    /// </summary>
    public class MessageDialogSettings
    {
        public MessageDialogSettings()
        {
            this.Title = "";
            this.MessageText = "";
            this.MessageTextVisible = true;

            this.IsInput = false;

            this.DialogtButtons = MessageDialogButtons.AffirmativeAndNegative;

            this.AffirmativeButtonText = "OK";
            this.NegativeButtonText = "Cancel";
            this.FirstAuxiliaryButtonText = string.Empty;
            this.SecondAuxiliaryButtonText = string.Empty;
        }

        /// <summary>
        /// Gets or sets the default title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the default text
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the default message text visibility
        /// </summary>
        public bool MessageTextVisible { get; set; }

        /// <summary>
        /// Gets or sets the type of dialog: input or message box
        /// </summary>
        public bool IsInput { get; set; }

        /// <summary>
        /// Gets or sets the message button number
        /// </summary>
        public MessageDialogButtons DialogtButtons { get; set; }

        /// <summary>
        /// Gets or sets the text used for the Affirmative button. For example: "OK" or "Yes".
        /// </summary>
        public string AffirmativeButtonText { get; set; }

        /// <summary>
        /// Gets or sets the text used for the Negative button. For example: "Cancel" or "No".
        /// </summary>
        public string NegativeButtonText { get; set; }

        /// <summary>
        /// Gets or sets the text used for the first auxiliary button.
        /// </summary>
        public string FirstAuxiliaryButtonText { get; set; }
        
        /// <summary>
        /// Gets or sets the text used for the second auxiliary button.
        /// </summary>
        public string SecondAuxiliaryButtonText { get; set; }
    }
}
