using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class ToonMaterial : Material
    {
        private Vector3 _emissiveColor;

        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        public ToonMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/ToonEffect");
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            var light0 = scene.Lights[0]; // FIXME

          
            // Light
            effect.Parameters["LightDirection"].SetValue(light0.Direction);
        }

        public override void Pass(RenderableComponent renderable)
        {
            // Material
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
