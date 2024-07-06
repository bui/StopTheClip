using System;

namespace StopTheClip.Structures
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class VirtualFunctionAttribute(uint index) : Attribute
    {
        public uint Index { get; } = index;
    }
}
