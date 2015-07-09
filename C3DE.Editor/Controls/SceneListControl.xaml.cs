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
            var index = -1;
            var i = 0;

            foreach (var item in sceneTreeView.Items)
            {
                if (item == sceneTreeView.SelectedItem)
                {
                    index = i;
                    break;
                }
                i++;
            }

            if (index > -1)
            {
                var sceneObject = Scene.FindById(_mapping[index]);
                Messenger.Notify(EditorEvent.SceneObjectUnSelected);
                Messenger.Notify(EditorEvent.SceneObjectSelected, new GenericMessage<SceneObject>(sceneObject));
            }
        }

        private void UpdateList()
        {
            if (Scene.current != null)
            {
                _mapping.Clear();
                sceneTreeView.Items.Clear();

                var objects = ((EDScene)(Scene.current)).SceneObjects;

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
