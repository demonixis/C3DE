using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class LavaMaterial : Material
    {
        private float _totalTime;

        public Texture2D NormalTexture { get; set; }
        public float EmissiveIntensity { get; set; } = 2.0f;

        public LavaMaterial(Scene scene, string name = "Lava Material")
            : base(scene)
        {
            DiffuseColor = Color.White;
            _totalTime = 0.0f;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/LavaEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection);
        }

        public override void Pass(Renderer renderable)
        {
            _totalTime += Time.DeltaTime / 10.0f;

            m_Effect.Parameters["Time"].SetValue(_totalTime);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_DiffuseColor);
            m_Effect.Parameters["World"].SetValue(renderable.Transform.world);
            m_Effect.Parameters["EmissiveIntensity"].SetValue(EmissiveIntensity);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
