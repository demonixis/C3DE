using C3DE.Components.Rendering.Particles;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardParticles : ShaderMaterial
    {
        private ParticleMaterial _material;

        public ForwardParticles(ParticleMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Particles");
            _effect.Clone(); //FIXME
        }

        public void Setup(ParticleSettings settings)
        {
            _effect.Parameters["Duration"].SetValue((float)settings.Duration.TotalSeconds);
            _effect.Parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
            _effect.Parameters["Gravity"].SetValue(settings.Gravity);
            _effect.Parameters["EndVelocity"].SetValue(settings.EndVelocity);
            _effect.Parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
            _effect.Parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());
            _effect.Parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));
            _effect.Parameters["StartSize"].SetValue(new Vector2(settings.MinStartSize, settings.MaxStartSize));
            _effect.Parameters["EndSize"].SetValue(new Vector2(settings.MinEndSize, settings.MaxEndSize));
            _effect.Parameters["Texture"].SetValue(_material.MainTexture);
            _effect.Parameters["ViewportScale"].SetValue(new Vector2(0.5f / Application.GraphicsDevice.Viewport.AspectRatio, -0.5f));
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            PrePass(ref cameraPosition, ref viewMatrix, ref projectionMatrix);
        }
    }
}
