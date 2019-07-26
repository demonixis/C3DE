using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredUnlit : ShaderMaterial
    {
        UnlitMaterial _material;

        public DeferredUnlit(UnlitMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Unlit");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["DiffuseColor"].SetValue(_material.DiffuseColor.ToVector3());
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);

            _effect.CurrentTechnique.Passes[_material.MainTexture == null ? 0 : 1].Apply();
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
         
        }
    }
}
