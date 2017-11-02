using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Materials
{
    [DataContract]
    public class PreLightMaterial : Material
    {
        private Vector2 _viewport;

        public PreLightMaterial(Scene scene, string name = "PreLightMaterial")
            : base(scene)
        {
            _viewport = new Vector2(Screen.Width, Screen.Height);
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("Shaders/PreLighting/PL_StandardFX");
        }

        public override void PrePass(Camera camera)
        {
            _viewport.X = Screen.Width;
            _viewport.Y = Screen.Height;

            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["Viewport"].SetValue(_viewport);
        }

        public override void Pass(Renderer renderable)
        {
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
			effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
