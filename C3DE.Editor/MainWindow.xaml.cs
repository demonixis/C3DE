using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    using C3DE.Editor.MonoGameBridge;
    using System.IO;
    using System.Windows.Input;
    using WPFApplication = System.Windows.Application;
    using Winforms = System.Windows.Forms;
    using C3DE.Editor.Events;

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

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && e.IsToggled)
                Messenger.Notify(EditorEvent.CommandEscape);

            if (e.IsToggled)
                Messenger.Notify(EditorEvent.KeyJustPressed, e.Key.ToString());
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandCopy);

            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandPast);

            else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandCut);

            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandAll);

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandSave);

            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandDuplicate);

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers == ModifierKeys.Shift)
                Messenger.Notify(EditorEvent.CommandSaveAll);
        }
        // -- DEPRECATED
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
        // -- DEPRECATED

        private void OnFileMenuClick(object sender, RoutedEventArgs e)
        {
            var item = sender as Control;
            if (item == null)
                return;

            var tag = item.Tag.ToString();
            switch (tag)
            {
                case "New":
                    editorGameHost.NewScene();
                    break;

                case "Save":
                case "SaveAs":
                    {
                        var saveFileDialog = new Winforms.SaveFileDialog();
                        saveFileDialog.Filter = "C3DE Scene (*.scene)|*.scene";

                        if (saveFileDialog.ShowDialog() == Winforms.DialogResult.OK)
                        {
                            var save = editorGameHost.SaveScene();
                            File.WriteAllText(saveFileDialog.FileName, save);
                        }
                    }
                    break;

                case "Load":
                    {
                        var openFileDialog = new Winforms.OpenFileDialog();
                        openFileDialog.Filter = "C3DE Scene (*.scene)|*.scene";

                        if (openFileDialog.ShowDialog() == Winforms.DialogResult.OK)
                        {
                            var data = File.ReadAllText(openFileDialog.FileName);
                            editorGameHost.LoadScene(data);
                        }
                    }
                    break;

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
            {
                var format = item.Tag.ToString();

                var saveFileDialog = new Winforms.SaveFileDialog();
                saveFileDialog.Filter = format == "stl" ? "STL file (*.stl)|*.stl" : "OBJ/MTL file (*.obj)|*.obj";

                if (saveFileDialog.ShowDialog() == Winforms.DialogResult.OK)
                {
                    var result = editorGameHost.ExportSceneTo(format);
                    if (result != null)
                        File.WriteAllText(saveFileDialog.FileName, result[0]);
                }
            }
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("C3DE Editor is a scene editor for the C3DE Engine. It's still very experimental.", "About C3DE Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
