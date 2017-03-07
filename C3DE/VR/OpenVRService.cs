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

            _textures = new Texture_t[2];
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            uint width = 0;
            uint height = 0;
            _hmd.GetRecommendedRenderTargetSize(ref width, ref height);

            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height);

            _textures[eye] = new Texture_t();

#if WINDOWS
            var info = typeof(RenderTarget2D).GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = info.GetValue(renderTarget) as SharpDX.Direct3D11.Resource;
            _textures[eye].handle = handle.NativePointer;
            _textures[eye].eType = ETextureType.DirectX;
#else
            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var glTexture = (int)info.GetValue(renderTarget);
            _textures[eye].handle = new IntPtr(glTexture);
            _textures[eye].eType = ETextureType.OpenGL;
#endif
            _textures[eye].eColorSpace = EColorSpace.Auto;

            return renderTarget;
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            var mat = _hmd.GetProjectionMatrix((EVREye)eye, 0.1f, 1000.0f);

            if (Input.Keys.Up)
            {
                Debug.Log(mat.m0 + " : " + mat.m1 + " : " + mat.m2 + " : " + mat.m3);
                Debug.Log(mat.m4 + " : " + mat.m5 + " : " + mat.m6 + " : " + mat.m7);
                Debug.Log(mat.m8 + " : " + mat.m9 + " : " + mat.m10 + " : " + mat.m11);
                Debug.Log(mat.m12 + " : " + mat.m13 + " : " + mat.m14 + " : " + mat.m15);
            }

            return Matrix.Transpose(Matrix44ToXNA(ref mat));
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            return 1.0f;
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            var mat = _hmd.GetEyeToHeadTransform((EVREye)eye);
      
            if (Input.Keys.Down)
            {
                Debug.Log(mat.m0 + " : " + mat.m1 + " : " + mat.m2 + " : " + mat.m3);
                Debug.Log(mat.m4 + " : " + mat.m5 + " : " + mat.m6 + " : " + mat.m7);
                Debug.Log(mat.m8 + " : " + mat.m9 + " : " + mat.m10 + " : " + mat.m11);
            }

            return Matrix.Transpose(Matrix34ToXNA(ref mat));
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
}
