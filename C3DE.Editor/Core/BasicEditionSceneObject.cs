using C3DE.Components.Renderers;
using C3DE.Editor.Events;
using System;

namespace C3DE.Editor.Core
{
    public class BasicEditionSceneObject
    {
        public GameObject Selected { get; set; }
        public GameObject Copy { get; set; }

        public void Reset()
        {
            Selected = null;
            Copy = null;
        }

        public void CopySelection()
        {
            Copy = Selected;
        }

        public void DuplicateSelection(Action<GameObject> addCallback)
        {
            Copy = Selected;
            PastSelection(addCallback);
        }

        public void PastSelection(Action<GameObject> addCallback)
        {
            if (Copy != null)
            {
                var sceneObject = (GameObject)Copy.Clone();

                var previous = Copy;

                addCallback(sceneObject);

                Copy = previous;

                var x = Copy.Transform.Position.X;
                x += Copy.GetComponent<Renderer>().BoundingSphere.Radius * 2.0f;

                sceneObject.Transform.SetPosition(x, null, null);

                if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    Copy = Selected;

                Messenger.Notify(EditorEvent.SceneObjectAdded);
            }
        }
    }
}
