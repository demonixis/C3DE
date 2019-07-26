using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardLava : ForwardStandardBase
    {
        private StandardLavaMaterial _material;
        private Vector2 _features;

        public bool EmissiveEnabled => false;

        public ForwardStandardLava(StandardLavaMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            var shaderPath = "Shaders/Forward/StandardLava";

            if (GraphicsAPI == GraphicsAPI.OpenGL)
                shaderPath = "Shaders/Forward/OpenGL/StandardLava";

            _effect = content.Load<Effect>(shaderPath);
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["LightCount"].SetValue(lightData.Count);

            if (lightData.Count > 0)
            {
                _effect.Parameters["LightPosition"].SetValue(lightData.Positions);
                _effect.Parameters["LightColor"].SetValue(lightData.Colors);
                _effect.Parameters["LightData"].SetValue(lightData.Data);

                if (GraphicsAPI == GraphicsAPI.Direct3D)
                    _effect.Parameters["SpotData"].SetValue(lightData.SpotData);
            }

            _effect.Parameters["AmbientColor"].SetValue(lightData.Ambient);

            if (GraphicsAPI == GraphicsAPI.Direct3D)
                _effect.Parameters["FogData"].SetValue(fogData);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["SpecularColor"].SetValue(_material.SpecularColor.ToVector3());
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);
            _effect.Parameters["SpecularIntensity"].SetValue(_material.SpecularIntensity);
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);

            if (GraphicsAPI == GraphicsAPI.Direct3D)
            {
                _features.X = _material.NormalMap != null ? 1 : 0;
                _features.Y = _material.SpecularMap != null ? 1 : 0;
                _effect.Parameters["Features"].SetValue(_features);
                _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
                _effect.Parameters["SpecularMap"].SetValue(_material.SpecularMap);
            }

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
