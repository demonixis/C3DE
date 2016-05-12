using C3DE.Editor.Events;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Logique d'interaction pour SceneObjectControl.xaml
    /// </summary>
    public partial class SceneObjectControl : UserControl
    {
        private GameObject sceneObject;

        public bool SceneObjectEnabled
        {
            get
            {
                if (sceneObject != null)
                    return sceneObject.Enabled;

                return false;
            }
            set
            {
                if (sceneObject != null)
                    sceneObject.Enabled = value;
            }
        }

        public string SceneObjectName
        {
            get
            {
                if (sceneObject != null)
                    return sceneObject.Name;

                return string.Empty;
            }
            set
            {
                if (sceneObject != null)
                {
                    sceneObject.Name = value;
                    Messenger.Notify(EditorEvent.SceneObjectRenamed, sceneObject.Id);
                }
            }
        }

        public SceneObjectControl()
        {
            InitializeComponent();
        }

        public SceneObjectControl(GameObject so)
            : this()
        {
            sceneObject = so;
            DataContext = this;
        }

        public void Set(GameObject so)
        {
            sceneObject = so;
            DataContext = this;
        }
    }
}
