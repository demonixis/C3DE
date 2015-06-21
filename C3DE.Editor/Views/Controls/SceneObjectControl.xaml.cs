using System.Windows.Controls;

namespace C3DE.Editor.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour SceneObjectControl.xaml
    /// </summary>
    public partial class SceneObjectControl : UserControl
    {
        private SceneObjectControlChanged _eventCache;
        private bool _initialized = false;

        public bool SceneObjectEnabled
        {
            get { return SOEnabled.IsChecked.HasValue ? SOEnabled.IsChecked.Value : false; }
            set { SOEnabled.IsChecked = value; }
        }

        public string SceneObjectName
        {
            get { return SOName.Text; }
            set { SOName.Text = value; }
        }

        private void Notify()
        {
            _eventCache.Set(SceneObjectName, SceneObjectEnabled);
            Messenger.Notify("Editor.SceneObjectChanged", _eventCache);
        }

        public SceneObjectControl()
        {
            _eventCache = new SceneObjectControlChanged();
            InitializeComponent();
            Messenger.Register("Editor.SceneObjectUpdated", OnSceneObjectUpdated);
            _initialized = true;
        }

        public void Set(string name, bool isEnabled)
        {
            SceneObjectName = name;
            SceneObjectEnabled = isEnabled;
        }

        private void OnSceneObjectUpdated(BasicMessage m)
        {
            var data = m as SceneObjectControlChanged;
            if (data != null && _initialized)
            {
                SceneObjectName = data.Name;
                SceneObjectEnabled = data.Enable;
            }
        }
    }
}
