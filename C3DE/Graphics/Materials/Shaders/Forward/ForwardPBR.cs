using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBR : ShaderMaterial
    {
#if WINDOWS
        public const int MaxLightCount = 64;
#else
        public const int MaxLightCount = 8;
#endif

        private PBRMaterial _material;
        private Vector2 _features;

        public ForwardPBR(PBRMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRStandard");
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

            UpdatePBRPrePass(_effect, camera);
        }

        public override void Pass(Renderer renderable)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMAOMap"].SetValue(_material._rmaoMap);
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(renderable.ReceiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public static void UpdatePBRPrePass(Effect effect, Camera camera)
        {
            effect.Parameters["View"].SetValue(camera._viewMatrix);
            effect.Parameters["Projection"].SetValue(camera._projectionMatrix);
            effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);

            var lights = Scene.current.lights;
            var nbLight = lights.Count;

            if (nbLight > ForwardPBR.MaxLightCount)
                nbLight = ForwardPBR.MaxLightCount;

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
                    effect.Parameters["ShadowStrength"].SetValue(lights[i]._shadowGenerator.ShadowStrength);
                    effect.Parameters["ShadowBias"].SetValue(lights[i]._shadowGenerator.ShadowBias);
                    effect.Parameters["ShadowMap"].SetValue(lights[i]._shadowGenerator.ShadowMap);
                    effect.Parameters["LightView"].SetValue(lights[i]._viewMatrix);
                    effect.Parameters["LightProjection"].SetValue(lights[i]._projectionMatrix);
                    shadow = true;
                }
            }

            effect.Parameters["LightCount"].SetValue(nbLight);
            effect.Parameters["LightPosition"].SetValue(pos);
            effect.Parameters["LightColor"].SetValue(col);
            effect.Parameters["LightData"].SetValue(lightData);
            effect.Parameters["IrradianceMap"].SetValue(Scene.current.RenderSettings.skybox.IrradianceTexture);
        }
    }
}
