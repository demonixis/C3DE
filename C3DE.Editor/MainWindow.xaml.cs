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
    using C3DE.Editor.Core;
    using C3DE.Editor.Core.Components;
    using C3DE.Editor.Controls;
    using C3DE.Components.Renderers;
    using C3DE.Components.Colliders;
    using C3DE.Components;

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;

            EDRegistry.Keys = new EDKeyboardComponent(null, this);
            EDRegistry.Mouse = new EDMouseComponent(null, this);

            Messenger.Register(EditorEvent.SceneObjectSelected, OnSceneObjectSelected);
            Messenger.Register(EditorEvent.SceneObjectUnSelected, OnSceneObjectUnselected);

            componentContainer.Children.Clear();
        }

        private void OnSceneObjectSelected(BasicMessage m)
        {
            var soMessage = m as GenericMessage<SceneObject>;
            var sceneObject = soMessage != null ? soMessage.Value : null;

            if (sceneObject != null)
            {
                var soEdition = new SceneObjectControl(sceneObject);
                var transform = new TransformControl();
                transform.Set(sceneObject.Transform);

                componentContainer.Children.Add(soEdition);
                componentContainer.Children.Add(transform);

                MeshRenderer meshRenderer = null;
                Camera camera = null;
                
                foreach (var component in sceneObject.Components)
                {
                    meshRenderer = component as MeshRenderer;
                    if (meshRenderer != null)
                    {
                        var meshRendererControl = new MeshRendererControl(meshRenderer);
                        componentContainer.Children.Add(meshRendererControl);
                        continue;
                    }

                    camera = component as Camera;
                    if (camera != null)
                    {
                        var cameraControl = new CameraControl(camera);
                        componentContainer.Children.Add(cameraControl);
                        continue;
                    }
                }
            }
        }

        private void OnSceneObjectUnselected(BasicMessage m)
        {
            componentContainer.Children.Clear();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("New");

            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Open");

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Save");

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers == ModifierKeys.Shift)
                NotifyCommand("SaveAs");

            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Copy");

            else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Cut");

            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Past");

            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("Duplicate");

            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
                NotifyCommand("All");

            else if (e.Key == Key.Escape)
                NotifyCommand("Escape");

            else
                Messenger.Notify(EditorEvent.KeyJustPressed, e.Key.ToString());
        }

        private void NotifyCommand(string commandName)
        {
            switch (commandName)
            {
                case "New": editorGameHost.NewScene(); break;

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

                case "All": Messenger.Notify(EditorEvent.CommandAll); break;
                case "Copy": Messenger.Notify(EditorEvent.CommandCopy); break;
                case "Cut": Messenger.Notify(EditorEvent.CommandCut); break;
                case "Past": Messenger.Notify(EditorEvent.CommandPast); break;
                case "Duplicate": Messenger.Notify(EditorEvent.CommandDuplicate); break;
                case "Delete": Messenger.Notify(EditorEvent.CommandDelete); break;
                case "Exit": WPFApplication.Current.Shutdown(); break;
                case "Escape": Messenger.Notify(EditorEvent.CommandEscape); break;
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
            if (item != null && item.Tag != null)
                Messenger.Notify(EditorEvent.CreateSceneObject, item.Tag.ToString());
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
