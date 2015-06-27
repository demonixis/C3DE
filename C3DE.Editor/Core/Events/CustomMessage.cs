using Microsoft.Xna.Framework;
using System;

namespace C3DE.Editor.Events
{
    public sealed class EditorEvent
    {
        public const ushort CommandCopy = 0x0001;
        public const ushort CommandCut = 0x0002;
        public const ushort CommandPast = 0x0003;
        public const ushort CommandAll = 0x0004;
        public const ushort CommandEscape = 0x0005;
        public const ushort CommandSave = 0x0006;
        public const ushort CommandSaveAll = 0x0007;
        public const ushort CommandDuplicate = 0x0008;
        public const ushort CommandNew = 0x0009;
        public const ushort CommandOpen = 0x0010;
        public const ushort KeyJustPressed = 0x1000;

        public const ushort SceneObjectAdded = 0xA000;
        public const ushort SceneObjectRemoved = 0xA001;
        public const ushort SceneObjectRenamed = 0xA010;
        public const ushort SceneObjectSelected = 0xA011;
        public const ushort TransformChanged = 0xB000;
        public const ushort TransformUpdated = 0xB001;
    }

    public enum TransformChangeType
    {
        Position = 0, Rotation, Scale
    }

    public class TransformChanged : BasicMessage
    {
        public TransformChangeType ChangeType { get; protected set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float Z { get; protected set; }

        public TransformChanged() { }

        public TransformChanged(TransformChangeType type, Vector3 vector)
        {
            Set(type, vector.X, vector.Y, vector.Z);
        }

        public TransformChanged(TransformChangeType type, float x, float y, float z)
        {
            Set(type, x, y, z);
        }

        public void Set(TransformChangeType type, float x, float y, float z)
        {
            ChangeType = type;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
