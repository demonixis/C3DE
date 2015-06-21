using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    using C3DE.Editor.MonoGameBridge;
    using System.Windows.Input;
    using WPFApplication = System.Windows.Application;

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private char[] _separator;

        public MainWindow()
        {
            InitializeComponent();
            _separator = new char[1] { '_' };

            editorGameHost.SceneObjectAdded += OnSceneObjectAdded;
            editorGameHost.SceneObjectRemoved += OnSceneObjectRemoved;

            KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify("Editor.Command.Copy");

            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify("Editor.Command.Past");

            else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify("Editor.Command.Cut");

            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify("Editor.Command.SelectAll");

            if (e.IsToggled)
                Messenger.Notify("Editor.JustPressed", e.Key.ToString());
        }

        private void OnSceneObjectAdded(object sender, SceneChangedEventArgs e)
        {
            if (e.Added)
                sceneListComponent.AddItem(e.Name);
        }

        private void OnSceneObjectRemoved(object sender, SceneChangedEventArgs e)
        {
            if (e.Added)
                return;

            sceneListComponent.RemoveItem(e.Name);
        }

        private void OnMenuFileClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            if (item != null)
            {
                var tmp = item.Name.Split(_separator);
                if (tmp[2] == "Exit")
                    WPFApplication.Current.Shutdown();
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
