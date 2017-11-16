using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class ReflectiveMaterial : Material
    {
        private Vector3 _reflectionColor;

        public TextureCube ReflectionMap { get; set; }

        [DataMember]
        public Color ReflectionColor
        {
            get { return new Color(_reflectionColor); }
            set { _reflectionColor = value.ToVector3(); }
        }

        public ReflectiveMaterial(Scene scene, string name = "Reflective Material")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/ReflectionEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection);

            // Material
            m_Effect.Parameters["EyePosition"].SetValue(camera.GameObject.Transform.LocalPosition);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["MainTextureEnabled"].SetValue(MainTexture != null);

            if (MainTexture != null)
                m_Effect.Parameters["MainTexture"].SetValue(MainTexture);

            m_Effect.Parameters["ReflectionColor"].SetValue(diffuseColor);
            m_Effect.Parameters["ReflectiveTexture"].SetValue(ReflectionMap);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["World"].SetValue(renderable.GameObject.Transform.world);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
