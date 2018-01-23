using C3DE.Components.Rendering;

namespace C3DE.Editor
{
    public class ObjectSelector
    {
        private BoundingBoxRenderer m_BoudingBoxRenderer;
        private Renderer m_Renderer;

        public GameObject GameObject { get; private set; }

        public void Set(GameObject gameObject)
        {
            GameObject = gameObject;

            m_BoudingBoxRenderer = gameObject.GetComponent<BoundingBoxRenderer>();
            if (m_BoudingBoxRenderer == null)
                m_BoudingBoxRenderer = gameObject.AddComponent<BoundingBoxRenderer>();

            m_Renderer = gameObject.GetComponent<Renderer>();
        }

        public void Select(bool isSelected)
        {
            if (GameObject != null)
            {
                m_BoudingBoxRenderer.Enabled = isSelected;

                if (!isSelected)
                {
                    m_Renderer = null;
                    m_BoudingBoxRenderer = null;
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