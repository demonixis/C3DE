using Microsoft.Xna.Framework;

namespace OSVR.MonoGame
{
    /// <summary>
    /// Report value for the pose callback
    /// </summary>
    public struct XnaPose
    {
        public XnaPose(Vector3 position, Quaternion rotation)
            : this()
        {
            Position = position;
            Rotation = rotation;
        }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public struct XnaEyeTracker3DState
    {
        public bool BasePositionValid { get; set; }
        public Vector3 BasePosition { get; set; }
        public bool DirectionValid { get; set; }
        public Vector3 Direction { get; set; }
    }
}
