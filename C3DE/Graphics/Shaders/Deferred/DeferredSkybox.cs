using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredSkybox : ShaderMaterial
    {
        private Skybox _skybox;

        public DeferredSkybox(Skybox skybox)
        {
            _skybox = skybox;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Skybox");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["World"].SetValue(_skybox._worldMatrix);
            _effect.Parameters["Texture"].SetValue(_skybox.Texture);
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
        }
    }
}
