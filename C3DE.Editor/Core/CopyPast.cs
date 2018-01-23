using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Editor
{
    public class CopyPast
    {
        public GameObject Selected { get; set; }
        public GameObject Copy { get; set; }

        public event Action<GameObject> GameObjectAdded = null;

        public void Reset()
        {
            Selected = null;
            Copy = null;
        }

        public void CopySelection()
        {
            Copy = Selected;
        }

        public void DuplicateSelection()
        {
            Copy = Selected;
            GameObjectAdded?.Invoke(Copy);
        }

        public void PastSelection()
        {
            if (Copy != null)
            {
                var sceneObject = (GameObject)Copy.Clone();

                var previous = Copy;

                Copy = previous;

                var x = Copy.Transform.Position.X;
                x += Copy.GetComponent<Renderer>().BoundingSphere.Radius * 2.0f;

                sceneObject.Transform.SetPosition(x, null, null);

                if (Input.Keys.Pressed(Keys.LeftControl))
                    Copy = Selected;

                GameObjectAdded?.Invoke(Copy);
            }
        }
    }
}