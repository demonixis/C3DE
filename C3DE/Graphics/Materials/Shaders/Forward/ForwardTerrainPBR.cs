using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardTerrainPBR : ShaderMaterial
    {
        private readonly int MaxLightCount;
        private PBRTerrainMaterial _material;
        private Vector2 _features;

        public ForwardTerrainPBR(PBRTerrainMaterial material)
        {
#if DESKTOP
            MaxLightCount = 8;
#else
            MaxLightCount = 64;
#endif

            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/TerrainPBR");
            SetupParamaters();
        }

        protected virtual void SetupParamaters()
        {
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera._viewMatrix);
            _effect.Parameters["Projection"].SetValue(camera._projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            _effect.Parameters["GammaCorrection"].SetValue(2.2f);

            var lights = Scene.current.lights;
            var nbLight = lights.Count;

            if (nbLight > MaxLightCount)
                nbLight = MaxLightCount;

            var pos = new Vector3[nbLight];
            var col = new Vector3[nbLight];
            var lightData = new Vector3[nbLight];
            var shadow = false;

            for (var i = 0; i < nbLight; i++)
            {
                pos[i] = lights[i].Transform.Position;
                col[i] = lights[i]._color;

                var directional = lights[i].TypeLight == LightType.Directional;
                lightData[i].X = directional ? 0 : 1;
                lightData[i].Y = lights[i].Intensity;
                lightData[i].Z = lights[i].Radius;

                if (!shadow && lights[i].ShadowEnabled)
                {
                    _effect.Parameters["ShadowStrength"].SetValue(lights[i]._shadowGenerator.ShadowStrength);
                    _effect.Parameters["ShadowBias"].SetValue(lights[i]._shadowGenerator.ShadowBias);
                    _effect.Parameters["ShadowMap"].SetValue(lights[i]._shadowGenerator.ShadowMap);
                    _effect.Parameters["LightView"].SetValue(lights[i]._viewMatrix);
                    _effect.Parameters["LightProjection"].SetValue(lights[i]._projectionMatrix);
                    shadow = true;
                }
            }

            _effect.Parameters["LightCount"].SetValue(nbLight);
            _effect.Parameters["LightPosition"].SetValue(pos);
            _effect.Parameters["LightColor"].SetValue(col);
            _effect.Parameters["LightData"].SetValue(lightData);
            _effect.Parameters["IrradianceMap"].SetValue(Scene.current.RenderSettings.skybox.IrradianceTexture);
        }

        public override void Pass(Renderer renderable)
        {
            var normalEnabled = _material.GrassNormalMap != null &&
                _material.RockNormalMap != null &&
                _material.SandNormalMap != null &&
                _material.SnownNormalMap != null;

            _features.X = normalEnabled ? 1 : 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["GrassTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["GrassNormalMap"].SetValue(_material.GrassNormalMap);
            _effect.Parameters["SandTexture"].SetValue(_material.SandTexture);
            _effect.Parameters["SandNormalMap"].SetValue(_material.SandNormalMap);
            _effect.Parameters["RockTexture"].SetValue(_material.RockTexture);
            _effect.Parameters["RockNormalMap"].SetValue(_material.RockNormalMap);
            _effect.Parameters["SnowTexture"].SetValue(_material.SnowTexture);
            _effect.Parameters["SnowNormalMap"].SetValue(_material.SnownNormalMap);
            _effect.Parameters["RMAOMap"].SetValue(_material._rmaoMap);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightTexture);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(renderable.ReceiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
