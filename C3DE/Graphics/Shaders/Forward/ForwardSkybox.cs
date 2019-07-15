using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
   public class ForwardSkybox : SkyboxShaderMaterial
    {
        private Skybox _skybox;

        public ForwardSkybox(Skybox skybox)
        {
            _skybox = skybox;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Skybox");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["SkyboxCubeMap"].SetValue(_skybox.Texture);
            _effect.Parameters["World"].SetValue(_skybox.WorldMatrix);
            _effect.Parameters["FogEnabled"].SetValue(_skybox.FogSupported);
            _effect.Parameters["FogData"].SetValue(_skybox.OverrideFog ? _skybox.CustomFogData : Scene.current.RenderSettings.fogData);
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            PrePass(ref cameraPosition, ref viewMatrix, ref projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
        }
    }
}
