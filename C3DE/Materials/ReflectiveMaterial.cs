using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class ReflectiveMaterial : Material
    {
        private bool _mainTextureEnabled;
        private Vector3 _reflectionColor;

        public new Texture2D MainTexture
        {
            get { return mainTexture; }
            set
            {
                mainTexture = value;
                _mainTextureEnabled = (value != null);
            }
        }

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

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);

            // Material
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["MainTextureEnabled"].SetValue(_mainTextureEnabled);

            if (_mainTextureEnabled)
                effect.Parameters["MainTexture"].SetValue(mainTexture);

            effect.Parameters["ReflectionColor"].SetValue(diffuseColor);
            effect.Parameters["ReflectiveTexture"].SetValue(ReflectionMap);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
