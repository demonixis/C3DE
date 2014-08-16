using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class SimpleMaterial : Material
    {
        private Vector4 _emissiveColor;
        private float _alpha;

        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector4(); }
        }

        public float Alpha
        {
            get { return _alpha; }
            set
            {
                if (value >= 0.0f && value <= 1.0f)
                    _alpha = value;
            }
        }

        public bool AlphaEnabled { get; set; }

        public SimpleMaterial(Scene scene)
            : base(scene)
        {
            diffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _alpha = 1.0f;
            AlphaEnabled = true;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/SimpleEffect");
        }

        public override void PrePass()
        {
            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection); 
        }

        public override void Pass(RenderableComponent renderable)
        {
            // Material
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["Alpha"].SetValue(_alpha);
            effect.Parameters["MainTexture"].SetValue(mainTexture);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[AlphaEnabled ? 0 : 1].Apply();
        }
    }
}
