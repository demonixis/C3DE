using C3DE.Components.Rendering;

namespace C3DE.Graphics.Materials
{
    public interface IEmissiveMaterial
    {
        string EmissivePassName { get; }
        void EmissivePass(Renderer renderer);
    }
}
