using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using OSVRQuaternion = OSVR.ClientKit.Quaternion;
using XnaQuaternion = Microsoft.Xna.Framework.Quaternion;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// XNA Interface for Orientation data.
        /// </summary>
        public class XnaOrientationInterface :
            OSVR.ClientKit.InterfaceAdapter<OSVRQuaternion, XnaQuaternion>
        {
            public XnaOrientationInterface(OSVR.ClientKit.IInterface<OSVRQuaternion> iface)
                : base(iface) {}

            protected override XnaQuaternion Convert(OSVRQuaternion sourceValue)
            {
                return Math.ConvertOrientation(sourceValue);
            }
        }
    }
}