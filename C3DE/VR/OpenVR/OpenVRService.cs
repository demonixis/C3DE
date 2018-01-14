using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Valve.VR;
using System.Runtime.InteropServices;

namespace C3DE.VR
{
    public class OpenVRService : VRService
    {
        private CVRSystem m_System;
        private TrackedDevicePose_t[] m_TrackedDevices;
        private TrackedDevicePose_t[] m_GamePose;
        private Texture_t[] m_Textures;
        private VRTextureBounds_t[] m_TextureBounds;
        private VREvent_t m_VREvent;
        private Matrix[] m_DevicePoses;
        private Matrix m_HMDPose;
        private OpenVRController[] m_Controllers;

        public override SpriteEffects PreviewRenderEffect => SpriteEffects.FlipHorizontally;

        public OpenVRService(Game game)
            : base(game)
        {
            m_Textures = new Texture_t[2];
            m_TextureBounds = new VRTextureBounds_t[2];
            m_TrackedDevices = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            m_GamePose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            m_DevicePoses = new Matrix[OpenVR.k_unMaxTrackedDeviceCount];
            m_VREvent = new VREvent_t();
            m_Controllers = new OpenVRController[2];
            m_Controllers[0] = new OpenVRController();
            m_Controllers[1] = new OpenVRController();
        }

        public override int TryInitialize()
        {
            var error = EVRInitError.None;

            m_System = OpenVR.Init(ref error);

            if (error != EVRInitError.None)
                return -1;

            var strDriver = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String);
            var strDisplay = GetTrackedDeviceString(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String);

            Debug.LogFormat("Driver: {0} - Display {1}", strDriver, strDisplay);

            PreviewRenderEffect = SpriteEffects.FlipVertically;

            return 0;
        }

        public override uint[] GetRenderTargetSize()
        {
            uint width = 0;
            uint height = 0;
            m_System.GetRecommendedRenderTargetSize(ref width, ref height);
            return new[] { width, height };
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
            m_System.GetRecommendedRenderTargetSize(ref width, ref height);

            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8, 2, RenderTargetUsage.PreserveContents);

            m_Textures[eye] = new Texture_t();

#if WINDOWS
            var info = typeof(RenderTarget2D).GetField("_texture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = info.GetValue(renderTarget) as SharpDX.Direct3D11.Resource;
            m_Textures[eye].handle = handle.NativePointer;
            m_Textures[eye].eType = ETextureType.DirectX;
#else
            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var glTexture = (int)info.GetValue(renderTarget);
            m_Textures[eye].handle = new IntPtr(glTexture);
            m_Textures[eye].eType = ETextureType.OpenGL;
#endif
            m_Textures[eye].eColorSpace = EColorSpace.Auto;
            m_TextureBounds[eye].uMin = 0;
            m_TextureBounds[eye].uMax = 1;
            m_TextureBounds[eye].vMin = 0;
            m_TextureBounds[eye].vMax = 1;

            return renderTarget;
        }

        public string GetTrackedDeviceString(uint deviceIndex, ETrackedDeviceProperty prop)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var bufferSize = m_System.GetStringTrackedDeviceProperty(deviceIndex, prop, null, 0, ref error);
            if (bufferSize == 0)
                return string.Empty;

            var buffer = new System.Text.StringBuilder((int)bufferSize);
            bufferSize = m_System.GetStringTrackedDeviceProperty(deviceIndex, prop, buffer, bufferSize, ref error);

            return buffer.ToString();
        }

        public override Matrix GetProjectionMatrix(int eye)
        {
            var mat = m_System.GetProjectionMatrix((EVREye)eye, 0.1f, 1000.0f);
            return mat.ToXNA();
        }

        public override Matrix GetViewMatrix(int eye, Matrix parent)
        {
            var matrixEyePos = m_System.GetEyeToHeadTransform((EVREye)eye).ToXNA();
            return Matrix.Invert(parent) * (m_HMDPose * matrixEyePos);
        }

        public override float GetRenderTargetAspectRatio(int eye) => 1.0f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            while (m_System.PollNextEvent(ref m_VREvent, (uint)Marshal.SizeOf(m_VREvent)))
                ProcessEvent(ref m_VREvent);

            OpenVR.Compositor.WaitGetPoses(m_TrackedDevices, m_GamePose);

            for (var i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; ++i)
            {
                if (m_TrackedDevices[i].bPoseIsValid)
                {
                    m_DevicePoses[i] = m_TrackedDevices[i].mDeviceToAbsoluteTracking.ToXNA();

                    if (m_System.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.Controller)
                    {
                        if (m_System.GetControllerRoleForTrackedDeviceIndex((uint)i) == ETrackedControllerRole.LeftHand)
                            m_Controllers[0].Update(m_System, i, m_DevicePoses[i]);
                        else if (m_System.GetControllerRoleForTrackedDeviceIndex((uint)i) == ETrackedControllerRole.RightHand)
                            m_Controllers[1].Update(m_System, i, m_DevicePoses[i]);
                    }
                }
            }

            if (m_TrackedDevices[OpenVR.k_unTrackedDeviceIndex_Hmd].bPoseIsValid)
                m_HMDPose = m_DevicePoses[OpenVR.k_unTrackedDeviceIndex_Hmd];
        }

        private void ProcessEvent(ref VREvent_t evt)
        {
        }

        public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
            OpenVR.Compositor.Submit(EVREye.Eye_Left, ref m_Textures[0], ref m_TextureBounds[0], EVRSubmitFlags.Submit_Default);
            OpenVR.Compositor.Submit(EVREye.Eye_Right, ref m_Textures[1], ref m_TextureBounds[1], EVRSubmitFlags.Submit_Default);
            return 0;
        }

        #region Controllers Management

        public override void GetLocalPosition(int hand, ref Vector3 position)
        {
            m_Controllers[hand].GetRelativePosition(ref position);
        }

        public override void GetLocalRotation(int hand, ref Quaternion quaternion)
        {
            m_Controllers[hand].GetRelativeRotation(ref quaternion);
        }

        public override bool GetButton(int hand, XRButton button)
        {
            if (button == XRButton.Trigger)
                return m_Controllers[hand].GetPress(EVRButtonId.k_EButton_SteamVR_Trigger);
            else if (button == XRButton.Menu)
                return m_Controllers[hand].GetPress(EVRButtonId.k_EButton_ApplicationMenu);
            else if (button == XRButton.Grip)
                return m_Controllers[hand].GetPress(EVRButtonId.k_EButton_Grip);

            return false;
        }

        public override bool GetButtonDown(int hand, XRButton button)
        {
            if (button == XRButton.Trigger)
                return m_Controllers[hand].GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger);
            else if (button == XRButton.Menu)
                return m_Controllers[hand].GetPressDown(EVRButtonId.k_EButton_ApplicationMenu);
            else if (button == XRButton.Grip)
                return m_Controllers[hand].GetPressDown(EVRButtonId.k_EButton_Grip);

            return false;
        }

        public override bool GetButtonUp(int hand, XRButton button)
        {
            if (button == XRButton.Trigger)
                return m_Controllers[hand].GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger);
            else if (button == XRButton.Menu)
                return m_Controllers[hand].GetPressUp(EVRButtonId.k_EButton_ApplicationMenu);
            else if (button == XRButton.Grip)
                return m_Controllers[hand].GetPressUp(EVRButtonId.k_EButton_Grip);

            return false;
        }

        public override float GetAxis(int hand, XRAxis axis)
        {
            Vector2 result = Vector2.Zero;
            m_Controllers[hand].GetAxis(EVRButtonId.k_EButton_Axis0, ref result);
            return axis == XRAxis.TouchpadX ? result.X : result.Y;
        }

        public override Vector2 GetAxis2D(int hand, XRAxis2D axis)
        {
            Vector2 result = Vector2.Zero;
            m_Controllers[hand].GetAxis(EVRButtonId.k_EButton_Axis0, ref result);
            return result;
        }

        #endregion
    }
}
