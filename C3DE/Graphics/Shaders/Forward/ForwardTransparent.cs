using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardTransparent : ForwardShader
    {
        private TransparentMaterial _material;

        public ForwardTransparent(TransparentMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Transparent");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["AmbientColor"].SetValue(Scene.current.RenderSettings.ambientColor);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
