using System;
using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBR : ShaderMaterial
    {
        private PBRMaterial _material;

        public ForwardPBR(PBRMaterial material)
        {
            _material = material;
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
            _effect.Parameters["GammaCorrection"].SetValue(2.2f);

            var light = Scene.current.lights[0];
            _effect.Parameters["LightPosition"].SetValue(light.Transform.Position);
            _effect.Parameters["LightColor"].SetValue(light.Color.ToVector3());
        }

        public override void Pass(Renderer renderable)
        {
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMSMap"].SetValue(_material.RMSMap);
            _effect.Parameters["AOMap"].SetValue(_material.AOMap);
            _effect.Parameters["Debug"].SetValue(1);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
