// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

namespace WinVVTools.CleanUp.Models
{
    internal enum AnalyseObjectType
    {
        //Object type unknown.
        Unknown = 0,
        //Hard disk with the operating system.
        OsDisk = 1,
        //Hard disk.
        HardDisk = 2,
        //A removable storage device, such as a floppy disk drive or USB flash drive.
        RemovableDisk = 3,
        //Network drive.
        NetworkDisk = 4,
        //Optical drive devices such as CD or DVD.
        CDRom = 5,
        //Folder.
        Folder = 6,
        //Registry key.
        Registry = 7
    }
}
