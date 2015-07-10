using Microsoft.Xna.Framework;
using System;
using System.Text;

namespace C3DE
{
    public sealed class SerializerHelper
    {
        private static StringBuilder stringBuilder = new StringBuilder();
        private static string[] temp;

        public static ISerializable CreateInstance(SerializedCollection data)
        {
            var type = data["Type"];
            var instance = Activator.CreateInstance(Type.GetType(type)) as ISerializable;

            if (instance != null)
                instance.Deserialize(data);

            return instance;
        }

        public static string ToString(Color color)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(color.R);
            stringBuilder.Append("|");
            stringBuilder.Append(color.G);
            stringBuilder.Append("|");
            stringBuilder.Append(color.B);
            stringBuilder.Append("|");
            stringBuilder.Append(color.A);
            return stringBuilder.ToString();
        }

        public static string ToString(Vector2 vec)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(vec.X);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.Y);
            return stringBuilder.ToString();
        }

        public static string ToString(Vector3 vec)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(vec.X);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.Y);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.Z);
            return stringBuilder.ToString();
        }

        public static string ToFloat(Vector4 vec)
        {
            stringBuilder.Length = 0;
            stringBuilder.Append(vec.X);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.Y);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.Z);
            stringBuilder.Append("|");
            stringBuilder.Append(vec.W);
            return stringBuilder.ToString();
        }

        public static Color ToColor(string value)
        {
            temp = value.Split('|');
            return new Color(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), temp.Length == 4 ? float.Parse(temp[3]) : 1);
        }

        public static Vector2 ToVector2(string value)
        {
            temp = value.Split('|');
            return new Vector2(float.Parse(temp[0]), float.Parse(temp[1]));
        }

        public static Vector3 ToVector3(string value)
        {
            temp = value.Split('|');
            return new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
        }

        public static Vector4 ToVector4(string value)
        {
            temp = value.Split('|');
            return new Vector4(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));
        }
    }
}
