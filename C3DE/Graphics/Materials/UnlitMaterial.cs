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
            if (renderer is ForwardRenderer || renderer is LightPrePassRenderer)
                m_ShaderMaterial = new ForwardUnlit(this);
            else if (renderer is DeferredRenderer)
                m_ShaderMaterial = new DeferredUnlit(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
