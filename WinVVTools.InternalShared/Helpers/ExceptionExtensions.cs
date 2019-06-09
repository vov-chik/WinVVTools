// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;

namespace WinVVTools.InternalShared.Helpers
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex)
        {
            return ex.InnerException == null
                 ? ex.Message
                 : $"{ex.Message} --> {ex.InnerException.GetFullMessage()}";
        }
    }
}
