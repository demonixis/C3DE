using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    public struct LightData
    {
        public Vector3 Ambient;
        public Vector3[] Positions;
        public Vector3[] Colors;
        public Vector4[] Data;
        public Vector4[] SpotData;
        public int Count;
    }

    public struct ShadowData
    {
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;
        public Vector3 Data;
        public RenderTarget2D ShadowMap;
    }
}
