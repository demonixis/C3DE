using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class TransparentMaterial : Material
    {
        public TransparentMaterial(Scene scene, string name = "Transparent Material")
            : base(scene)
        {
            m_DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            m_hasAlpha = true;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/TransparentEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_DiffuseColor);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["World"].SetValue(renderable.GameObject.Transform.m_WorldMatrix);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
