using FFXIVClientStructs.Interop.Attributes;
using System.Runtime.InteropServices;

namespace StopTheClip.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct vtblTask
    {
        [FieldOffset(0x8)]
        public unsafe delegate* unmanaged[Stdcall]<GameTask*, float*, void> vf1;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GameTask
    {
        [FieldOffset(0x0)] public vtblTask* vtbl;

        [VirtualFunction(1)]
        public unsafe void vf1(float* taskItem)
        {
            fixed (GameTask* ptr = &this)
            {
                vtbl->vf1(ptr, taskItem);
            }
        }
    }
}
