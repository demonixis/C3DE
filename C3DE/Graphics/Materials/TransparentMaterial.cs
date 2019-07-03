using C3DE.Graphics.Materials.Shaders;
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
            _hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer || renderer is LightPrePassRenderer)
                _shaderMaterial = new ForwardTransparent(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredTransparent(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
