using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;

namespace StopTheClip.Structures
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Model
    {
        [FieldOffset(0x18)] public GameObject* ParentObject;
        [FieldOffset(0x20)] public GameObject* PrevLinkedObject;
        [FieldOffset(0x28)] public GameObject* NextLinkedObject;
        [FieldOffset(0x50)] public hkQsTransformf basePosition;
        [FieldOffset(0x88)] public ModelCullTypes CullType;
        [FieldOffset(0x89)] public byte RenderStyle;
        [FieldOffset(0xA0)] public Skeleton* skeleton;
        [FieldOffset(0x120)] public int mountFlag1;
        [FieldOffset(0x124)] public int mountFlag2;
        [FieldOffset(0x128)] public Skeleton* mountedOwnerSkeleton;
        [FieldOffset(0x130)] public GameObject* mountedObject;
        [FieldOffset(0x138)] public int mountFlag3;
    }
}
