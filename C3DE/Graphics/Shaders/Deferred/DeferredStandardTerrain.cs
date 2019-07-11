using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandardTerrain : ForwardStandardTerrain
    {
        public DeferredStandardTerrain(StandardTerrainMaterial material) : base(material)
        {
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/StandardTerrain");
            SetupParamaters();
            m_PassAmbient = _effect.CurrentTechnique.Passes[0];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera._viewMatrix);
            m_EPProjection.SetValue(camera._projectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPSpecularLightColor.SetValue(_material.SpecularColor.ToVector3());
            m_EPSpecularPower.SetValue(_material.Shininess);
            m_EPSpecularIntensity.SetValue(_material.SpecularIntensity);
            base.Pass(renderable);
        }
    }
}
