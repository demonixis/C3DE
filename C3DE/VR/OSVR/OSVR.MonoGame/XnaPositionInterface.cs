using System;
using Microsoft.Xna.Framework;
using OSVR.ClientKit;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// XNA Interface for Position tracking data.
        /// </summary>
        public class XnaPositionInterface : InterfaceAdapter<Vec3, Vector3>
        {
            public XnaPositionInterface(IInterface<Vec3> iface)
                : base(iface)
            { }

            protected override Vector3 Convert(Vec3 sourceValue)
            {
                return Math.ConvertPosition(sourceValue);
            }
        }
    }
}