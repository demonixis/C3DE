using Microsoft.Xna.Framework;

namespace TES3Unity
{
    public static class NIFUtils
    {
        public static Vector3 NifVectorToUnityVector(Vector3 NIFVector)
        {
            Utils.Swap(ref NIFVector.Y, ref NIFVector.Z);

            return NIFVector;
        }
        public static Vector3 NifPointToUnityPoint(Vector3 NIFPoint)
        {
            return NifVectorToUnityVector(NIFPoint) / Convert.MeterInMWUnits;
        }
        public static Matrix NifRotationMatrixToUnityRotationMatrix(Matrix NIFRotationMatrix)
        {
            Matrix matrix = new Matrix();
            matrix.M11 = NIFRotationMatrix.M11;
            matrix.M12 = NIFRotationMatrix.M13;
            matrix.M13 = NIFRotationMatrix.M12;
            matrix.M14 = 0;
            matrix.M21 = NIFRotationMatrix.M21;
            matrix.M22 = NIFRotationMatrix.M23;
            matrix.M23 = NIFRotationMatrix.M22;
            matrix.M24 = 0;
            matrix.M31 = NIFRotationMatrix.M31;
            matrix.M32 = NIFRotationMatrix.M33;
            matrix.M33 = NIFRotationMatrix.M32;
            matrix.M34 = 0f;
            matrix.M41 = 0f;
            matrix.M42 = 0f;
            matrix.M43 = 0f;
            matrix.M44 = 1f;

            return  Matrix.Transpose(matrix);
        }
        public static Quaternion NifRotationMatrixToUnityQuaternion(Matrix NIFRotationMatrix)
        {
            var matrix = NifRotationMatrixToUnityRotationMatrix(NIFRotationMatrix);
            matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            return rotation;
        }

        public static Quaternion NifEulerAnglesToUnityQuaternion(Vector3 NifEulerAngles)
        {
            var eulerAngles = NifVectorToUnityVector(NifEulerAngles);

            var xRot = Quaternion.CreateFromAxisAngle(Vector3.Right, MathHelper.ToDegrees(eulerAngles.X));
            var yRot = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToDegrees(eulerAngles.Y));
            var zRot = Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToDegrees(eulerAngles.Z));

            return xRot * zRot * yRot;
        }
    }
}