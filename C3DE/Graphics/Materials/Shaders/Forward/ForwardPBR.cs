using System;
using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBR : ShaderMaterial
    {
        private PBRMaterial m_Material;

        public ForwardPBR(PBRMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBR");
            SetupParamaters();
        }

        protected virtual void SetupParamaters()
        { 
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            _effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);

            var light = Scene.current.lights[0];
            _effect.Parameters["LightPosition"].SetValue(light.Transform.Position);
            _effect.Parameters["LightColor"].SetValue(light.Color.ToVector3());
        }

        public override void Pass(Renderer renderable)
        {
            _effect.Parameters["World"].SetValue(renderable.Transform.m_WorldMatrix);
            _effect.Parameters["Exposure"].SetValue(1.0f);

            _effect.Parameters["AlbedoMap"].SetValue(m_Material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(m_Material.NormalMap);
            _effect.Parameters["RMSMap"].SetValue(m_Material.RMSMap);
            _effect.Parameters["AOMap"].SetValue(m_Material.AOMap);
            _effect.Parameters["Debug"].SetValue(1);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
