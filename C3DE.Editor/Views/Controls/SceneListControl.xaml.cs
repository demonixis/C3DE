using C3DE.Editor.Events;
using System.Collections.Generic;
using System.Windows.Controls;

namespace C3DE.Editor.Views.Controls
{
    /// <summary>
    /// Interaction logic for SceneListControl.xaml
    /// </summary>
    public partial class SceneListControl : UserControl
    {
        public static GenericMessage<SceneObject> SceneObjectMessage;
        private Dictionary<string, int> _itemMapping;

        public SceneListControl()
        {
            InitializeComponent();
            _itemMapping = new Dictionary<string, int>();
            Messenger.Register(EditorEvent.SceneObjectAdded, OnSceneObjectAdded);
            Messenger.Register(EditorEvent.SceneObjectRemoved, OnSceneObjectRemoved);
            Messenger.Register(EditorEvent.SceneObjectRenamed, OnSceneObjectChanged);
        }

        private void OnSceneObjectAdded(BasicMessage m)
        {
            SceneObjectMessage = m as GenericMessage<SceneObject>;

            if (SceneObjectMessage != null)
            {
                var index = sceneTreeView.Items.Add(SceneObjectMessage.Value.Name);
                if (index > -1)
                    _itemMapping.Add(SceneObjectMessage.Value.Id, index);
            }
        }

        private void OnSceneObjectRemoved(BasicMessage m)
        {
            if (_itemMapping.ContainsKey(m.Message))
            {
                var index = _itemMapping[m.Message];
                sceneTreeView.Items.RemoveAt(index);
                _itemMapping.Remove(m.Message);
            }
        }

        private void OnSceneObjectChanged(BasicMessage m)
        {
            SceneObjectMessage = m as GenericMessage<SceneObject>;

            if (SceneObjectMessage != null)
            {
                var id = SceneObjectMessage.Value.Id;

                if (_itemMapping.ContainsKey(id))
                {
                    var index = _itemMapping[id];
                    sceneTreeView.Items[index] = SceneObjectMessage.Value.Name;
                }
            }
        }
    }
}
