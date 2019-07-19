using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardUnlit : ShaderMaterial
    {
        private UnlitMaterial _material;

        public ForwardUnlit(UnlitMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Unlit");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["CutoutEnabled"].SetValue(_material.CutoutEnabled);
            _effect.Parameters["Cutout"].SetValue(_material.Cutout);

            _effect.CurrentTechnique.Passes[_material.MainTexture == null ? 0 : 1].Apply(); ;
        }
    }
}
