using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandard : ShaderMaterial
    {
        private StandardMaterial m_Material;

        public DeferredStandard(StandardMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Deferred/Standard");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["World"].SetValue(renderable.m_Transform.m_WorldMatrix);
            m_Effect.Parameters["Texture"].SetValue(renderable.material.MainTexture);
            m_Effect.Parameters["AmbientColor"].SetValue(Scene.current.RenderSettings.ambientColor);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_Material.m_DiffuseColor);
            m_Effect.Parameters["NormalTextureEnabled"].SetValue(m_Material.NormalTexture != null);
            m_Effect.Parameters["NormalMap"].SetValue(m_Material.NormalTexture);
            m_Effect.Parameters["SpecularTextureEnabled"].SetValue(m_Material.SpecularTexture != null);
            m_Effect.Parameters["SpecularMap"].SetValue(m_Material.SpecularTexture);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
