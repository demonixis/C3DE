using System;

namespace Microsoft.Xna.Framework
{
    public static class QuaternionExtensions
    {
        public static float ArcTanAngle(float X, float Y)
        {
            if (X == 0)
            {
                if (Y == 1)
                    return (float)MathHelper.PiOver2;
                else
                    return (float)-MathHelper.PiOver2;
            }
            else if (X > 0)
                return (float)Math.Atan(Y / X);
            else if (X < 0)
            {
                if (Y > 0)
                    return (float)Math.Atan(Y / X) + MathHelper.Pi;
                else
                    return (float)Math.Atan(Y / X) - MathHelper.Pi;
            }
            else
                return 0;
        }

        //returns Euler angles that point from one point to another
        public static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);
            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = ArcTanAngle(-v3.Z, -v3.X);
            return angle;
        }

        public static void ToEuler(float x, float y, float z, float w, ref Vector3 result)
        {
            var rotation = new Quaternion(x, y, z, w);
            var forward = Vector3.Transform(Vector3.Forward, rotation);
            var up = Vector3.Transform(Vector3.Up, rotation);
            result = AngleTo(new Vector3(), forward);
            if (result.X == MathHelper.PiOver2)
            {
                result.Y = ArcTanAngle(up.Z, up.X);
                result.Z = 0;
            }
            else if (result.X == -MathHelper.PiOver2)
            {
                result.Y = ArcTanAngle(-up.Z, -up.X);
                result.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix.CreateRotationY(-result.Y));
                up = Vector3.Transform(up, Matrix.CreateRotationX(-result.X));
                result.Z = ArcTanAngle(up.Y, -up.X);
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