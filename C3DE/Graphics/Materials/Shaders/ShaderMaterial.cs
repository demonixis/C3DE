using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public abstract class ShaderMaterial
    {
        protected internal Effect m_Effect;

        public abstract void LoadEffect(ContentManager content);

        public abstract void PrePass(Camera camera);

        public abstract void Pass(Renderer renderable);
    }
}
