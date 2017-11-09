using C3DE.Components.Lighting;
using C3DE.Components.Rendering;

namespace C3DE.Graphics.Materials
{
    public interface IMultipassLightingMaterial
    {
        string LightPassName { get; }
        void LightPass(Renderer renderer, Light light);
    }
}
