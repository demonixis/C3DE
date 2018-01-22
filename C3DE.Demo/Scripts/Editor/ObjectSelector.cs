using C3DE.Components.Rendering;

namespace C3DE.Editor
{
    public class ObjectSelector
    {
        public GameObject GameObject { get; private set; }
        private BoundingBoxRenderer _boundingBoxRenderer;
        private Renderer _renderer;

        public void Set(GameObject sceneObject)
        {
            GameObject = sceneObject;

            _boundingBoxRenderer = sceneObject.GetComponent<BoundingBoxRenderer>();
            if (_boundingBoxRenderer == null)
                _boundingBoxRenderer = sceneObject.AddComponent<BoundingBoxRenderer>();

            _renderer = sceneObject.GetComponent<Renderer>();
        }

        public void Select(bool isSelected)
        {
            if (GameObject != null)
            {
                _boundingBoxRenderer.Enabled = isSelected;

                if (!isSelected)
                {
                    _renderer = null;
                    _boundingBoxRenderer = null;
                    GameObject = null;
                }
            }
        }

        public bool IsEqualTo(GameObject other)
        {
            if (GameObject == null)
                return false;

            return other == GameObject;
        }

        public bool IsNull()
        {
            return GameObject == null;
        }
    }
}