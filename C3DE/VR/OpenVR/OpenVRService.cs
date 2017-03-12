using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Valve.VR;
using System.Runtime.InteropServices;

namespace C3DE.VR
{
    public class OpenVRService : VRService
    {
        private CVRSystem _hmd;
        private TrackedDevicePose_t[] _trackedDevices;
        private TrackedDevicePose_t[] _gamePose;
        private Texture_t[] _textures;
        private VRTextureBounds_t[] _textureBounds;
        private int _validPoseCount = 0;
        private int _trackedControllerCount = 0;
        private int _leftControllerDeviceID = -1;
        private int _rightControllerDeviceID = -1;
        private Matrix[] _devicePoses;
        private Matrix _hmdPose;
        private Matrix _leftControllerPose;
        private Matrix _rightControllerPose;

		public OpenVRService(Game game)
            : base(game)
        {
			_textures = new Texture_t[2];
			_textureBounds = new VRTextureBounds_t[2];
			_trackedDevices = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
			_gamePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
			_devicePoses = new Matrix[OpenVR.k_unMaxTrackedDeviceCount];
        }

		public override int TryInitialize()
		{
			var error = EVRInitError.None;

			_hmd = OpenVR.Init(ref error);

			if (error != EVRInitError.None)
				return -1;

			var strDriver = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String);
			var strDisplay = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String);

			Debug.LogFormat("Driver: {0} - Display {1}", strDriver, strDisplay);

			PreviewRenderEffect = SpriteEffects.FlipVertically;

			Game.Components.Add(this);

			return 0;
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            OpenVR.Shutdown();
        }

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            uint width = 0;
            uint height = 0;
            _hmd.GetRecommendedRenderTargetSize(ref width, ref height);

            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);

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

            _textureBounds[eye].uMin = 0;
            _textureBounds[eye].uMax = 1;
            _textureBounds[eye].vMin = 0;
            _textureBounds[eye].vMax = 1;

            return renderTarget;
        }

        public override Matrix GetProjectionMatrix(int eye)
        {
            var mat = _hmd.GetProjectionMatrix((EVREye)eye, 0.1f, 1000.0f);
            return mat.ToXNA();
        }

        public override Matrix GetViewMatrix(int eye, Matrix playerPose)
        {
            var view = _hmd.GetEyeToHeadTransform((EVREye)eye).ToXNA();
            var target = _hmdPose.Translation + Vector3.Transform(Vector3.Forward, _hmdPose.Rotation);
            return Matrix.CreateLookAt(_hmdPose.Translation, target, Vector3.Down) * view;
        }

		public override float GetRenderTargetAspectRatio(int eye) => 1.0f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            VREvent_t evt = new VREvent_t();
            while (_hmd.PollNextEvent(ref evt, (uint)Marshal.SizeOf(evt)))
                ProcessEvent(ref evt);

            _validPoseCount = 0;
            _trackedControllerCount = 0;
            _leftControllerDeviceID = -1;
            _rightControllerDeviceID = -1;

            OpenVR.Compositor.WaitGetPoses(_trackedDevices, _gamePose);

            for (var i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; ++i)
            {
                if (_trackedDevices[i].bPoseIsValid)
                {
                    _validPoseCount++;
                    _devicePoses[i] = _trackedDevices[i].mDeviceToAbsoluteTracking.ToXNA();

                    if (_hmd.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.Controller)
                    {
                        _trackedControllerCount++;

                        if (_hmd.GetControllerRoleForTrackedDeviceIndex((uint)i) == ETrackedControllerRole.LeftHand)
                        {
                            _leftControllerDeviceID = i;
                            _leftControllerPose = _devicePoses[i];
                        }
                        else if (_hmd.GetControllerRoleForTrackedDeviceIndex((uint)i) == ETrackedControllerRole.RightHand)
                        {
                            _rightControllerDeviceID = i;
                            _rightControllerPose = _devicePoses[i];
                        }
                    }
                }
            }

            if (_trackedDevices[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
                _hmdPose = _devicePoses[OpenVR.k_unTrackedDeviceIndex_Hmd];
        }

        private void ProcessEvent(ref VREvent_t evt)
        {
            // Controllers state
        }

		public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
            OpenVR.Compositor.Submit(EVREye.Eye_Left, ref _textures[0], ref _textureBounds[0], EVRSubmitFlags.Submit_Default);
            OpenVR.Compositor.Submit(EVREye.Eye_Right, ref _textures[1], ref _textureBounds[1], EVRSubmitFlags.Submit_Default);
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
	}

    public static class OpenVRExtension
    {
        public static Matrix ToXNA(this HmdMatrix34_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m4, mat.m8, 0.0f,
                mat.m1, mat.m5, mat.m9, 0.0f,
                mat.m2, mat.m6, mat.m10, 0.0f,
                mat.m3, mat.m7, mat.m11, 1.0f);

            return m;
        }

        public static Matrix ToXNA(this HmdMatrix44_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m4, mat.m8, mat.m12,
                mat.m1, mat.m5, mat.m9, mat.m13,
                mat.m2, mat.m6, mat.m10, mat.m14,
                mat.m3, mat.m7, mat.m11, mat.m15);

            return m;
        }
    }
}
