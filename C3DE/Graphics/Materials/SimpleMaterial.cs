using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
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
            m_Effect = content.Load<Effect>("Shaders/SimpleEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection); 
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            
            m_Effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            m_Effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["Alpha"].SetValue(_alpha);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["World"].SetValue(renderable.transform.world);
            m_Effect.CurrentTechnique.Passes[AlphaEnabled ? 0 : 1].Apply();
        }
    }
}
