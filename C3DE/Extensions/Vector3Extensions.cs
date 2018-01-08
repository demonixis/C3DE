using System;
namespace Microsoft.Xna.Framework
{
    public static class Vector3Extensions
    {
        public static Quaternion ToQuaternion(this Vector3 vector)
        {
            return Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);
        }

        public static void ToQuaternion(this Vector3 vector, ref Quaternion quaternion)
        {
            quaternion = Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);
        }
    }
}
