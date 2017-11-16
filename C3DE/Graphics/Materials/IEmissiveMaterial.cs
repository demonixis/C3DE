using C3DE.Components.Rendering;

namespace C3DE.Graphics.Materials
{
    public interface IEmissiveMaterial
    {
        bool EmissiveEnabled { get; }
        void EmissivePass(Renderer renderer);
    }
}
