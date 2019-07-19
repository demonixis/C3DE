using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardPBRWater : ForwardPBRBase
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
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime);

            base.PrePass(ref cameraPosition, ref viewMatrix, ref projectionMatrix, ref lightData, ref shadowData, ref fogData);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMAOMap"].SetValue(_material._rmaoMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);
            _effect.Parameters["Alpha"].SetValue(_material.Alpha);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
