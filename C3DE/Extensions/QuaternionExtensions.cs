using System;

namespace Microsoft.Xna.Framework
{
    public static class QuaternionExtensions
    {
        public static void ToEuler(float x, float y, float z, float w, ref Vector3 result)
        {
            result.X = (float)Math.Atan2(2.0f * y * w - 2.0f * x * z, 1.0f - 2.0f * Math.Pow(y, 2.0f) - 2.0f * Math.Pow(z, 2.0f));
            result.Y = (float)Math.Asin(2.0f * x * y + 2.0f * z * w);
            result.Z = (float)Math.Atan2(2.0f * x * w - 2.0f * y * z, 1.0f - 2.0f * Math.Pow(x, 2.0f) - 2.0f * Math.Pow(z, 2.0f));

            if (x * y + z * w == 0.5f)
            {
                result.X = (float)(2.0f * Math.Atan2(x, w));
                result.Z = 0.0f;
            }
            else if (x * y + z * w == -0.5f)
            {
                result.X = (float)(-2.0f * Math.Atan2(x, w));
                result.Z = 0.0f;
            }
        }

        public static void ToEuler(this Quaternion quaternion, ref Vector3 result)
        {
            ToEuler(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W, ref result);
        }

        public static Vector3 ToEuler(this Quaternion quaternion)
        {
            Vector3 result = Vector3.Zero;
            ToEuler(quaternion, ref result);
            return result;
        }

        public static Quaternion Euler(this Quaternion q, float x, float y, float z) => Quaternion.CreateFromYawPitchRoll(y, x, z);

        public static Quaternion Euler(this Quaternion q, Vector3 rotation) => Euler(q, rotation.X, rotation.Y, rotation.Z);
    }
}