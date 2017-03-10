using System;
using Microsoft.Xna.Framework;
using OSVR.ClientKit;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// XNA Interface for Pose data.
        /// </summary>
        public class XnaPoseInterface : OSVR.ClientKit.InterfaceAdapter<Pose3, XnaPose>
        {
			public XnaPoseInterface(IInterface<Pose3> iface)
				: base(iface)
			{ }

            protected override XnaPose Convert(Pose3 sourceValue)
            {
                return Math.ConvertPose(sourceValue);
            }
        }
    }
}