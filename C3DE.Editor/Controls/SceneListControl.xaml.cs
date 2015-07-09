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
        public SceneListControl()
        {
            InitializeComponent();
            Messenger.Register(EditorEvent.SceneObjectAdded, OnSceneObjectAdded);
            Messenger.Register(EditorEvent.SceneObjectRemoved, OnSceneObjectRemoved);
            Messenger.Register(EditorEvent.SceneObjectRenamed, OnSceneObjectChanged);
            Loaded += SceneListControl_Loaded;
        }

        void SceneListControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateList();
        }

        private void UpdateList()
        {
            if (Scene.current != null)
            {
                sceneTreeView.Items.Clear();

                var objects = ((EDScene)(Scene.current)).SceneObjects;

                for (int i = 0, l = objects.Length; i < l; i++)
                    sceneTreeView.Items.Add(objects[i]);
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
