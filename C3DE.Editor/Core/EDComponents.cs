using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using System.Runtime.Serialization;

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
