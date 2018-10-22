using System;
using Microsoft.Xna.Framework;
using OSVR.ClientKit;

namespace OSVR
{
    namespace MonoGame
    {
        /// <summary>
        /// XNA adapter for 2D interface data.
        /// </summary>
        public class XnaPosition2DInterface : InterfaceAdapter<Vec2, Vector2>
        {
            public XnaPosition2DInterface(IInterface<Vec2> iface)
                : base(iface)
            { }

            protected override Vector2 Convert(Vec2 sourceValue)
            {
                return Math.ConvertPosition(sourceValue);
            }
        }
    }
}