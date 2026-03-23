using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// CPU-side SoA buffers mirrored to the forward lighting shader arrays.
    /// Data.x encodes the GPU light type: 0 directional, 1 point, 2 spot.
    /// Data.yzw encode intensity, range and falloff.
    /// SpotData.xyz stores the normalized spot direction when applicable.
    /// </summary>
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
