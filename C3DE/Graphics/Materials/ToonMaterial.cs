using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class ToonMaterial : Material
    {
        private Vector3 _emissiveColor;

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(_emissiveColor); }
            set { _emissiveColor = value.ToVector3(); }
        }

        public ToonMaterial(Scene scene, string name = "Toon Material")
            : base(scene)
        {
            m_DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _emissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/ToonEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            m_Effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);

            if (scene.lights.Count > 0)
            {
                var light0 = scene.Lights[0]; // FIXME
                m_Effect.Parameters["LightDirection"].SetValue(light0.m_Transform.LocalRotation);
            }
        }

        public override void Pass(Renderer renderable)
        {
            // Material
            
            m_Effect.Parameters["DiffuseColor"].SetValue(m_DiffuseColor);
            m_Effect.Parameters["EmissiveColor"].SetValue(_emissiveColor);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["World"].SetValue(renderable.GameObject.Transform.m_WorldMatrix);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
