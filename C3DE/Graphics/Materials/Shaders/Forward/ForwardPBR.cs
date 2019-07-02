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
            _effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            _effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            _effect.Parameters["GammaCorrection"].SetValue(2.2f);

            var light = Scene.current.lights;
            var nbLight = light.Count;

            if (nbLight > MaxLightCount)
                nbLight = MaxLightCount;

            var pos = new Vector3[nbLight];
            var col = new Vector3[nbLight];

            for(var i = 0; i < nbLight; i++)
            {
                pos[i] = light[i].Transform.Position;
                col[i] = light[i]._color;
            }

            _effect.Parameters["LightCount"].SetValue(nbLight);
            _effect.Parameters["LightPosition"].SetValue(pos);
            _effect.Parameters["LightColor"].SetValue(col);
        }

        public override void Pass(Renderer renderable)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;

            _effect.Parameters["World"].SetValue(renderable.Transform._worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMSAOMap"].SetValue(_material._rmsaoMap);
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["IrradianceMap"].SetValue(_material.IrradianceMap);
            _effect.Parameters["Features"].SetValue(_features);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
