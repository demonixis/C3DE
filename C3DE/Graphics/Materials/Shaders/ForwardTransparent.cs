﻿using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class Transparent : ShaderMaterial
    {
        private TransparentMaterial m_Material;

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/Transparent");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["TextureTiling"].SetValue(m_Material.Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(m_Material.Offset);
            m_Effect.Parameters["AmbientColor"].SetValue(m_Material.scene.RenderSettings.ambientColor);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_Material.m_DiffuseColor);
            m_Effect.Parameters["MainTexture"].SetValue(m_Material.MainTexture);
            m_Effect.Parameters["World"].SetValue(renderable.GameObject.Transform.m_WorldMatrix);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}