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

        public const ushort KeyJustPressed = 0x1000;

        public const ushort SceneObjectChanged = 0xA000;
        public const ushort SceneObjectUpdated = 0xA001;
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

    public class SceneObjectControlChanged : BasicMessage
    {
        public string Name { get; protected set; }
        public bool Enable { get; protected set; }

        public SceneObjectControlChanged() { }

        public SceneObjectControlChanged(string name, bool enable)
        {
            Set(name, enable);
        }

        public void Set(string name, bool enable)
        {
            Name = name;
            Enable = enable;
        }
    }
}
