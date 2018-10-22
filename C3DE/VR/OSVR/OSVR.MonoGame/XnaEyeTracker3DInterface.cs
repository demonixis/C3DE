using System;
using Microsoft.Xna.Framework;
using OSVR.ClientKit;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// XNA Interface for EyeTracker3D data.
        /// </summary>
        public class XnaEyeTracker3DInterface : OSVR.ClientKit.InterfaceAdapter<EyeTracker3DState, XnaEyeTracker3DState>
        {
            public XnaEyeTracker3DInterface(IInterface<EyeTracker3DState> iface)
                : base(iface)
            { }

            protected override XnaEyeTracker3DState Convert(EyeTracker3DState sourceValue)
            {
                return Math.ConvertEyeTracker3DState(sourceValue);
            }
        }
    }
}