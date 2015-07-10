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

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sceneTreeView.SelectedItemChanged += OnSelectedItemChanged;
            UpdateList();
            scene = Scene.current as EDScene;
        }

        private void OnSelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            var index = GetSelectedIndex();
            if (index > -1)
            {
                var textBlock = sceneTreeView.Items[index] as TextBlock;
                if (textBlock != null)
                    scene.SetSeletected(textBlock.Tag.ToString());
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
            TextBlock textBlock = null;

            for (int i = 0, l = sceneTreeView.Items.Count; i < l; i++)
            {
                textBlock = sceneTreeView.Items[i] as TextBlock;
                if (textBlock != null && textBlock.Tag.ToString() == tag)
                    return sceneTreeView.Items[i];
            }

            return null;
        }

        private void UpdateList(BasicMessage m = null)
        {
            if (Scene.current != null)
            {
                sceneTreeView.Items.Clear();

                var sceneObjects = ((EDScene)(Scene.current)).GetSceneObjects();
                TextBlock textBlock = null;

                for (int i = 0, l = sceneObjects.Length; i < l; i++)
                {
                    if (sceneObjects[i].Tag != EDScene.EditorTag)
                    {
                        textBlock = new TextBlock();
                        textBlock.Text = sceneObjects[i].Name;
                        textBlock.Tag = sceneObjects[i].Id;
                        sceneTreeView.Items.Add(textBlock);
                    }
                }
            }
        }
    }
}
