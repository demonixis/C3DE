using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    using C3DE.Editor.MonoGameBridge;
    using System.IO;
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

        private void OnFileMenuClick(object sender, RoutedEventArgs e)
        {
            var item = sender as Control;
            if (item == null)
                return;

            var tag = item.Tag.ToString();
            switch(tag)
            {
                case "New": break;
                case "Save": break;
                case "SaveAs":
                    var data = editorGameHost.SaveScene();
                    var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            File.WriteAllText(saveFileDialog.FileName, data);
                    break;
                case "Load": break;
                case "NewP": break;
                case "LoadP": break;
                case "SaveP": break;
                case "SaveAsP": break;
                case "Exit": WPFApplication.Current.Shutdown(); break;
            }
        }

        private void OnAddSceneObject(object sender, RoutedEventArgs e)
        {
            var item = sender as Control;
            if (item != null)
                editorGameHost.Add(item.Tag.ToString());
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            var item = sender as Control;
            if (item != null)
                editorGameHost.ExportSceneTo(item.Tag.ToString());
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("C3DE Editor is a scene editor for the C3DE Engine. It's still very experimental.", "About C3DE Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
