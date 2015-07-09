using C3DE.Editor.Core;
using C3DE.Editor.Events;
using System.Collections.Generic;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SceneListControl.xaml
    /// </summary>
    public partial class SceneListControl : UserControl
    {
        Dictionary<int, string> _mapping;

        public SceneListControl()
        {
            InitializeComponent();
            _mapping = new Dictionary<int, string>();
            Messenger.Register(EditorEvent.SceneObjectAdded, OnSceneObjectAdded);
            Messenger.Register(EditorEvent.SceneObjectRemoved, OnSceneObjectRemoved);
            Messenger.Register(EditorEvent.SceneObjectRenamed, OnSceneObjectChanged);
            Loaded += SceneListControl_Loaded;
        }

        void SceneListControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sceneTreeView.SelectedItemChanged += sceneTreeView_SelectedItemChanged;
            UpdateList();
        }

        void sceneTreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.Log(sceneTreeView.SelectedItem, sceneTreeView.SelectedValue);
        }

        private void UpdateList()
        {
            if (Scene.current != null)
            {
                sceneTreeView.Items.Clear();

                var objects = ((EDScene)(Scene.current)).SceneObjects2;

                foreach (var obj in objects)
                {
                    var index = sceneTreeView.Items.Add(obj.Value);
                    _mapping.Add(index, obj.Key);
                }
            }
        }

        private void OnSceneObjectAdded(BasicMessage m)
        {
            UpdateList();
        }

        private void OnSceneObjectRemoved(BasicMessage m)
        {
            UpdateList();
        }

        private void OnSceneObjectChanged(BasicMessage m)
        {
            UpdateList();
        }
    }
}
