using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE
{
    public interface ISerializable
    {
        Dictionary<string, object> Serialize();
        void Deserialize(Dictionary<string, object> data);
    }

    public sealed class SerializerHelper
    {
        public static ISerializable CreateFromType(Dictionary<string, object> data)
        {
            var type = (string)data["Type"];
            var instance = Activator.CreateInstance(Type.GetType(type)) as ISerializable;

            if (instance != null)
                instance.Deserialize(data);

            return instance;
        }

        public static float[] ToFloat(Color color)
        {
            return new float[4] { color.R, color.G, color.B, color.A };
        }

        public static float[] ToFloat(Vector2 vec)
        {
            return new float[2] { vec.X, vec.Y };
        }

        public static float[] ToFloat(Vector3 vec)
        {
            return new float[3] { vec.X, vec.Y, vec.Z };
        }

        public static float[] ToFloat(Vector4 vec)
        {
            return new float[4] { vec.X, vec.Y, vec.Z, vec.W };
        }

        public static Color ToColor(float[] value)
        {
            return new Color(value[0], value[1], value[2], value[3]);
        }

        public static Vector2 ToVector2(float[] value)
        {
            return new Vector2(value[0], value[1]);
        }

        public static Vector3 ToVector3(float[] value)
        {
            return new Vector3(value[0], value[1], value[2]);
        }

        public static Vector4 ToVector4(float[] value)
        {
            return new Vector4(value[0], value[1], value[2], value[3]);
        }
    }
}
