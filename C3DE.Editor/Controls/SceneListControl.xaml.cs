using C3DE.Editor.Core;
using C3DE.Editor.Events;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SceneListControl.xaml
    /// </summary>
    public partial class SceneListControl : UserControl
    {
        private EDScene scene;

        public SceneListControl()
        {
            InitializeComponent();

            Messenger.Register(EditorEvent.SceneObjectAdded, UpdateList);
            Messenger.Register(EditorEvent.SceneObjectRemoved, UpdateList);
            Messenger.Register(EditorEvent.SceneObjectRenamed, UpdateList);
            Messenger.Register(EditorEvent.SceneObjectSelected, OnSelected);
            Messenger.Register(EditorEvent.SceneObjectUnSelected, OnUnselected);
            Loaded += OnLoaded;
            Visibility = System.Windows.Visibility.Hidden;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sceneTreeView.SelectedItemChanged += OnSelectedItemChanged;
            UpdateList();
            scene = Scene.current as EDScene;
        }

        private void OnSelected(BasicMessage m)
        {
            if (sceneTreeView.Items.Count == 0)
                Visibility = System.Windows.Visibility.Visible;
        }

        private void OnUnselected(BasicMessage m)
        {
            if (sceneTreeView.Items.Count == 0)
                Visibility = System.Windows.Visibility.Hidden;
        }

        private void OnSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            var index = GetSelectedIndex();
            if (index > -1)
            {
                var stackPanel = sceneTreeView.Items[index] as StackPanel;
                var textBlock = stackPanel.Children[0] as TextBlock;
                if (textBlock != null)
                    scene.SetSeletected(stackPanel.Tag.ToString());
            }
        }

        private int GetSelectedIndex()
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

            return index;
        }

        private object GetObjectByTag(string tag)
        {
            StackPanel control = null;

            for (int i = 0, l = sceneTreeView.Items.Count; i < l; i++)
            {
                control = sceneTreeView.Items[i] as StackPanel;
                if (control != null && control.Tag.ToString() == tag)
                    return sceneTreeView.Items[i];
            }

            return null;
        }

        private StackPanel CreateItem(string title, string tag)
        {
            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            sp.Tag = tag;

            var text = new TextBlock();
            text.Text = title;
            text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            text.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            sp.Children.Add(text);

            return sp;
        }

        private void UpdateList(BasicMessage m = null)
        {
            if (Scene.current != null)
            {
                sceneTreeView.Items.Clear();

                var sceneObjects = ((EDScene)(Scene.current)).GetSceneObjects();
                StackPanel control = null;

                for (int i = 0, l = sceneObjects.Length; i < l; i++)
                {
                    if (sceneObjects[i].Tag != EDScene.EditorTag)
                    {
                        control = CreateItem(sceneObjects[i].Name, sceneObjects[i].Id);
                        sceneTreeView.Items.Add(control);
                    }
                }
            }
        }
    }
}
