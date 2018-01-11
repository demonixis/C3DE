using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class TransparentMaterial : Material
    {
        public TransparentMaterial(string name = "Transparent Material")
            : base(name)
        {
            m_hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardTransparent(this);
            else
                throw new System.NotSupportedException("Unlit is not supported with this renderer");

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
