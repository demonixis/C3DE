using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class FresnelMaterial : Material
    {
        public FresnelMaterial(Scene scene, string name = "Fresnel Material")
            : base(scene)
        {
            m_DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/FresnelEffect");
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            m_Effect.Parameters["EyePosition"].SetValue(camera.Transform.LocalPosition);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["World"].SetValue(renderable.Transform.m_WorldMatrix);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
