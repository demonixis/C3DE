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
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandNew);

            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandOpen);

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandSave);

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers == ModifierKeys.Shift)
                Messenger.Notify(EditorEvent.CommandSaveAll);

            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandCopy);

            else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandCut);

            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandPast);

            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandDuplicate);

            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                Messenger.Notify(EditorEvent.CommandAll);

            else if (e.Key == Key.Escape)
                Messenger.Notify(EditorEvent.CommandEscape);

            else
                Messenger.Notify(EditorEvent.KeyJustPressed, e.Key.ToString());
        }

        private void NotifyCommand(string commandName)
        {
            switch (commandName)
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

                case "Open":
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

                case "Copy": editorGameHost.CopySelection(); break;
                //case "Cut": editorGameHost.CutSelection(); break;
                case "Past": editorGameHost.PastSelection(); break;
                case "Duplicate": editorGameHost.DuplicateSelection(); break;
                case "Delete": editorGameHost.DeleteSelection(); break;
                case "Exit": WPFApplication.Current.Shutdown(); break;
            }
        }

        private void OnCommonMenuClick(object sender, RoutedEventArgs e)
        {
            var item = sender as Control;
            if (item == null)
                return;

            NotifyCommand(item.Tag.ToString());
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

        private void OnDrop(object sender, DragEventArgs e)
        {
            Debug.Log("drop");
        }
    }
}
