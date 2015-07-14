using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Materials
{
    [DataContract]
    public class SimpleMaterial : Material
    {
        private Vector3 _emissiveColor;
        private float _alpha;

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        [DataMember]
        public float Alpha
        {
            get { return _alpha; }
            set
            {
                if (value >= 0.0f && value <= 1.0f)
                    _alpha = value;
            }
        }

        [DataMember]
        public bool AlphaEnabled { get; set; }

        public SimpleMaterial()
            : this(null)
        {
        }

        public SimpleMaterial(Scene scene, string name = "Simple Material")
            : base(scene)
        {
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
            _alpha = 1.0f;
            Name = name;
            AlphaEnabled = true;
            hasAlpha = true;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/SimpleEffect");
        }

        public override void PrePass(Camera camera)
        {
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection); 
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["Alpha"].SetValue(_alpha);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[AlphaEnabled ? 0 : 1].Apply();
        }
    }
}
