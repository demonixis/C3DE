using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public abstract class LPPShader : ShaderMaterial
    {
        public abstract void Pass(Renderer renderable, RenderTarget2D lightmap);
    }
}
