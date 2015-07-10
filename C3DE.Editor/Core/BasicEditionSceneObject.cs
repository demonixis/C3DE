using C3DE.Components.Renderers;
using C3DE.Editor.Events;
using System;

namespace C3DE.Editor.Core
{
    public class BasicEditionSceneObject
    {
        public SceneObject Selected { get; set; }
        public SceneObject Copy { get; set; }

        public void Reset()
        {
            Selected = null;
            Copy = null;
        }

        public void CopySelection()
        {
            Copy = Selected;
        }

        public void DuplicateSelection(Action<SceneObject> addCallback)
        {
            Copy = Selected;
            PastSelection(addCallback);
        }

        public void PastSelection(Action<SceneObject> addCallback)
        {
            if (Copy != null)
            {
                var sceneObject = (SceneObject)Copy.Clone();

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
