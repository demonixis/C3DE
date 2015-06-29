using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class PreLightMaterial : Material
    {
        public PreLightMaterial(Scene scene)
            : base(scene)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/PreLighting/PL_StandardFX");
        }

        public override void PrePass()
        {
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["Viewport"].SetValue(new Vector2(Screen.Width, Screen.Height));
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
			effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
