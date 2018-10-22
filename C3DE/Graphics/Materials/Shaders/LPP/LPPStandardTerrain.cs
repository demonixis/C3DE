using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class LPPStandardTerrain : LPPShader
    {
        private StandardTerrainMaterial m_Material;

        public LPPStandardTerrain(StandardTerrainMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/LPP/Standard");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
        }

        public override void Pass(Renderer renderable, RenderTarget2D lightmap)
        {
            m_Effect.Parameters["World"].SetValue(renderable.m_Transform.m_WorldMatrix);
            m_Effect.Parameters["MainTexture"].SetValue(renderable.material.MainTexture);
            m_Effect.Parameters["AmbientColor"].SetValue(Scene.current.RenderSettings.ambientColor);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_Material.m_DiffuseColor);
            m_Effect.Parameters["LightMap"].SetValue(lightmap);
            m_Effect.Parameters["Viewport"].SetValue(new Vector2(Screen.Width, Screen.Height));
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
