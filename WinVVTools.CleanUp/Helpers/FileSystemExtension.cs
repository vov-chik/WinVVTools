// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinVVTools.CleanUp.Helpers
{
    internal static class FileSystemExtension
    {
        #region RecycleBin

        private const uint ERROR_FILE_NOT_FOUND = 2;  //file not exist
        private const uint ERROR_INVALID_LEVEL = 124; //folder not exist

        public static void MoveFileToRecycleBin(string path)
        {
            const uint delete = 3;
            const ushort silentNoConfirmationUndo = 84;
            var fileOp = new SHFILEOPSTRUCT
            {
                FileFunc = delete,
                NamesFrom = path + "\0", //The NamesFrom and NameTo strings must end with two '\ 0'. One will add p/invoke. The result will be two.
                Flags = silentNoConfirmationUndo
            };
            int hResult = SHFileOperation(ref fileOp);
            if (hResult != 0)
            {
                if (hResult != ERROR_FILE_NOT_FOUND &&
                    hResult != ERROR_INVALID_LEVEL)
                    throw new Win32Exception(hResult);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint FileFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NamesFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string NameTo;
            public ushort Flags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        #endregion

        #region File Properties
        
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        public static bool OpenFile(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "open";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        #endregion
    }
}
