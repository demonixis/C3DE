using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBRWater : ShaderMaterial
    {
        private PBRWaterMaterial _material;
        private Vector2 _features;

        public ForwardPBRWater(PBRWaterMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRWater");
            SetupParamaters();
        }

        protected virtual void SetupParamaters()
        {
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera._viewMatrix);
            _effect.Parameters["Projection"].SetValue(camera._projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime);
            ForwardPBR.UpdatePBRPrePass(_effect, camera);
        }

        public override void Pass(Renderer renderable)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMAOMap"].SetValue(_material._rmaoMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(renderable.ReceiveShadow);
            _effect.Parameters["Alpha"].SetValue(_material.Alpha);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
