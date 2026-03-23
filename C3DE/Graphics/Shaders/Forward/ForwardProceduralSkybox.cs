using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardProceduralSkybox : ShaderMaterial
    {
        private readonly Skybox _skybox;

        public ForwardProceduralSkybox(Skybox skybox)
        {
            _skybox = skybox;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/ProceduralSkybox");
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            var settings = _skybox.ProceduralSettings;
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["World"].SetValue(_skybox.WorldMatrix);
            _effect.Parameters["CloudNoiseTexture"].SetValue(_skybox.CloudNoiseTexture);
            _effect.Parameters["StarNoiseTexture"].SetValue(_skybox.StarNoiseTexture);
            _effect.Parameters["SunDirection"].SetValue(_skybox.SunSkyDirection);
            _effect.Parameters["MoonDirection"].SetValue(_skybox.MoonSkyDirection);
            _effect.Parameters["SkyParams"].SetValue(new Vector4(
                settings.TimeOfDay,
                _skybox.DayFactor,
                settings.StarIntensity,
                settings.CloudScale));
            _effect.Parameters["CloudParams"].SetValue(new Vector4(
                settings.CloudCoverage,
                settings.CloudSpeed,
                settings.SunIntensity,
                settings.MoonIntensity));
            _effect.Parameters["DayTopColor"].SetValue(settings.DayTopColor.ToVector4());
            _effect.Parameters["DayHorizonColor"].SetValue(settings.DayHorizonColor.ToVector4());
            _effect.Parameters["NightTopColor"].SetValue(settings.NightTopColor.ToVector4());
            _effect.Parameters["NightHorizonColor"].SetValue(settings.NightHorizonColor.ToVector4());
            _effect.Parameters["NightTint"].SetValue(settings.NightTint.ToVector4());
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            PrePass(ref cameraPosition, ref viewMatrix, ref projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
        }
    }
}
