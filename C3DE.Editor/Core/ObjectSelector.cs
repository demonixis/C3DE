using C3DE.Components.Rendering;

namespace C3DE.Editor
{
    public class ObjectSelector
    {
        private BoundingBoxRenderer _boudingBoxRenderer;
        private Renderer _renderer;

        public GameObject GameObject { get; private set; }

        public void Set(GameObject gameObject)
        {
            GameObject = gameObject;

            _boudingBoxRenderer = gameObject.GetComponent<BoundingBoxRenderer>();
            if (_boudingBoxRenderer == null)
                _boudingBoxRenderer = gameObject.AddComponent<BoundingBoxRenderer>();

            _renderer = gameObject.GetComponent<Renderer>();
        }

        public void Select(bool isSelected)
        {
            if (GameObject != null)
            {
                _boudingBoxRenderer.Enabled = isSelected;

                if (!isSelected)
                {
                    _renderer = null;
                    _boudingBoxRenderer = null;
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