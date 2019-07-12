using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public struct LightData
    {
        public Vector3[] Positions;
        public Vector3[] Colors;
        public Vector3[] Data;
        public int Count;
    }

    public struct ShadowData
    {
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;
        public Vector3 Data;
        public RenderTarget2D ShadowMap;
    }

    public abstract class ForwardShader : ShaderMaterial
    {
        public override void Pass(Renderer renderable)
        {
        }

        public override void PrePass(Camera camera)
        {
        }

        public abstract void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData);
        public abstract void Pass(ref Matrix worldMatrix, bool receiveShadow);
    }
}
