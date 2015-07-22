using System.Windows;
using System.Windows.Controls;

namespace C3DE.Editor
{
    using C3DE.Components;
    using C3DE.Components.Colliders;
    using C3DE.Components.Lights;
    using C3DE.Components.Renderers;
    using C3DE.Editor.Controls;
    using C3DE.Editor.Core;
    using C3DE.Editor.Core.Components;
    using C3DE.Editor.Events;
    using System;
    using System.IO;
    using System.Windows.Input;
    using Winforms = System.Windows.Forms;
    using WPFApplication = System.Windows.Application;

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
            editorGameHost.EngineReady += InitializeUI;

            EDRegistry.Keys = new EDKeyboardComponent(null, this);
            EDRegistry.Mouse = new EDMouseComponent(null, editorGameHost);

            Messenger.Register(EditorEvent.SceneObjectSelected, OnSceneObjectSelected);
            Messenger.Register(EditorEvent.SceneObjectUnSelected, OnSceneObjectUnselected);         
        }

        private void InitializeUI()
        {
            sceneListControl.UpdateList();
            projectExplorer.ShowFilesForDirectory("temp");
        }

        private void OnSceneObjectSelected(BasicMessage m)
        {
            var soMessage = m as GenericMessage<SceneObject>;
            var sceneObject = soMessage != null ? soMessage.Value : null;

            if (sceneObject != null)
            {
                var soEdition = new SceneObjectControl(sceneObject);
                var transform = new TransformControl(sceneObject.Transform);

                componentContainer.Children.Add(soEdition);
                componentContainer.Children.Add(transform);

                Control control = null;
                MeshRenderer meshRenderer = null;
                Camera camera = null;
                Light light = null;
                Collider collider = null;

                foreach (var component in sceneObject.Components)
                {
                    meshRenderer = component as MeshRenderer;
                    if (meshRenderer != null)
                    {
                        control = new MeshRendererControl(meshRenderer);
                        componentContainer.Children.Add(control);
                        continue;
                    }

                    camera = component as Camera;
                    if (camera != null)
                    {
                        control = new CameraControl(camera);
                        componentContainer.Children.Add(control);
                        continue;
                    }

                    light = component as Light;
                    if (light != null)
                    {
                        control = new LightControl(light);
                        componentContainer.Children.Add(control);
                        continue;
                    }

                    collider = component as Collider;
                    if (collider != null)
                    {
                        control = new ColliderControl(collider);
                        componentContainer.Children.Add(control);
                        continue;
                    }
                }

                MainTabControl.SelectedItem = MainTabControl.Items[0];
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
                            if (!editorGameHost.SaveScene(saveFileDialog.FileName))
                                MessageBox.Show("This scene can't be saved. Please contact the developer with the error file", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;

                case "Load":
                    {
                        var openFileDialog = new Winforms.OpenFileDialog();
                        openFileDialog.Filter = "C3DE Scene (*.scene)|*.scene";

                        if (openFileDialog.ShowDialog() == Winforms.DialogResult.OK)
                        {
                            if (!editorGameHost.LoadScene(openFileDialog.FileName))
                                MessageBox.Show("This scene can't be loaded. Please contact the developer with the error file", "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AskXnbWindow dialog = null;

                foreach (var file in files)
                {
                    var filename = Path.GetFileName(file);

                    dialog = new AskXnbWindow(EDRegistry.ContentTempPath, filename);
                    dialog.TypeSelected += (s, evt) =>
                    {
                        if (evt.Type == typeof(Microsoft.Xna.Framework.Graphics.Effect))
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Effects/", filename), true);
                        
                        else if (evt.Type == typeof(Microsoft.Xna.Framework.Graphics.SpriteFont))
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Fonts/", filename), true);

                        else if (evt.Type == typeof(Microsoft.Xna.Framework.Graphics.Model))
                        {
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Models/", filename), true);
                            editorGameHost.AddModelFromTemp("Models/" + filename.Replace(".xnb", ""));
                        }

                        else if (evt.Type == typeof(Microsoft.Xna.Framework.Audio.SoundEffect))
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Audio/Sounds/", filename), true);

                        else if (evt.Type == typeof(Microsoft.Xna.Framework.Media.Song))
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Audio/Music/", filename), true);

                        else if (evt.Type == typeof(Microsoft.Xna.Framework.Graphics.Texture2D))
                            File.Copy(file, Path.Combine(EDRegistry.ContentTempPath + "/Textures/", filename), true);
                    };
                    dialog.Show();
                }
            }
        }

        private void sceneListControl_MouseEnter(object sender, MouseEventArgs e)
        {
            editorGameHost.Focus();
        }

        private void ShowRenderSettings(object sender, RoutedEventArgs e)
        {
            var renderSettings = new RenderSettingsWindow(editorGameHost.Scene.RenderSettings);
            renderSettings.Show();
        }

        private void OnActionBarClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button != null ? button.Tag.ToString() : null;

            if (tag != null)
            {
                if (tag == "Translation")
                    editorGameHost.gizmoComponent.ActiveMode = XNAGizmo.GizmoMode.Translate;
                else if (tag == "Rotation")
                    editorGameHost.gizmoComponent.ActiveMode = XNAGizmo.GizmoMode.Rotate;
                else if (tag == "Scale")
                    editorGameHost.gizmoComponent.ActiveMode = XNAGizmo.GizmoMode.NonUniformScale;
                else if (tag == "UScale")
                    editorGameHost.gizmoComponent.ActiveMode = XNAGizmo.GizmoMode.UniformScale;
                else if (tag == "Precision")
                    editorGameHost.gizmoComponent.PrecisionModeEnabled = !editorGameHost.gizmoComponent.PrecisionModeEnabled;
            }
        }
    }
}
