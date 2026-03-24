using System;
using System.IO;
using System.Numerics;
using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Editor.Assets;
using C3DE.Editor.Core;
using C3DE.Editor.ProjectSystem;
using C3DE.Graphics.PostProcessing;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGuiNet;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector3 = System.Numerics.Vector3;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace C3DE.Editor.UI
{
    public sealed class UIManager : DrawableGameComponent
    {
        private sealed class TextDialogState
        {
            public string Title;
            public string Prompt;
            public string Value;
            public Action<string> Callback;
            public bool OpenRequested;
        }

        private readonly EditorContext _context;
        private ImGuiRenderer _imGuiRenderer;
        private GraphicsDeviceManager _graphicsDeviceManager;
        private bool _changeGraphicsSettings;
        private string _statusMessage = "C3DE Editor Ready";
        private string _selectedComponentToAdd = "Camera";
        private string _selectedAssetGuid;
        private TextDialogState _dialogState;
        private string _messageTitle;
        private string _messageText;
        private bool _messageOpenRequested;
        private Point _lastDockLayoutSize;
        private bool _applyDefaultLayout;
        private IntPtr _previewTextureId;
        private Texture2D _boundPreviewTexture;
        private NumericsVector2 _workspacePosition;
        private NumericsVector2 _workspaceSize;
        private bool _sceneViewHovered;
        private bool _sceneViewFocused;
        private string _startupProjectDraft = string.Empty;
        private Rectangle _sceneViewBounds;

        public bool WantsMouseCapture { get; private set; }
        public bool IsSceneViewHovered => _sceneViewHovered;
        public bool IsSceneViewFocused => _sceneViewFocused;
        public Rectangle SceneViewBounds => _sceneViewBounds;
        public bool IsTextInputActive => ImGui.GetIO().WantTextInput;

        public event Action<string> MenuCommandSelected = null;
        public event Action<string> MenuGameObjectSelected = null;
        public event Action<string> MenuComponentSelected = null;
        public event Action<string, bool> TreeViewGameObjectSelected = null;
        public event Action<string> ProjectSceneOpenRequested = null;
        public event Action<AssetMeta> AssetSelected = null;
        public event Action<string> StartupProjectChanged = null;
        public event Action<string> ToolbarActionSelected = null;

        public UIManager(Game game, EditorContext context)
            : base(game)
        {
            _context = context;
        }

        public void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphicsDeviceManager = graphicsDeviceManager;
            _imGuiRenderer = new ImGuiRenderer(Game);
            _imGuiRenderer.RebuildFontAtlas();

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            Game.Window.AllowUserResizing = true;
            Game.IsMouseVisible = true;
            Game.Window.ClientSizeChanged += OnClientSizeChanged;
        }

        public void OpenSave(Action<string> callback)
        {
            OpenTextDialog("Save Scene", "Scene path", _context.CurrentScenePath ?? Path.Combine(GetDocumentsRoot(), "Main.scene.json"), callback);
        }

        public void OpenFolder(Action<string> callback, string title)
        {
            OpenTextDialog(title, "Folder path", GetDocumentsRoot(), callback);
        }

        public void OpenLoadDialog(Action<string> callback)
        {
            OpenTextDialog("Load Scene", "Scene path", _context.CurrentScenePath ?? Path.Combine(GetDocumentsRoot(), "Main.scene.json"), callback);
        }

        public void OpenMessageBox(string title, string text, int width = 320, int height = 200)
        {
            _messageTitle = title;
            _messageText = text;
            _messageOpenRequested = true;
        }

        public void SetStatusMessage(string message)
        {
            _statusMessage = message ?? string.Empty;
        }

        public void AddGameObject(GameObject go)
        {
        }

        public void RemoveGameObject(GameObject go)
        {
        }

        public void SelectGameObject(GameObject go, bool selected)
        {
            if (selected)
                _context.SetSelection(go);
        }

        public void ClearGameObjects()
        {
            _context.SetSelection(null);
        }

        public void SetScene(EditorScene scene)
        {
        }

        public void SetProject(string projectPath, AssetDatabase database)
        {
            _startupProjectDraft = _context.CurrentProject?.StartupProject ?? string.Empty;
        }

        public override void Update(GameTime gameTime)
        {
            if (_changeGraphicsSettings)
            {
                _graphicsDeviceManager.ApplyChanges();
                _changeGraphicsSettings = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _imGuiRenderer.BeforeLayout(gameTime);
            _sceneViewHovered = false;
            _sceneViewFocused = false;
            _sceneViewBounds = Rectangle.Empty;

            RenderDockSpace(gameTime);
            RenderToolbar();
            RenderHierarchyWindow();
            RenderSceneViewWindow();
            RenderInspectorWindow();
            RenderProjectWindow();
            RenderAssetWindow();
            RenderDialog();
            RenderMessagePopup();

            WantsMouseCapture = ImGui.GetIO().WantCaptureMouse && !_sceneViewHovered;

            _imGuiRenderer.AfterLayout();
            base.Draw(gameTime);
        }

        private void RenderDockSpace(GameTime gameTime)
        {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);

            const ImGuiWindowFlags hostWindowFlags =
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.MenuBar;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, NumericsVector2.Zero);

            var open = true;
            ImGui.Begin("EditorDockHost", ref open, hostWindowFlags);
            ImGui.PopStyleVar(3);

            var dockSpaceId = ImGui.GetID("EditorDockSpace");
            ImGui.DockSpace(dockSpaceId, NumericsVector2.Zero, ImGuiDockNodeFlags.None);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    EmitCommand("New Project");
                    EmitCommand("Open Project");
                    EmitCommand("New Scene");
                    EmitCommand("Save Scene");
                    EmitCommand("Save Scene As");
                    EmitCommand("Load Scene");
                    EmitCommand("Exit");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    EmitCommand("Copy");
                    EmitCommand("Cut");
                    EmitCommand("Past");
                    EmitCommand("Duplicate");
                    EmitCommand("Delete");
                    EmitCommand("Select All");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("GameObject"))
                {
                    EmitGameObject("Empty");
                    EmitGameObject("Cube");
                    EmitGameObject("Cylinder");
                    EmitGameObject("Plane");
                    EmitGameObject("Pyramid");
                    EmitGameObject("Quad");
                    EmitGameObject("Sphere");
                    EmitGameObject("Torus");
                    EmitGameObject("Camera");
                    EmitGameObject("Terrain");
                    EmitGameObject("Lava");
                    EmitGameObject("Water");
                    EmitGameObject("Directional");
                    EmitGameObject("Point");
                    EmitGameObject("Spot");
                    EmitGameObject("Reflection Probe");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help"))
                {
                    EmitCommand("About");
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            var currentSize = new Point((int)viewport.WorkSize.X, (int)viewport.WorkSize.Y);
            _applyDefaultLayout = currentSize != _lastDockLayoutSize;
            _lastDockLayoutSize = currentSize;

            var windowPosition = ImGui.GetWindowPos();
            var contentMin = ImGui.GetWindowContentRegionMin();
            var contentMax = ImGui.GetWindowContentRegionMax();
            _workspacePosition = windowPosition + contentMin;
            _workspaceSize = contentMax - contentMin;

            ImGui.End();
        }

        private void RenderToolbar()
        {
            var position = new NumericsVector2(_workspacePosition.X, _workspacePosition.Y);
            var size = new NumericsVector2(_workspaceSize.X, 36.0f);

            ImGui.SetNextWindowPos(position, ImGuiCond.Always);
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoTitleBar;

            if (!ImGui.Begin("Toolbar", flags))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Button("Play"))
                MenuCommandSelected?.Invoke("Play");

            ImGui.SameLine();
            if (ImGui.Button("Save"))
                ToolbarActionSelected?.Invoke("Save");

            ImGui.SameLine();
            if (ImGui.Button("Translate"))
                ToolbarActionSelected?.Invoke("Translate");

            ImGui.SameLine();
            if (ImGui.Button("Rotate"))
                ToolbarActionSelected?.Invoke("Rotate");

            ImGui.SameLine();
            if (ImGui.Button("Scale"))
                ToolbarActionSelected?.Invoke("Scale");

            ImGui.SameLine();
            if (ImGui.Button("Local/World"))
                ToolbarActionSelected?.Invoke("ToggleSpace");

            ImGui.SameLine();
            if (ImGui.Button("Snap"))
                ToolbarActionSelected?.Invoke("ToggleSnap");

            ImGui.SameLine();
            ImGui.TextDisabled(_statusMessage);

            ImGui.End();
        }

        private void RenderHierarchyWindow()
        {
            ApplyDefaultWindowLayout("Hierarchy", GetHierarchyPosition(), GetHierarchySize());
            if (!ImGui.Begin("Hierarchy"))
            {
                ImGui.End();
                return;
            }

            var scene = _context.CurrentScene;
            if (scene != null)
            {
                foreach (var gameObject in scene.GetRootGameObjects())
                    DrawHierarchyNode(gameObject);
            }

            ImGui.End();
        }

        private void RenderSceneViewWindow()
        {
            ApplyDefaultWindowLayout("Scene View", GetViewportPosition(), GetViewportSize());
            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Scene View", flags))
            {
                ImGui.End();
                return;
            }

            DrawPreviewTexture("Editor viewport preview");

            ImGui.End();
        }

        private void DrawHierarchyNode(GameObject gameObject)
        {
            var selected = _context.SelectedGameObject == gameObject;
            var children = gameObject.Transform.Transforms;
            var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
            if (selected)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            var open = ImGui.TreeNodeEx($"{gameObject.Name}##{gameObject.Id}", flags);

            if (ImGui.IsItemClicked())
                TreeViewGameObjectSelected?.Invoke(gameObject.Id, true);

            if (open)
            {
                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i]?.GameObject;
                    if (child != null && child.Tag != EditorGame.EditorTag)
                        DrawHierarchyNode(child);
                }

                ImGui.TreePop();
            }
        }

        private void RenderInspectorWindow()
        {
            ApplyDefaultWindowLayout("Inspector", GetInspectorPosition(), GetInspectorSize());
            if (!ImGui.Begin("Inspector"))
            {
                ImGui.End();
                return;
            }

            if (ImGui.BeginTabBar("InspectorTabs"))
            {
                if (ImGui.BeginTabItem("Inspector"))
                {
                    var selected = _context.SelectedGameObject;
                    if (selected == null)
                    {
                        ImGui.TextDisabled("No selection");
                    }
                    else
                    {
                        ImGui.SetNextItemWidth(220.0f);
                        if (ImGui.BeginCombo("##add_component_combo", _selectedComponentToAdd))
                        {
                            foreach (var entry in new[] { "Camera", "Directional", "Point", "Spot", "BoxCollider", "SphereCollider", "Rigidbody", "Terrain" })
                            {
                                var isSelected = _selectedComponentToAdd == entry;
                                if (ImGui.Selectable(entry, isSelected))
                                    _selectedComponentToAdd = entry;

                                if (isSelected)
                                    ImGui.SetItemDefaultFocus();
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.SameLine();
                        if (ImGui.Button("Add Component"))
                            MenuComponentSelected?.Invoke(_selectedComponentToAdd);

                        ImGui.Separator();

                        DrawGameObjectInspector(selected);
                        DrawTransformInspector(selected.Transform);
                        DrawCameraInspector(selected.GetComponent<Camera>());
                        DrawLightInspector(selected.GetComponent<Light>());
                        DrawMeshRendererInspector(selected.GetComponent<MeshRenderer>());
                        DrawTerrainInspector(selected.GetComponent<Terrain>());
                        DrawBoxColliderInspector(selected.GetComponent<BoxCollider>());
                        DrawSphereColliderInspector(selected.GetComponent<SphereCollider>());
                        DrawRigidbodyInspector(selected.GetComponent<Rigidbody>());
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Scene Settings"))
                {
                    RenderSceneSettingsContent();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        private void DrawGameObjectInspector(GameObject gameObject)
        {
            if (!ImGui.CollapsingHeader("GameObject", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            var name = gameObject.Name ?? string.Empty;
            if (ImGui.InputText("Name", ref name, 256))
            {
                gameObject.Name = name;
                _context.MarkSceneDirty();
            }

            var tag = gameObject.Tag ?? string.Empty;
            if (ImGui.InputText("Tag", ref tag, 256))
            {
                gameObject.Tag = tag;
                _context.MarkSceneDirty();
            }

            var enabled = gameObject.Enabled;
            if (ImGui.Checkbox("Enabled", ref enabled))
            {
                gameObject.Enabled = enabled;
                _context.MarkSceneDirty();
            }

            var isStatic = gameObject.IsStatic;
            if (ImGui.Checkbox("Is Static", ref isStatic))
            {
                gameObject.IsStatic = isStatic;
                _context.MarkSceneDirty();
            }
        }

        private void DrawTransformInspector(Transform transform)
        {
            if (!ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            DrawVector3("Position", transform.LocalPosition, value =>
            {
                transform.LocalPosition = value;
                _context.MarkSceneDirty();
            });
            DrawVector3("Rotation", transform.LocalRotation, value =>
            {
                transform.LocalRotation = value;
                _context.MarkSceneDirty();
            });
            DrawVector3("Scale", transform.LocalScale, value =>
            {
                transform.LocalScale = value;
                _context.MarkSceneDirty();
            });
        }

        private void DrawCameraInspector(Camera camera)
        {
            if (camera == null || !ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            var fov = camera.FieldOfView;
            if (ImGui.InputFloat("FOV", ref fov))
            {
                camera.FieldOfView = fov;
                _context.MarkSceneDirty();
            }

            var nearPlane = camera.Near;
            if (ImGui.InputFloat("Near", ref nearPlane))
            {
                camera.Near = nearPlane;
                _context.MarkSceneDirty();
            }

            var farPlane = camera.Far;
            if (ImGui.InputFloat("Far", ref farPlane))
            {
                camera.Far = farPlane;
                _context.MarkSceneDirty();
            }

            DrawEnum("Projection", camera.ProjectionType.ToString(), Enum.GetNames(typeof(CameraProjectionType)), value =>
            {
                camera.ProjectionType = (CameraProjectionType)Enum.Parse(typeof(CameraProjectionType), value);
                _context.MarkSceneDirty();
            });
        }

        private void DrawLightInspector(Light light)
        {
            if (light == null || !ImGui.CollapsingHeader("Light", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            DrawEnum("Type", light.Type.ToString(), new[] { "Directional", "Point", "Spot" }, value =>
            {
                light.Type = (LightType)Enum.Parse(typeof(LightType), value);
                _context.MarkSceneDirty();
            });

            DrawColor3("Color", light.Color.ToVector3(), value =>
            {
                light.Color = new XnaColor(value);
                _context.MarkSceneDirty();
            });

            var intensity = light.Intensity;
            if (ImGui.InputFloat("Intensity", ref intensity))
            {
                light.Intensity = intensity;
                _context.MarkSceneDirty();
            }

            var radius = light.Radius;
            if (ImGui.InputFloat("Radius", ref radius))
            {
                light.Radius = radius;
                _context.MarkSceneDirty();
            }

            var angle = light.Angle;
            if (ImGui.InputFloat("Angle", ref angle))
            {
                light.Angle = angle;
                _context.MarkSceneDirty();
            }

            var isSun = light.IsSun;
            if (ImGui.Checkbox("Is Sun", ref isSun))
            {
                light.IsSun = isSun;
                _context.MarkSceneDirty();
            }
        }

        private void DrawMeshRendererInspector(MeshRenderer renderer)
        {
            if (renderer == null || !ImGui.CollapsingHeader("MeshRenderer", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            var castShadow = renderer.CastShadow;
            if (ImGui.Checkbox("Cast Shadow", ref castShadow))
            {
                renderer.CastShadow = castShadow;
                _context.MarkSceneDirty();
            }

            var receiveShadow = renderer.ReceiveShadow;
            if (ImGui.Checkbox("Receive Shadow", ref receiveShadow))
            {
                renderer.ReceiveShadow = receiveShadow;
                _context.MarkSceneDirty();
            }

            ImGui.TextUnformatted($"Mesh: {renderer.Mesh?.GetType().Name ?? "None"}");

            if (renderer.Material != null)
            {
                DrawColor3("Diffuse", renderer.Material.DiffuseColor.ToVector3(), value =>
                {
                    renderer.Material.DiffuseColor = new XnaColor(value);
                    _context.MarkSceneDirty();
                });
            }
        }

        private void DrawTerrainInspector(Terrain terrain)
        {
            if (terrain == null || !ImGui.CollapsingHeader("Terrain", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            DrawVector3("Mesh Size", terrain.Geometry.Size, value =>
            {
                terrain.Geometry.Size = value;
                terrain.Build();
                _context.MarkSceneDirty();
            });

            if (ImGui.Button("Flatten"))
            {
                terrain.Flatten();
                _context.MarkSceneDirty();
            }

            ImGui.SameLine();
            if (ImGui.Button("Randomize"))
            {
                terrain.Randomize();
                _context.MarkSceneDirty();
            }

            var sand = terrain.WeightData.SandLayer;
            if (ImGui.InputFloat("Sand", ref sand))
            {
                terrain.SetWeightData(sand, terrain.WeightData.GroundLayer, terrain.WeightData.RockLayer, terrain.WeightData.SnowLayer);
                _context.MarkSceneDirty();
            }

            var ground = terrain.WeightData.GroundLayer;
            if (ImGui.InputFloat("Ground", ref ground))
            {
                terrain.SetWeightData(terrain.WeightData.SandLayer, ground, terrain.WeightData.RockLayer, terrain.WeightData.SnowLayer);
                _context.MarkSceneDirty();
            }

            var rock = terrain.WeightData.RockLayer;
            if (ImGui.InputFloat("Rock", ref rock))
            {
                terrain.SetWeightData(terrain.WeightData.SandLayer, terrain.WeightData.GroundLayer, rock, terrain.WeightData.SnowLayer);
                _context.MarkSceneDirty();
            }

            var snow = terrain.WeightData.SnowLayer;
            if (ImGui.InputFloat("Snow", ref snow))
            {
                terrain.SetWeightData(terrain.WeightData.SandLayer, terrain.WeightData.GroundLayer, terrain.WeightData.RockLayer, snow);
                _context.MarkSceneDirty();
            }
        }

        private void DrawBoxColliderInspector(BoxCollider collider)
        {
            if (collider == null || !ImGui.CollapsingHeader("BoxCollider", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            DrawVector3("Center", collider.Center, value =>
            {
                collider.SetCenter(value.X, value.Y, value.Z);
                _context.MarkSceneDirty();
            });

            DrawVector3("Size", collider.Size, value =>
            {
                collider.SetSize(value.X, value.Y, value.Z);
                _context.MarkSceneDirty();
            });

            var pickable = collider.IsPickable;
            if (ImGui.Checkbox("Pickable", ref pickable))
            {
                collider.IsPickable = pickable;
                _context.MarkSceneDirty();
            }

            var trigger = collider.IsTrigger;
            if (ImGui.Checkbox("Trigger", ref trigger))
            {
                collider.IsTrigger = trigger;
                _context.MarkSceneDirty();
            }
        }

        private void DrawSphereColliderInspector(SphereCollider collider)
        {
            if (collider == null || !ImGui.CollapsingHeader("SphereCollider", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            var radius = collider.Sphere.Radius;
            if (ImGui.InputFloat("Radius", ref radius))
            {
                collider.Sphere = new BoundingSphere(collider.Sphere.Center, radius);
                _context.MarkSceneDirty();
            }

            var pickable = collider.IsPickable;
            if (ImGui.Checkbox("Pickable", ref pickable))
            {
                collider.IsPickable = pickable;
                _context.MarkSceneDirty();
            }

            var trigger = collider.IsTrigger;
            if (ImGui.Checkbox("Trigger", ref trigger))
            {
                collider.IsTrigger = trigger;
                _context.MarkSceneDirty();
            }
        }

        private void DrawRigidbodyInspector(Rigidbody rigidbody)
        {
            if (rigidbody == null || !ImGui.CollapsingHeader("Rigidbody", ImGuiTreeNodeFlags.DefaultOpen))
                return;

            var isStatic = rigidbody.IsStatic;
            if (ImGui.Checkbox("Is Static", ref isStatic))
            {
                rigidbody.IsStatic = isStatic;
                _context.MarkSceneDirty();
            }

            var gravity = rigidbody.Gravity;
            if (ImGui.Checkbox("Gravity", ref gravity))
            {
                rigidbody.Gravity = gravity;
                _context.MarkSceneDirty();
            }

            var isKinematic = rigidbody.IsKinematic;
            if (ImGui.Checkbox("Is Kinematic", ref isKinematic))
            {
                rigidbody.IsKinematic = isKinematic;
                _context.MarkSceneDirty();
            }

            DrawVector3("Velocity", rigidbody.Velocity, value =>
            {
                rigidbody.Velocity = value;
                _context.MarkSceneDirty();
            });

            DrawVector3("Angular Velocity", rigidbody.AngularVelocity, value =>
            {
                rigidbody.AngularVelocity = value;
                _context.MarkSceneDirty();
            });
        }

        private void RenderSceneSettingsContent()
        {
            var scene = _context.CurrentScene;
            if (scene == null)
            {
                ImGui.TextDisabled("No scene loaded");
                return;
            }

            var settings = scene.RenderSettings;
            if (ImGui.CollapsingHeader("Render Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var fogEnabled = settings.FogEnabled;
                if (ImGui.Checkbox("Fog Enabled", ref fogEnabled))
                {
                    settings.FogEnabled = fogEnabled;
                    _context.MarkSceneDirty();
                }

                DrawEnum("Fog Mode", settings.FogMode.ToString(), Enum.GetNames(typeof(FogMode)), value =>
                {
                    settings.FogMode = (FogMode)Enum.Parse(typeof(FogMode), value);
                    _context.MarkSceneDirty();
                });

                var fogDensity = settings.FogDensity;
                if (ImGui.InputFloat("Fog Density", ref fogDensity))
                {
                    settings.FogDensity = fogDensity;
                    _context.MarkSceneDirty();
                }

                var fogStart = settings.FogStart;
                if (ImGui.InputFloat("Fog Start", ref fogStart))
                {
                    settings.FogStart = fogStart;
                    _context.MarkSceneDirty();
                }

                var fogEnd = settings.FogEnd;
                if (ImGui.InputFloat("Fog End", ref fogEnd))
                {
                    settings.FogEnd = fogEnd;
                    _context.MarkSceneDirty();
                }

                DrawColor3("Ambient", settings.AmbientColor.ToVector3(), value =>
                {
                    settings.AmbientColor = new XnaColor(value);
                    _context.MarkSceneDirty();
                });

                DrawColor3("Fog Color", settings.FogColor.ToVector3(), value =>
                {
                    settings.FogColor = new XnaColor(value);
                    _context.MarkSceneDirty();
                });

                var skyboxEnabled = settings.Skybox.Enabled;
                if (ImGui.Checkbox("Skybox Enabled", ref skyboxEnabled))
                {
                    settings.Skybox.Enabled = skyboxEnabled;
                    _context.MarkSceneDirty();
                }
            }

            if (ImGui.CollapsingHeader("Post Processing", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var post = settings.PostProcessing;
                var enabled = post.Enabled;
                if (ImGui.Checkbox("Stack Enabled", ref enabled))
                {
                    post.Enabled = enabled;
                    _context.MarkSceneDirty();
                }

                DrawEnum("Debug View", post.DebugView.ToString(), Enum.GetNames(typeof(PostProcessDebugView)), value =>
                {
                    post.DebugView = (PostProcessDebugView)Enum.Parse(typeof(PostProcessDebugView), value);
                    _context.MarkSceneDirty();
                });

                var tonemapping = post.Tonemapping.Enabled;
                if (ImGui.Checkbox("Tonemapping", ref tonemapping))
                {
                    post.Tonemapping.Enabled = tonemapping;
                    _context.MarkSceneDirty();
                }

                var exposure = post.Tonemapping.Exposure;
                if (ImGui.InputFloat("Exposure", ref exposure))
                {
                    post.Tonemapping.Exposure = exposure;
                    _context.MarkSceneDirty();
                }

                var bloom = post.Bloom.Enabled;
                if (ImGui.Checkbox("Bloom", ref bloom))
                {
                    post.Bloom.Enabled = bloom;
                    _context.MarkSceneDirty();
                }

                var bloomIntensity = post.Bloom.Intensity;
                if (ImGui.InputFloat("Bloom Intensity", ref bloomIntensity))
                {
                    post.Bloom.Intensity = bloomIntensity;
                    _context.MarkSceneDirty();
                }

                var ao = post.AmbientOcclusion.Enabled;
                if (ImGui.Checkbox("Ambient Occlusion", ref ao))
                {
                    post.AmbientOcclusion.Enabled = ao;
                    _context.MarkSceneDirty();
                }

                var aoIntensity = post.AmbientOcclusion.Intensity;
                if (ImGui.InputFloat("AO Intensity", ref aoIntensity))
                {
                    post.AmbientOcclusion.Intensity = aoIntensity;
                    _context.MarkSceneDirty();
                }

                var sharpen = post.Sharpen.Enabled;
                if (ImGui.Checkbox("Sharpen", ref sharpen))
                {
                    post.Sharpen.Enabled = sharpen;
                    _context.MarkSceneDirty();
                }

                var sharpenIntensity = post.Sharpen.Intensity;
                if (ImGui.InputFloat("Sharpen Intensity", ref sharpenIntensity))
                {
                    post.Sharpen.Intensity = sharpenIntensity;
                    _context.MarkSceneDirty();
                }

                var fxaa = post.AntiAliasing.Enabled;
                if (ImGui.Checkbox("FXAA", ref fxaa))
                {
                    post.AntiAliasing.Enabled = fxaa;
                    _context.MarkSceneDirty();
                }

                var vignette = post.Vignette.Enabled;
                if (ImGui.Checkbox("Vignette", ref vignette))
                {
                    post.Vignette.Enabled = vignette;
                    _context.MarkSceneDirty();
                }
            }

        }

        private void RenderProjectWindow()
        {
            ApplyDefaultWindowLayout("Project", GetProjectPosition(), GetProjectSize());
            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Project", flags))
            {
                ImGui.End();
                return;
            }

            var projectPath = _context.CurrentProjectPath;
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                ImGui.TextDisabled("No project opened");
                ImGui.End();
                return;
            }

            ImGui.TextUnformatted("Startup Project");
            if (ImGui.InputText("##startup_project", ref _startupProjectDraft, 1024))
            {
            }

            ImGui.SameLine();
            if (ImGui.Button("Apply"))
                StartupProjectChanged?.Invoke(_startupProjectDraft);

            ImGui.SameLine();
            if (ImGui.Button("Use Scene Project"))
                StartupProjectChanged?.Invoke(string.Empty);

            ImGui.Separator();
            DrawProjectTree(projectPath, projectPath);
            ImGui.End();
        }

        private void DrawProjectTree(string rootPath, string currentPath)
        {
            foreach (var directory in Directory.GetDirectories(currentPath))
            {
                if (Path.GetFileName(directory).Equals("Library", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.TreeNode(Path.GetFileName(directory)))
                {
                    DrawProjectTree(rootPath, directory);
                    ImGui.TreePop();
                }
            }

            foreach (var file in Directory.GetFiles(currentPath))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                var selected = string.Equals(_context.CurrentScenePath, file, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(Path.GetFileName(file), selected))
                {
                    if (file.EndsWith(".scene.json", StringComparison.OrdinalIgnoreCase) && ImGui.IsMouseDoubleClicked(0))
                        ProjectSceneOpenRequested?.Invoke(file);
                }
            }
        }

        private void RenderAssetWindow()
        {
            ApplyDefaultWindowLayout("Assets", GetAssetsPosition(), GetAssetsSize());
            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse;

            if (!ImGui.Begin("Assets", flags))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Button("Refresh Assets"))
                _context.AssetDatabase?.Scan();

            ImGui.SameLine();
            if (ImGui.Button("Import File"))
                OpenTextDialog("Import Asset", "Source file path", GetDocumentsRoot(), ImportAssetFile);

            ImGui.Separator();

            var database = _context.AssetDatabase;
            if (database != null)
            {
                foreach (var asset in database.Assets)
                {
                    var selected = _selectedAssetGuid == asset.Guid;
                    if (ImGui.Selectable($"{asset.AssetType}: {asset.SourceRelativePath}", selected))
                    {
                        _selectedAssetGuid = asset.Guid;
                        AssetSelected?.Invoke(asset);
                    }
                }
            }

            ImGui.End();
        }

        private void DrawPreviewTexture(string emptyLabel)
        {
            var texture = ResolvePreviewTexture();
            if (texture == null)
            {
                ImGui.TextDisabled(emptyLabel);
                return;
            }

            var textureId = GetOrBindPreviewTexture(texture);
            var available = ImGui.GetContentRegionAvail();
            if (available.X <= 1.0f || available.Y <= 1.0f)
                return;

            var fittedSize = FitSize(texture.Width, texture.Height, available.X, available.Y);
            var cursor = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new NumericsVector2(
                cursor.X + MathF.Max(0.0f, (available.X - fittedSize.X) * 0.5f),
                cursor.Y + MathF.Max(0.0f, (available.Y - fittedSize.Y) * 0.5f)));

            ImGui.Image(textureId, fittedSize, new NumericsVector2(0, 0), new NumericsVector2(1, 1));
            _sceneViewHovered = ImGui.IsItemHovered();
            _sceneViewFocused = ImGui.IsItemFocused() || ImGui.IsWindowFocused();

            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            _sceneViewBounds = new Rectangle(
                (int)min.X,
                (int)min.Y,
                Math.Max(1, (int)(max.X - min.X)),
                Math.Max(1, (int)(max.Y - min.Y)));
        }

        private void RenderDialog()
        {
            if (_dialogState == null)
                return;

            if (_dialogState.OpenRequested)
            {
                ImGui.OpenPopup(_dialogState.Title);
                _dialogState.OpenRequested = false;
            }

            var open = true;
            if (ImGui.BeginPopupModal(_dialogState.Title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextWrapped(_dialogState.Prompt);
                ImGui.InputText("##dialog_path", ref _dialogState.Value, 1024);

                if (ImGui.Button("OK"))
                {
                    var callback = _dialogState.Callback;
                    var value = _dialogState.Value;
                    _dialogState = null;
                    ImGui.CloseCurrentPopup();
                    callback?.Invoke(value);
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    _dialogState = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            else if (!open)
            {
                _dialogState = null;
            }
        }

        private void RenderMessagePopup()
        {
            if (_messageOpenRequested)
            {
                ImGui.OpenPopup(_messageTitle ?? "Message");
                _messageOpenRequested = false;
            }

            var open = true;
            if (ImGui.BeginPopupModal(_messageTitle ?? "Message", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextWrapped(_messageText ?? string.Empty);
                if (ImGui.Button("OK"))
                {
                    ImGui.CloseCurrentPopup();
                    _messageTitle = null;
                    _messageText = null;
                }

                ImGui.EndPopup();
            }
        }

        private void ImportAssetFile(string sourcePath)
        {
            if (_context.AssetDatabase == null || string.IsNullOrWhiteSpace(sourcePath))
                return;

            _context.AssetDatabase.ImportFiles(new[] { sourcePath });
            _context.AssetDatabase.Scan();
            _statusMessage = $"Imported asset: {Path.GetFileName(sourcePath)}";
        }

        private void OpenTextDialog(string title, string prompt, string initialValue, Action<string> callback)
        {
            _dialogState = new TextDialogState
            {
                Title = title,
                Prompt = prompt,
                Value = initialValue ?? string.Empty,
                Callback = callback,
                OpenRequested = true
            };
        }

        private Texture2D ResolvePreviewTexture()
        {
            if (Game is not Engine engine)
                return null;

            return engine.Renderer?.EditorPreviewRenderTarget;
        }

        private IntPtr GetOrBindPreviewTexture(Texture2D texture)
        {
            if (_boundPreviewTexture == texture && _previewTextureId != IntPtr.Zero)
                return _previewTextureId;

            if (_previewTextureId != IntPtr.Zero)
                _imGuiRenderer.UnbindTexture(_previewTextureId);

            _previewTextureId = _imGuiRenderer.BindTexture(texture);
            _boundPreviewTexture = texture;
            return _previewTextureId;
        }

        private static NumericsVector2 FitSize(float sourceWidth, float sourceHeight, float maxWidth, float maxHeight)
        {
            if (sourceWidth <= 0.0f || sourceHeight <= 0.0f || maxWidth <= 0.0f || maxHeight <= 0.0f)
                return NumericsVector2.Zero;

            var scale = MathF.Min(maxWidth / sourceWidth, maxHeight / sourceHeight);
            return new NumericsVector2(sourceWidth * scale, sourceHeight * scale);
        }

        private void ApplyDefaultWindowLayout(string windowName, NumericsVector2 position, NumericsVector2 size)
        {
            if (!_applyDefaultLayout)
                return;

            ImGui.SetNextWindowPos(position, ImGuiCond.Always);
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);
        }

        private NumericsVector2 GetViewportPosition()
        {
            return new NumericsVector2(_workspacePosition.X, _workspacePosition.Y + 36.0f);
        }

        private NumericsVector2 GetViewportSize()
        {
            var rightWidth = GetRightPaneWidth();
            var bottomHeight = GetBottomPaneHeight();
            return new NumericsVector2(_workspaceSize.X - rightWidth, _workspaceSize.Y - bottomHeight - 36.0f);
        }

        private NumericsVector2 GetAssetsPosition()
        {
            return new NumericsVector2(_workspacePosition.X, _workspacePosition.Y + GetViewportSize().Y);
        }

        private NumericsVector2 GetAssetsSize()
        {
            return new NumericsVector2(GetViewportSize().X, GetBottomPaneHeight());
        }

        private NumericsVector2 GetHierarchyPosition()
        {
            return new NumericsVector2(_workspacePosition.X + GetViewportSize().X, _workspacePosition.Y);
        }

        private NumericsVector2 GetHierarchySize()
        {
            return new NumericsVector2(GetHierarchyPaneWidth(), _workspaceSize.Y - GetProjectPaneHeight());
        }

        private NumericsVector2 GetProjectPosition()
        {
            return new NumericsVector2(_workspacePosition.X + GetViewportSize().X, _workspacePosition.Y + GetHierarchySize().Y);
        }

        private NumericsVector2 GetProjectSize()
        {
            return new NumericsVector2(GetHierarchyPaneWidth(), GetProjectPaneHeight());
        }

        private NumericsVector2 GetInspectorPosition()
        {
            return new NumericsVector2(_workspacePosition.X + GetViewportSize().X + GetHierarchyPaneWidth(), _workspacePosition.Y);
        }

        private NumericsVector2 GetInspectorSize()
        {
            return new NumericsVector2(GetRightPaneWidth() - GetHierarchyPaneWidth(), _workspaceSize.Y);
        }

        private float GetRightPaneWidth()
        {
            return MathF.Max(360.0f, _workspaceSize.X * 0.34f);
        }

        private float GetBottomPaneHeight()
        {
            return MathF.Max(220.0f, _workspaceSize.Y * 0.36f);
        }

        private float GetHierarchyPaneWidth()
        {
            return MathF.Max(220.0f, GetRightPaneWidth() * 0.44f);
        }

        private float GetProjectPaneHeight()
        {
            return MathF.Max(200.0f, _workspaceSize.Y * 0.34f);
        }

        private void DrawVector3(string label, XnaVector3 value, Action<XnaVector3> onChanged)
        {
            var vector = new NumericsVector3(value.X, value.Y, value.Z);
            if (ImGui.InputFloat3(label, ref vector))
                onChanged(new XnaVector3(vector.X, vector.Y, vector.Z));
        }

        private void DrawColor3(string label, XnaVector3 value, Action<XnaVector3> onChanged)
        {
            var vector = new NumericsVector3(value.X, value.Y, value.Z);
            if (ImGui.ColorEdit3(label, ref vector))
                onChanged(new XnaVector3(vector.X, vector.Y, vector.Z));
        }

        private void DrawEnum(string label, string currentValue, string[] values, Action<string> onChanged)
        {
            if (!ImGui.BeginCombo(label, currentValue))
                return;

            for (var i = 0; i < values.Length; i++)
            {
                var isSelected = values[i] == currentValue;
                if (ImGui.Selectable(values[i], isSelected))
                    onChanged(values[i]);

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        private void EmitCommand(string label)
        {
            if (ImGui.MenuItem(label))
                MenuCommandSelected?.Invoke(label);
        }

        private void EmitGameObject(string label)
        {
            if (ImGui.MenuItem(label))
                MenuGameObjectSelected?.Invoke(label);
        }

        private static string GetDocumentsRoot()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            _graphicsDeviceManager.PreferredBackBufferWidth = Game.Window.ClientBounds.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = Game.Window.ClientBounds.Height;
            _changeGraphicsSettings = true;
        }
    }
}
