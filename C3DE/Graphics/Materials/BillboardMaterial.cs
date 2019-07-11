namespace C3DE.Graphics.Materials
{
    public class BillboardMaterial : TransparentMaterial
    {
        public BillboardMaterial() : base() { }

        /*public override void Pass(Renderer renderable)
         {
             var world = Matrix.CreateConstrainedBillboard(renderable.m_Transform.LocalPosition, Camera.Main.m_Transform.LocalPosition, Vector3.Up, Camera.Main.m_Transform.Forward, renderable.m_Transform.Forward);
             m_Effect.Parameters["World"].SetValue(world);
             m_Effect.Parameters["MainTexture"].SetValue(MainTexture);
             m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
             m_Effect.Parameters["TextureOffset"].SetValue(Offset);
             m_Effect.CurrentTechnique.Passes[0].Apply();
         }*/
    }
}
