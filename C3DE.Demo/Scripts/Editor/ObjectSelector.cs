using C3DE.Components.Rendering;

namespace C3DE.Editor.Core
{
    public class ObjectSelector
    {
        public GameObject SceneObject { get; private set; }
        private BoundingBoxRenderer _boundingBoxRenderer;
        private Renderer _renderer;

        public void Set(GameObject sceneObject)
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

        public bool IsEqualTo(GameObject other)
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