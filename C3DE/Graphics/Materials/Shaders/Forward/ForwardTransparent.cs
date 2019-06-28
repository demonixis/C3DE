using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardTransparent : ShaderMaterial
    {
        private TransparentMaterial _material;

        public ForwardTransparent(TransparentMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Transparent");
        }

        public override void PrePass(Camera camera)
        {
            _effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            _effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["AmbientColor"].SetValue(Scene.current.RenderSettings.ambientColor);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["World"].SetValue(renderable.GameObject.Transform.m_WorldMatrix);
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
