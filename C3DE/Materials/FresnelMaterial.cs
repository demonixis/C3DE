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

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.ProjectionMatrix);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.WorldMatrix);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
