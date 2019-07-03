using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class UnlitMaterial : Material
    {
        public UnlitMaterial() : base() { }
        public UnlitMaterial(string name) : base(name) { }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer || renderer is LightPrePassRenderer)
                _shaderMaterial = new ForwardUnlit(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredUnlit(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
