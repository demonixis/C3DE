using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class LavaMaterial : Material
    {
        private float _totalTime;

        public Texture2D NormalMap;

        public LavaMaterial(Scene scene)
            : base(scene)
        {
            DiffuseColor = Color.White;
            _totalTime = 0.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/LavaEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(RenderableComponent renderable)
        {
            _totalTime += Time.DeltaTime / 10.0f;

            effect.Parameters["Time"].SetValue(_totalTime);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
