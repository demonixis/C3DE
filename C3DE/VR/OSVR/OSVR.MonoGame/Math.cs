/*
 * Port of Math.cs from OSVR-Unity by Sensics, Inc.
 */
using Microsoft.Xna.Framework;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// Class of static methods for converting from OSVR math/tracking types to MonoGame-native data types,
        /// including coordinate system change as needed.
        /// </summary>
        public class Math
        {
            public static Vector3 ConvertPosition(OSVR.ClientKit.Vec3 vec)
            {
                return new Vector3((float)vec.x, (float)vec.y, (float)vec.z);
            }

            public static Vector2 ConvertPosition(ClientKit.Vec2 sourceValue)
            {
                return new Vector2((float)sourceValue.x, (float)sourceValue.y);
            }

            public static Quaternion ConvertOrientation(OSVR.ClientKit.Quaternion quat)
            {
                //// Wikipedia may say quaternions are not handed, but these needed modification.
                //return new Quaternion(-(float)quat.x, -(float)quat.y, (float)quat.z, (float)quat.w);

                // JEB: not sure if these need conversion for MonoGame. Won't know until I get
                // an actual headset.
                return new Quaternion((float)quat.x, (float)quat.y, (float)quat.z, (float)quat.w);
            }

            public static Matrix ConvertPoseToMatrix(XnaPose pose)
            {
                var ret = Matrix.CreateFromQuaternion(pose.Rotation);
                ret.Translation = pose.Position; // This saves one matrix multiply
                return ret;
            }

            public static XnaPose ConvertPose(OSVR.ClientKit.Pose3 pose)
            {
                return new XnaPose
                {
                    Position = Math.ConvertPosition(pose.translation),
                    Rotation = Math.ConvertOrientation(pose.rotation),
                };
            }

            public static XnaEyeTracker3DState ConvertEyeTracker3DState(ClientKit.EyeTracker3DState sourceValue)
            {
                return new XnaEyeTracker3DState
                {
                    BasePositionValid = sourceValue.basePointValid,
                    BasePosition = Math.ConvertPosition(sourceValue.basePoint),
                    DirectionValid = sourceValue.directionValid,
                    Direction = Math.ConvertPosition(sourceValue.direction),
                };
            }
        }
    }
}