using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Valve.VR;

namespace C3DE.VR
{
    public class ButtonMask
    {
        public const ulong System = (1ul << (int)EVRButtonId.k_EButton_System); // reserved
        public const ulong ApplicationMenu = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
        public const ulong Grip = (1ul << (int)EVRButtonId.k_EButton_Grip);
        public const ulong Axis0 = (1ul << (int)EVRButtonId.k_EButton_Axis0);
        public const ulong Axis1 = (1ul << (int)EVRButtonId.k_EButton_Axis1);
        public const ulong Axis2 = (1ul << (int)EVRButtonId.k_EButton_Axis2);
        public const ulong Axis3 = (1ul << (int)EVRButtonId.k_EButton_Axis3);
        public const ulong Axis4 = (1ul << (int)EVRButtonId.k_EButton_Axis4);
        public const ulong Touchpad = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
        public const ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    public class OpenVRController
    {
        private VRControllerState_t state;
        private VRControllerState_t prevState;
        private TrackedDevicePose_t pose;
        private Vector3 m_Position;
        private Quaternion m_Rotation;
        private Vector3 m_Scale;

        public int Index { get; private set; } = -1;
        public bool IsValid { get; private set; }
        public bool IsConnected => pose.bDeviceIsConnected;
        public bool IsTracked => pose.bPoseIsValid;

        public void Update(CVRSystem system, int index, Matrix transform)
        {
            Index = index;

            if (Index < 0)
            {
                IsValid = false;
                pose.bPoseIsValid = false;
                return;
            }

            prevState = state;

            IsValid = system.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, (uint)index, ref state, (uint)Marshal.SizeOf(typeof(VRControllerState_t)), ref pose);

            transform = Matrix.Invert(transform);

            var parent = Components.Camera.Main.Transform.Parent;
            //if (parent != null)
                //transform = parent.m_WorldMatrix * transform;

            transform.Decompose(out m_Scale, out m_Rotation, out m_Position);
        }

        public void GetAxis(EVRButtonId buttonId, ref Vector2 result)
        {
            var axisId = (uint)buttonId - (uint)EVRButtonId.k_EButton_Axis0;
            switch (axisId)
            {
                case 0:
                    result.X = state.rAxis0.x;
                    result.Y = state.rAxis0.y;
                    break;

                case 1:
                    result.X = state.rAxis1.x;
                    result.Y = state.rAxis1.y;
                    break;
                case 2:
                    result.X = state.rAxis2.x;
                    result.Y = state.rAxis2.y;
                    break;
                case 3:
                    result.X = state.rAxis3.x;
                    result.Y = state.rAxis3.y;
                    break;

                case 4:
                    result.X = state.rAxis4.x;
                    result.Y = state.rAxis4.y;
                    break;

                default:
                    result.X = 0;
                    result.Y = 0;
                    break;
            }
        }

        public bool GetPress(ulong buttonMask) => (state.ulButtonPressed & buttonMask) != 0;
        public bool GetPressDown(ulong buttonMask) => (state.ulButtonPressed & buttonMask) != 0 && (prevState.ulButtonPressed & buttonMask) == 0;
        public bool GetPressUp(ulong buttonMask) => (state.ulButtonPressed & buttonMask) == 0 && (prevState.ulButtonPressed & buttonMask) != 0;

        public bool GetPress(EVRButtonId buttonId) => GetPress(1ul << (int)buttonId);
        public bool GetPressDown(EVRButtonId buttonId) => GetPressDown(1ul << (int)buttonId);
        public bool GetPressUp(EVRButtonId buttonId) => GetPressUp(1ul << (int)buttonId);

        public bool GetTouch(ulong buttonMask) => (state.ulButtonTouched & buttonMask) != 0;
        public bool GetTouchDown(ulong buttonMask) => (state.ulButtonTouched & buttonMask) != 0 && (prevState.ulButtonTouched & buttonMask) == 0;
        public bool GetTouchUp(ulong buttonMask) => (state.ulButtonTouched & buttonMask) == 0 && (prevState.ulButtonTouched & buttonMask) != 0;

        public bool GetTouch(EVRButtonId buttonId) => GetTouch(1ul << (int)buttonId);
        public bool GetTouchDown(EVRButtonId buttonId) => GetTouchDown(1ul << (int)buttonId);
        public bool GetTouchUp(EVRButtonId buttonId) => GetTouchUp(1ul << (int)buttonId);

        public void GetRelativePosition(ref Vector3 position) => position = m_Position;
        public void GetRelativeRotation(ref Quaternion rotation) => rotation = m_Rotation;
    }
}
