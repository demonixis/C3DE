using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class UnlitMaterial : Material
    {
        public UnlitMaterial()
            : this(null)
        {
        }

        public UnlitMaterial(Scene scene, string name = "Unlit Material")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Unlit");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["World"].SetValue(renderable.GameObject.Transform.world);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
