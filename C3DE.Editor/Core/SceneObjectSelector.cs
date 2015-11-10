using C3DE.Components.Renderers;

namespace C3DE.Editor.Core
{
    public class SceneObjectSelector
    {
        public SceneObject SceneObject { get; private set; }
        private BoundingBoxRenderer _boundingBoxRenderer;
        private Renderer _renderer;

        public SceneObjectSelector()
        {
        }

        public void Set(SceneObject sceneObject)
        {
            SceneObject = sceneObject;

            _boundingBoxRenderer = sceneObject.GetComponent<BoundingBoxRenderer>();
            if (_boundingBoxRenderer == null)
                _boundingBoxRenderer = sceneObject.AddComponent<BoundingBoxRenderer>();

            _renderer = sceneObject.GetComponent<Renderer>();
        }

        public void Select(bool isSelected)
        {
            if (SceneObject != null)
            {
                _boundingBoxRenderer.Enabled = isSelected;

                if (!isSelected)
                {
                    _renderer = null;
                    _boundingBoxRenderer = null;
                    SceneObject = null;
                }
            }
        }

        public bool IsEqualTo(SceneObject other)
        {
            if (SceneObject == null)
                return false;

            return other == SceneObject;
        }

        public bool IsNull()
        {
            return SceneObject == null;
        }
    }
}
