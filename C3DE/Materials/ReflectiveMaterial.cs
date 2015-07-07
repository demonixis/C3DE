using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class ReflectiveMaterial : Material
    {
        private Vector3 _reflectionColor;

        public TextureCube ReflectionMap { get; set; }

        public Color ReflectionColor
        {
            get { return new Color(_reflectionColor); }
            set { _reflectionColor = value.ToVector3(); }
        }

        public ReflectiveMaterial(Scene scene)
            : base(scene)
        {

        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/ReflectionEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);

            // Material
            effect.Parameters["EyePosition"].SetValue(camera.SceneObject.Transform.Position);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["MainTextureEnabled"].SetValue(diffuseTexture != null);

            if (diffuseTexture != null)
                effect.Parameters["MainTexture"].SetValue(diffuseTexture);

            effect.Parameters["ReflectionColor"].SetValue(diffuseColor);
            effect.Parameters["ReflectiveTexture"].SetValue(ReflectionMap);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
