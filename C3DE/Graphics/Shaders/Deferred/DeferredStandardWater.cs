using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandardWater : ForwardStandardWater
    {
        public DeferredStandardWater(StandardWaterMaterial material) : base(material)
        {
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/StandardWater");
            SetupParamaters();
        }

        public override void PrePass(Camera camera)
        {
            _EPView.SetValue(camera._viewMatrix);
            _EPProjection.SetValue(camera._projectionMatrix);
            _EPEyePosition.SetValue(camera.Transform.LocalPosition);
        }

        public override void Pass(Renderer renderable)
        {
            _EPSpecularLightColor.SetValue(_material.SpecularColor.ToVector3());
            _EPSpecularPower.SetValue(_material.SpecularPower);
            _EPSpecularIntensity.SetValue(_material.SpecularIntensity);
            _EPSpecularTextureEnabled.SetValue(_material.SpecularTexture != null);
            _EPSpecularTexture.SetValue(_material.SpecularTexture);
            base.Pass(renderable);
        }
    }
}
