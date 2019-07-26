using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandardTerrain : ShaderMaterial
    {
        private StandardTerrainMaterial _material;

        public DeferredStandardTerrain(StandardTerrainMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/StandardTerrain");
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            // Material
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["SandTexture"].SetValue(_material.SandTexture);
            _effect.Parameters["RockTexture"].SetValue(_material.RockNormalMap);
            _effect.Parameters["SnowTexture"].SetValue(_material.SnowTexture);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightTexture);
            
            // Specular
            _effect.Parameters["SpecularColor"].SetValue(_material.SpecularColor.ToVector3());
            _effect.Parameters["SpecularIntensity"].SetValue(_material.SpecularIntensity);
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
