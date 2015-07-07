using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class UnlitMaterial : Material
    {
        public UnlitMaterial()
            : this(null)
        {
        }

        public UnlitMaterial(Scene scene)
            : base(scene)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/Unlit");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
