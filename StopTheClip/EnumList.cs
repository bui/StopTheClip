using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.Havok;

namespace StopTheClip
{

    [Flags]
    public enum CameraModes
    {
        None = -1,
        FirstPerson = 0,
        ThirdPerson = 1,
    }

    [Flags]
    public enum ModelCullTypes : byte
    {
        None = 0,
        InsideCamera = 0x42,
        OutsideCullCone = 0x43,
        Visible = 0x4B
    }

}