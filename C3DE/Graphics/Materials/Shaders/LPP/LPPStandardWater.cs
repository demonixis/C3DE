using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class LPPStandardWater : LPPShader
    {
        private StandardWaterMaterial m_Material;

        public LPPStandardWater(StandardWaterMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/LPP/Standard");
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            _effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
        }

        public override void Pass(Renderer renderable, RenderTarget2D lightmap)
        {
            _effect.Parameters["World"].SetValue(renderable.m_Transform._worldMatrix);
            _effect.Parameters["MainTexture"].SetValue(renderable.material.MainTexture);
            _effect.Parameters["AmbientColor"].SetValue(Scene.current.RenderSettings.ambientColor);
            _effect.Parameters["DiffuseColor"].SetValue(m_Material._diffuseColor);
            _effect.Parameters["LightMap"].SetValue(lightmap);
            _effect.Parameters["Viewport"].SetValue(new Vector2(Screen.Width, Screen.Height));
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
