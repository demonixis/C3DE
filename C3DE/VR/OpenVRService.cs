using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Valve.VR;
using C3DE.Components;

namespace C3DE.VR
{
    public class OpenVRService : GameComponent, IVRDevice
    {
        private CVRSystem _hmd;
        private TrackedDevicePose_t[] _trackedDevices;
        private TrackedDevicePose_t[] _gamePose;
        private Texture_t[] _textures;
        private VRTextureBounds_t[] textureBounds;

        public OpenVRService(Game game)
            : base(game)
        {
            var error = EVRInitError.None;
            _hmd = OpenVR.Init(ref error);

            _trackedDevices = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            _gamePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

            var strDriver = "No Driver";
            var strDisplay = "No Display";

            strDriver = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String);
            strDisplay = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String);

            Debug.LogFormat("Driver: {0} - Display {1}", strDriver, strDisplay);

            float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
            _hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

            float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
            _hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

            var tanHalfFov = new Vector2(Math.Max(Math.Max(-l_left, l_right), Math.Max(-r_left, r_right)), Math.Max(Math.Max(-l_top, l_bottom), Math.Max(-r_top, r_bottom)));
            textureBounds = new VRTextureBounds_t[2];

            textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.X;
            textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.X;
            textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.Y;
            textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.Y;

            textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.X;
            textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.X;
            textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.Y;
            textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.Y;

            game.Components.Add(this);
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            uint width = 0;
            uint height = 0;
            _hmd.GetRecommendedRenderTargetSize(ref width, ref height);

            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height);

            _textures[eye] = new Texture_t();
            _textures[eye].handle = renderTarget.GetSharedHandle();
#if WINDOWS
            _textures[eye].eType = ETextureType.DirectX;
#else
            _textures[eye].eType = ETextureType.OpenGL;
#endif
            _textures[eye].eColorSpace = EColorSpace.Auto;

            return renderTarget;
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            var mat = _hmd.GetProjectionMatrix((EVREye)eye, 0.1f, 1000.0f);
            var n = new RigidTransform(mat);
            return Matrix.CreateScale(1) * Matrix.CreateFromQuaternion(n.rot) * Matrix.CreateTranslation(n.pos);
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            return 1.0f;
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            var mat = _hmd.GetEyeToHeadTransform((EVREye)eye);
            var n = new RigidTransform(mat);
            return Matrix.CreateScale(1) * Matrix.CreateFromQuaternion(n.rot) * Matrix.CreateTranslation(n.pos);
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
            OpenVR.Compositor.Submit(EVREye.Eye_Left, ref _textures[0], ref textureBounds[0], EVRSubmitFlags.Submit_Default);
            OpenVR.Compositor.Submit(EVREye.Eye_Right, ref _textures[1], ref textureBounds[1], EVRSubmitFlags.Submit_Default);
            OpenVR.Compositor.WaitGetPoses(_trackedDevices, _gamePose);
            return 0;
        }

        public string GetTrackedDeviceString(uint deviceIndex, ETrackedDeviceProperty prop)
        {
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            var bufferSize = _hmd.GetStringTrackedDeviceProperty(deviceIndex, prop, null, 0, ref error);
            if (bufferSize == 0)
                return string.Empty;

            var buffer = new System.Text.StringBuilder((int)bufferSize);
            bufferSize = _hmd.GetStringTrackedDeviceProperty(deviceIndex, prop, buffer, bufferSize, ref error);

            return buffer.ToString();
        }

        private Matrix Matrix34ToXNA(ref HmdMatrix34_t mat)
        {
            return new Matrix(
                mat.m0, mat.m1, mat.m2, mat.m3,
                mat.m4, mat.m5, mat.m6, mat.m7,
                mat.m8, mat.m9, mat.m10, mat.m11,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        private Matrix Matrix44ToXNA(ref HmdMatrix44_t mat)
        {
            return new Matrix(
                mat.m0, mat.m1, mat.m2, mat.m3,
                mat.m4, mat.m5, mat.m6, mat.m7,
                mat.m8, mat.m9, mat.m10, mat.m11,
                mat.m12, mat.m13, mat.m14, mat.m15);
        }
    }

    [System.Serializable]
    public struct RigidTransform
    {
        public Vector3 pos;
        public Quaternion rot;

        public static RigidTransform identity
        {
            get { return new RigidTransform(Vector3.Zero, Quaternion.Identity); }
        }

        public static RigidTransform FromLocal(Transform t)
        {
            return new RigidTransform(t.Position, Quaternion.CreateFromYawPitchRoll(t.Rotation.Y, t.Rotation.X, t.Rotation.Z));
        }

        public RigidTransform(Vector3 pos, Quaternion rot)
        {
            this.pos = pos;
            this.rot = rot;
        }

        public RigidTransform(Transform t)
        {
            this.pos = t.Position;
            this.rot = Quaternion.CreateFromYawPitchRoll(t.Rotation.Y, t.Rotation.X, t.Rotation.Z);
        }

        public RigidTransform(HmdMatrix34_t pose)
        {
            var m = Matrix.Identity;

            m[0, 0] = pose.m0;
            m[0, 1] = pose.m1;
            m[0, 2] = -pose.m2;
            m[0, 3] = pose.m3;

            m[1, 0] = pose.m4;
            m[1, 1] = pose.m5;
            m[1, 2] = -pose.m6;
            m[1, 3] = pose.m7;

            m[2, 0] = -pose.m8;
            m[2, 1] = -pose.m9;
            m[2, 2] = pose.m10;
            m[2, 3] = -pose.m11;

            this.pos = m.Translation;
            this.rot = m.Rotation;
        }

        public RigidTransform(HmdMatrix44_t pose)
        {
            var m = Matrix.Identity;

            m[0, 0] = pose.m0;
            m[0, 1] = pose.m1;
            m[0, 2] = -pose.m2;
            m[0, 3] = pose.m3;

            m[1, 0] = pose.m4;
            m[1, 1] = pose.m5;
            m[1, 2] = -pose.m6;
            m[1, 3] = pose.m7;

            m[2, 0] = -pose.m8;
            m[2, 1] = -pose.m9;
            m[2, 2] = pose.m10;
            m[2, 3] = -pose.m11;

            m[3, 0] = pose.m12;
            m[3, 1] = pose.m13;
            m[3, 2] = -pose.m14;
            m[3, 3] = pose.m15;
           
            this.pos = m.Translation;
            this.rot = m.Rotation;
        }

        public HmdMatrix44_t ToHmdMatrix44()
        {
            var m = Matrix.CreateScale(1) * Matrix.CreateFromQuaternion(rot) * Matrix.CreateTranslation(pos);
            var pose = new HmdMatrix44_t();

            pose.m0 = m[0, 0];
            pose.m1 = m[0, 1];
            pose.m2 = -m[0, 2];
            pose.m3 = m[0, 3];

            pose.m4 = m[1, 0];
            pose.m5 = m[1, 1];
            pose.m6 = -m[1, 2];
            pose.m7 = m[1, 3];

            pose.m8 = -m[2, 0];
            pose.m9 = -m[2, 1];
            pose.m10 = m[2, 2];
            pose.m11 = -m[2, 3];

            pose.m12 = m[3, 0];
            pose.m13 = m[3, 1];
            pose.m14 = -m[3, 2];
            pose.m15 = m[3, 3];

            return pose;
        }

        public HmdMatrix34_t ToHmdMatrix34()
        {
            var m = Matrix.CreateScale(1) * Matrix.CreateFromQuaternion(rot) * Matrix.CreateTranslation(pos);
            var pose = new HmdMatrix34_t();
            
            pose.m0 = m[0, 0];
            pose.m1 = m[0, 1];
            pose.m2 = -m[0, 2];
            pose.m3 = m[0, 3];

            pose.m4 = m[1, 0];
            pose.m5 = m[1, 1];
            pose.m6 = -m[1, 2];
            pose.m7 = m[1, 3];

            pose.m8 = -m[2, 0];
            pose.m9 = -m[2, 1];
            pose.m10 = m[2, 2];
            pose.m11 = -m[2, 3];

            return pose;
        }

        public override bool Equals(object o)
        {
            if (o is RigidTransform)
            {
                RigidTransform t = (RigidTransform)o;
                return pos == t.pos && rot == t.rot;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return pos.GetHashCode() ^ rot.GetHashCode();
        }

        public static bool operator ==(RigidTransform a, RigidTransform b)
        {
            return a.pos == b.pos && a.rot == b.rot;
        }

        public static bool operator !=(RigidTransform a, RigidTransform b)
        {
            return a.pos != b.pos || a.rot != b.rot;
        }
    }
}
