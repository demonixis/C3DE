using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders.Forward
{
    public class ForwardPBR : ShaderMaterial
    {
        private readonly int MaxLightCount;
        private PBRMaterial _material;
        private Vector2 _features;

        public ForwardPBR(PBRMaterial material)
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
            _effect = content.Load<Effect>("Shaders/Forward/PBR");
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
            var shadow = false;

            for (var i = 0; i < nbLight; i++)
            {
                pos[i] = lights[i].Transform.Position;
                col[i] = lights[i]._color;

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
            _effect.Parameters["IrradianceMap"].SetValue(Scene.current.RenderSettings.skybox.IrradianceTexture);

#if !DESKTOP && !ANDROID
            _effect.Parameters["Debug"].SetValue(1);
#endif
        }

        public override void Pass(Renderer renderable)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMSAOMap"].SetValue(_material._rmsaoMap);
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(renderable.ReceiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
