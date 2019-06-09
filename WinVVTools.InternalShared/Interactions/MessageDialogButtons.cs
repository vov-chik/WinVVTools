// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

namespace WinVVTools.InternalShared.Interactions
{
    /// <summary>
    /// An enum representing the different button states for a Message Dialog.
    /// </summary>
    public enum MessageDialogButtons
    {
        /// <summary>
        /// Just "OK"
        /// </summary>
        Affirmative = 0,
        /// <summary>
        /// "OK" and "Cancel"
        /// </summary>
        AffirmativeAndNegative = 1,
        AffirmativeAndNegativeAndSingleAuxiliary = 2,
        AffirmativeAndNegativeAndDoubleAuxiliary = 3
    }
}
