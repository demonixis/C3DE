using C3DE.Editor.Events;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Logique d'interaction pour SceneObjectControl.xaml
    /// </summary>
    public partial class GameObjectControl : UserControl
    {
        private GameObject gameObject;

        public bool SceneObjectEnabled
        {
            get
            {
                if (gameObject != null)
                    return gameObject.Enabled;

                return false;
            }
            set
            {
                if (gameObject != null)
                    gameObject.Enabled = value;
            }
        }

        public string SceneObjectName
        {
            get
            {
                if (gameObject != null)
                    return gameObject.Name;

                return string.Empty;
            }
            set
            {
                if (gameObject != null)
                {
                    gameObject.Name = value;
                    Messenger.Notify(EditorEvent.SceneObjectRenamed, gameObject.Id);
                }
            }
        }

        public GameObjectControl()
        {
            InitializeComponent();
        }

        public GameObjectControl(GameObject so)
            : this()
        {
            gameObject = so;
            DataContext = this;
        }

        public void Set(GameObject so)
        {
            gameObject = so;
            DataContext = this;
        }
    }
}
