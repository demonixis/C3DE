using System;

namespace C3DE.Editor
{
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
