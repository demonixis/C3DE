using C3DE.Components;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs
{
    public abstract class Prefab : SceneObject
    {
        public Prefab(string name, Scene scene)
            : base(name)
        {
            this.scene = scene;
            this.scene.Add(this);
        }
    }
}
