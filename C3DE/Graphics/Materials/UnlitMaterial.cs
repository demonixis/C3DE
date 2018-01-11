using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class UnlitMaterial : Material
    {
        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                m_ShaderMaterial = new ForwardUnlit(this);
            else
                throw new System.NotSupportedException("Unlit is not supported with this renderer");

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
