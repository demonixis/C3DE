using System;

namespace Microsoft.Xna.Framework
{
    public static class MatrixExtensions
    {
        public static void ExtractRotation(this Matrix matrix, ref Quaternion rotation)
        {
            if (float.IsNaN(matrix.M11))
                return;

            var sx = (Math.Sign(matrix.M11 * matrix.M12 * matrix.M13 * matrix.M14) < 0) ? -1.0f : 1.0f;
            var sy = (Math.Sign(matrix.M21 * matrix.M22 * matrix.M23 * matrix.M24) < 0) ? -1.0f : 1.0f;
            var sz = (Math.Sign(matrix.M31 * matrix.M32 * matrix.M33 * matrix.M34) < 0) ? -1.0f : 1.0f;

            sx *= (float)Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13);
            sy *= (float)Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23);
            sz *= (float)Math.Sqrt(matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33);

            if (sx == 0.0 || sy == 0.0 || sz == 0.0)
            {
                rotation = Quaternion.Identity;
                return;
            }

            var m = new Matrix(
                matrix.M11 / sx, matrix.M12 / sx, matrix.M13 / sx, 0.0f,
                matrix.M21 / sy, matrix.M22 / sy, matrix.M23 / sy, 0.0f,
                matrix.M31 / sz, matrix.M32 / sz, matrix.M33 / sz, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);

            rotation = Quaternion.CreateFromRotationMatrix(m);
        }

        public static Vector3 ExtractRottaion(this Matrix matrix)
        {
            var rotation = Vector3.Zero;
            var q = Quaternion.Identity;
            ExtractRotation(matrix, ref q);
            q.ToEuler(ref rotation);
            return rotation;
        }

        public static void ExtractRotation(this Matrix matrix, ref Vector3 rotation)
        {
            var q = Quaternion.Identity;
            ExtractRotation(matrix, ref q);
            q.ToEuler(ref rotation);
        }

        public static Matrix CreateFromVector3(Vector3 row1, Vector3 row2, Vector3 row3, Vector3 row4)
        {
            var v1 = new Vector4(row1, 0);
            var v2 = new Vector4(row2, 0);
            var v3 = new Vector4(row3, 0);
            var v4 = new Vector4(row4, 0);

            return new Matrix(v1, v2, v3, v4);
        }
    }
}
