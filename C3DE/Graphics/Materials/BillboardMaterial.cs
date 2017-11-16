using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class BillboardMaterial : TransparentMaterial
    {
        public BillboardMaterial()
            : this(null)
        {
        }

        public BillboardMaterial(Scene scene, string name = "Billboard Material")
            : base(scene)
        {
            Name = name;
        }

        public override void Pass(Renderer renderable)
        {
            var world = Matrix.CreateConstrainedBillboard(renderable.transform.LocalPosition, Camera.Main.transform.LocalPosition, Vector3.Up, Camera.Main.transform.Forward, renderable.transform.Forward);
            m_Effect.Parameters["World"].SetValue(world);
            m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
