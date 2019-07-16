using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardPBRLava : ForwardPBRBase
    {
        private PBRLavaMaterial _material;
        private Vector2 _features;

        public ForwardPBRLava(PBRLavaMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRLava");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
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
            }

            _effect.Parameters["IrradianceMap"].SetValue(Scene.current.RenderSettings.skybox.IrradianceTexture);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["Roughness"].SetValue(_material.Roughness);
            _effect.Parameters["Metallic"].SetValue(_material.Metallic);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
