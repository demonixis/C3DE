using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Valve.VR;

namespace C3DE.VR
{
    public class OpenVRService : GameComponent, IVRDevice
    {
        private CVRSystem _hmd;
        private TrackedDevicePose_t[] _trackedDevices;
        private TrackedDevicePose_t[] _gamePose;
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

            Debug.Log("Driver: {0} - Display {1}", strDriver, strDisplay);

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

            return new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height);
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            var mat = _hmd.GetProjectionMatrix((EVREye)eye, 0.1f, 1000.0f);

            var matrix = Matrix44ToXNA(ref mat);
            return Matrix.Transpose(matrix);
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            return 1.0f;
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            var mat = _hmd.GetEyeToHeadTransform((EVREye)eye);
            var matrix = Matrix34ToXNA(ref mat);
            return Matrix.Transpose(matrix);
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
            Texture_t texture0;
            texture0.handle = leftRT.GetSharedHandle();
            texture0.eType = ETextureType.DirectX;
            texture0.eColorSpace = EColorSpace.Auto;

            OpenVR.Compositor.Submit(EVREye.Eye_Left, ref texture0, ref textureBounds[0], EVRSubmitFlags.Submit_Default);

            Texture_t texture1;
            texture1.handle = leftRT.GetSharedHandle();
            texture1.eType = ETextureType.DirectX;
            texture1.eColorSpace = EColorSpace.Auto;

            OpenVR.Compositor.Submit(EVREye.Eye_Right, ref texture1, ref textureBounds[1], EVRSubmitFlags.Submit_Default);

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
}
