using C3DE.Components.Physics;
using C3DE.Components.Rendering;

namespace C3DE.Editor.Core
{
    public interface IEDComponent
    {
    }

    public class EDMeshRenderer : MeshRenderer, IEDComponent
    {
    }

    public class EDBoxCollider : BoxCollider, IEDComponent
    {
    }

    public class EDSphereCollider : SphereCollider, IEDComponent
    {
    }
}
