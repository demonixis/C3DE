using C3DE.Editor.Events;
using System.Windows.Controls;

namespace C3DE.Editor.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour SceneObjectControl.xaml
    /// </summary>
    public partial class SceneObjectControl : UserControl
    {
        private static GenericMessage<bool> SceneObjectChangedMessage = new GenericMessage<bool>();
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
            SceneObjectChangedMessage.Value = SceneObjectEnabled;
            SceneObjectChangedMessage.Message = SceneObjectName;
            Messenger.Notify(EditorEvent.SceneObjectRenamed, SceneObjectChangedMessage);
        }

        public SceneObjectControl()
        {
            InitializeComponent();
            Messenger.Register(EditorEvent.SceneObjectSelected, OnSceneObjectSelected);
            _initialized = true;
        }

        public void Set(string name, bool isEnabled)
        {
            SceneObjectName = name;
            SceneObjectEnabled = isEnabled;
        }

        private void OnSceneObjectSelected(BasicMessage m)
        {
            SceneObjectChangedMessage = m as GenericMessage<bool>;
            if (SceneObjectChangedMessage != null && _initialized)
            {
                SceneObjectName = SceneObjectChangedMessage.Message;
                SceneObjectEnabled = SceneObjectChangedMessage.Value;
            }
        }
    }
}
