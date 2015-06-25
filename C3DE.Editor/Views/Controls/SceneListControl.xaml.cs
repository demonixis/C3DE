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
        private Dictionary<string, int> _itemMapping;

        public SceneListControl()
        {
            InitializeComponent();
            _itemMapping = new Dictionary<string, int>();
            Messenger.Register(EditorEvent.SceneObjectChanged, OnSceneObjectChanged);
        }

        public void AddItem(string name)
        {
            var index = sceneTreeView.Items.Add(name);
            //if (index > -1)
                //_itemMapping.Add(name, index);
        }

        public void RemoveItem(string name)
        {
            var index = sceneTreeView.Items.IndexOf(name);
            if (index > -1)
            {
                sceneTreeView.Items.RemoveAt(index);
                //_itemMapping.Remove(name);
            }
        }

        public void RenameItem(string name, string newName)
        {
            var index = sceneTreeView.Items.IndexOf(name);
            if (index > -1)
            {
                sceneTreeView.Items[index] = newName;
                //_itemMapping.Remove(name);
                //_itemMapping.Add(newName, index);
            }
        }

        private void OnSceneObjectChanged(BasicMessage m)
        {
            var data = m as SceneObjectControlChanged;
        }
    }
}
