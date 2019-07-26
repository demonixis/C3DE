using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandardLava : ShaderMaterial
    {
        private StandardLavaMaterial _material;

        public DeferredStandardLava(StandardLavaMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Lava");
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            // Material
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            // Normal
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            // Emissive
           // _effect.Parameters["EmissiveColor"].SetValue(_material.EmissiveColor.ToVector3());
           // _effect.Parameters["EmissiveIntensity"].SetValue(_material.EmissiveIntensity);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
