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
            var world = Matrix.CreateConstrainedBillboard(renderable.transform.Position, Camera.Main.transform.Position, Vector3.Up, Camera.Main.transform.Forward, renderable.transform.Forward);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["MainTexture"].SetValue(diffuseTexture);
            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
