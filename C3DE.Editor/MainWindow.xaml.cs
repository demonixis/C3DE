using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    using C3DE.Editor.MonoGameBridge;
    using System;
    using System.Windows.Input;
    using WpfApplication = System.Windows.Application;

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private char[] _separator;
        private Point _lastMouseDown;
        private TreeViewItem _draggedItem;
        private TreeViewItem _target;

        public MainWindow()
        {
            InitializeComponent();
            _separator = new char[1] { '_' };

            editorGameHost.SceneObjectAdded += editorGameHost_SceneObjectAdded;
            editorGameHost.SceneObjectRemoved += editorGameHost_SceneObjectRemoved;
        }

        private void editorGameHost_SceneObjectAdded(object sender, SceneChangedEventArgs e)
        {
            if (e.Added)
                sceneTreeView.Items.Add(e.Name);
        }

        private void editorGameHost_SceneObjectRemoved(object sender, SceneChangedEventArgs e)
        {
            if (e.Added)
                return;

            var index = sceneTreeView.Items.IndexOf(e.Name);
            if (index > -1)
                sceneTreeView.Items.RemoveAt(index);
        }

        private void OnMenuFileClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            if (item != null)
            {
                var tmp = item.Name.Split(_separator);

                if (tmp[2] == "Exit")
                    WpfApplication.Current.Shutdown();
            }
        }

        private void OnMenuAddSceneObjectClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            if (item != null)
            {
                var tmp = item.Name.Split(_separator);
                editorGameHost.Add(tmp[2]);
            }
        }
    }
}
