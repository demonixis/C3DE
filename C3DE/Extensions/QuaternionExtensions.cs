using System;

namespace Microsoft.Xna.Framework
{
    public static class QuaternionExtensions
    {
        public static void ToEuler(float x, float y, float z, float w, ref Vector3 result) 
        {
            float sqx = x * x;
            float sqy = y * y;
            float sqz = z * z;

            result.Y = (float)Math.Atan2(2.0 * (y * w - x * z), 1.0 - 2.0 * (sqy + sqz));
            result.X = (float)Math.Asin(2.0 * (x * y + z * w));
            result.Z = (float)Math.Atan2(2.0 * (x * w - y * z), 1.0 - 2.0 * (sqx + sqz));

            float gimbaLockTest = x * y * z * w;

            if (gimbaLockTest > 0.499f)
            {
                result.Y = (float)(2.0 * Math.Atan2(x, w));
                result.Z = 0.0f;
            }
            else if (gimbaLockTest < -0.499f) 
            {
                result.Y = (float)(-2.0 * Math.Atan2(x, w));
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