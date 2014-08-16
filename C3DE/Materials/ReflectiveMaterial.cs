using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class ReflectiveMaterial : Material
    {
        private Matrix _worldInvertTranspose;
        private Vector4 _reflectionColor;

        public TextureCube TextureCube { get; set; }
        
        public Color ReflectionColor
        {
            get { return new Color(_reflectionColor); }
            set { _reflectionColor = value.ToVector4(); }
        }

        public ReflectiveMaterial(Scene scene)
            : base(scene)
        {

        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/ReflectionEffect");
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // Material
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);
        }

        public override void Pass(RenderableComponent renderable)
        {
            _worldInvertTranspose = Matrix.Transpose(Matrix.Invert(renderable.SceneObject.Transform.world));

            effect.Parameters["TintColor"].SetValue(diffuseColor);
            effect.Parameters["ReflectiveTexture"].SetValue(TextureCube);
            //effect.Parameters["TextureTiling"].SetValue(Tiling);
            //effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.Parameters["WorldInverseTranspose"].SetValue(_worldInvertTranspose);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
