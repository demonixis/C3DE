using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class FresnelMaterial : Material
    {
        public FresnelMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/FresnelEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
