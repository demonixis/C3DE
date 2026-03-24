using System;
using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Editor.UI.Items;
using Gwen.Control;
using Microsoft.Xna.Framework;

namespace C3DE.Editor.UI.Panels
{
    public sealed class InspectorPanel : ControlBase
    {
        private readonly PropertyTree _tree;
        private readonly ComboBox _addComponentCombo;
        private readonly Button _addComponentButton;
        private GameObject _gameObject;

        public event Action<string> AddComponentRequested;

        public InspectorPanel(ControlBase parent) : base(parent)
        {
            var layout = new Gwen.Control.Layout.VerticalLayout(this)
            {
                Dock = Gwen.Dock.Fill
            };

            var actions = new Gwen.Control.Layout.FlowLayout(layout)
            {
                Dock = Gwen.Dock.Top,
                Height = 28
            };

            _addComponentCombo = new ComboBox(actions)
            {
                Width = 180
            };

            foreach (var value in new[] { "Camera", "Directional", "Point", "Spot", "BoxCollider", "SphereCollider", "Rigidbody", "Terrain" })
                _addComponentCombo.AddItem(value, value, value);

            _addComponentButton = new Button(actions)
            {
                Text = "Add Component",
                Width = 100
            };
            _addComponentButton.Clicked += (_, __) =>
            {
                var text = _addComponentCombo.SelectedItem?.Text;
                if (!string.IsNullOrWhiteSpace(text))
                    AddComponentRequested?.Invoke(text);
            };

            _tree = new PropertyTree(layout)
            {
                Dock = Gwen.Dock.Fill
            };
        }

        public void Bind(GameObject gameObject, Action onDirty)
        {
            _gameObject = gameObject;
            _tree.DeleteAllChildren();

            if (_gameObject == null)
                return;

            var header = _tree.Add("GameObject");
            AddString(header, "Name", _gameObject.Name, value => { _gameObject.Name = value; onDirty?.Invoke(); });
            AddString(header, "Tag", _gameObject.Tag, value => { _gameObject.Tag = value; onDirty?.Invoke(); });
            AddBool(header, "Enabled", _gameObject.Enabled, value => { _gameObject.Enabled = value; onDirty?.Invoke(); });
            AddBool(header, "IsStatic", _gameObject.IsStatic, value => { _gameObject.IsStatic = value; onDirty?.Invoke(); });

            var transform = _tree.Add("Transform");
            AddVector3(transform, "Position", _gameObject.Transform.LocalPosition, value => { _gameObject.Transform.LocalPosition = value; onDirty?.Invoke(); });
            AddVector3(transform, "Rotation", _gameObject.Transform.LocalRotation, value => { _gameObject.Transform.LocalRotation = value; onDirty?.Invoke(); });
            AddVector3(transform, "Scale", _gameObject.Transform.LocalScale, value => { _gameObject.Transform.LocalScale = value; onDirty?.Invoke(); });

            var camera = _gameObject.GetComponent<Camera>();
            if (camera != null)
            {
                var section = _tree.Add("Camera");
                AddFloat(section, "FOV", camera.FieldOfView, value => { camera.FieldOfView = value; onDirty?.Invoke(); });
                AddFloat(section, "Near", camera.Near, value => { camera.Near = value; onDirty?.Invoke(); });
                AddFloat(section, "Far", camera.Far, value => { camera.Far = value; onDirty?.Invoke(); });
                AddEnum(section, "Projection", camera.ProjectionType.ToString(), Enum.GetNames(typeof(CameraProjectionType)), value =>
                {
                    camera.ProjectionType = (CameraProjectionType)Enum.Parse(typeof(CameraProjectionType), value);
                    onDirty?.Invoke();
                });
            }

            var light = _gameObject.GetComponent<Light>();
            if (light != null)
            {
                var section = _tree.Add("Light");
                AddEnum(section, "Type", light.Type.ToString(), new[] { "Directional", "Point", "Spot" }, value =>
                {
                    light.Type = (LightType)Enum.Parse(typeof(LightType), value);
                    onDirty?.Invoke();
                });
                AddVector3(section, "Color", light.Color.ToVector3(), value => { light.Color = new Microsoft.Xna.Framework.Color(value); onDirty?.Invoke(); });
                AddFloat(section, "Intensity", light.Intensity, value => { light.Intensity = value; onDirty?.Invoke(); });
                AddFloat(section, "Radius", light.Radius, value => { light.Radius = value; onDirty?.Invoke(); });
                AddFloat(section, "Angle", light.Angle, value => { light.Angle = value; onDirty?.Invoke(); });
                AddBool(section, "IsSun", light.IsSun, value => { light.IsSun = value; onDirty?.Invoke(); });
            }

            var meshRenderer = _gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var section = _tree.Add("MeshRenderer");
                AddBool(section, "Cast Shadow", meshRenderer.CastShadow, value => { meshRenderer.CastShadow = value; onDirty?.Invoke(); });
                AddBool(section, "Receive Shadow", meshRenderer.ReceiveShadow, value => { meshRenderer.ReceiveShadow = value; onDirty?.Invoke(); });
                AddString(section, "Mesh", meshRenderer.Mesh?.GetType().Name ?? string.Empty, _ => { });
                if (meshRenderer.Material != null)
                    AddVector3(section, "Diffuse", meshRenderer.Material.DiffuseColor.ToVector3(), value => { meshRenderer.Material.DiffuseColor = new Microsoft.Xna.Framework.Color(value); onDirty?.Invoke(); });
            }

            var terrain = _gameObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                var section = _tree.Add("Terrain");
                AddVector3(section, "Mesh Size", terrain.Geometry.Size, value =>
                {
                    terrain.Geometry.Size = value;
                    terrain.Build();
                    onDirty?.Invoke();
                });
                AddFloat(section, "Sand", terrain.WeightData.SandLayer, value => { terrain.SetWeightData(value, terrain.WeightData.GroundLayer, terrain.WeightData.RockLayer, terrain.WeightData.SnowLayer); onDirty?.Invoke(); });
                AddFloat(section, "Ground", terrain.WeightData.GroundLayer, value => { terrain.SetWeightData(terrain.WeightData.SandLayer, value, terrain.WeightData.RockLayer, terrain.WeightData.SnowLayer); onDirty?.Invoke(); });
                AddFloat(section, "Rock", terrain.WeightData.RockLayer, value => { terrain.SetWeightData(terrain.WeightData.SandLayer, terrain.WeightData.GroundLayer, value, terrain.WeightData.SnowLayer); onDirty?.Invoke(); });
                AddFloat(section, "Snow", terrain.WeightData.SnowLayer, value => { terrain.SetWeightData(terrain.WeightData.SandLayer, terrain.WeightData.GroundLayer, terrain.WeightData.RockLayer, value); onDirty?.Invoke(); });
            }

            var boxCollider = _gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                var section = _tree.Add("BoxCollider");
                AddVector3(section, "Center", boxCollider.Center, value => { boxCollider.SetCenter(value.X, value.Y, value.Z); onDirty?.Invoke(); });
                AddVector3(section, "Size", boxCollider.Size, value => { boxCollider.SetSize(value.X, value.Y, value.Z); onDirty?.Invoke(); });
                AddBool(section, "Pickable", boxCollider.IsPickable, value => { boxCollider.IsPickable = value; onDirty?.Invoke(); });
                AddBool(section, "Trigger", boxCollider.IsTrigger, value => { boxCollider.IsTrigger = value; onDirty?.Invoke(); });
            }

            var sphereCollider = _gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                var section = _tree.Add("SphereCollider");
                AddFloat(section, "Radius", sphereCollider.Sphere.Radius, value =>
                {
                    sphereCollider.Sphere = new BoundingSphere(sphereCollider.Sphere.Center, value);
                    onDirty?.Invoke();
                });
                AddBool(section, "Pickable", sphereCollider.IsPickable, value => { sphereCollider.IsPickable = value; onDirty?.Invoke(); });
                AddBool(section, "Trigger", sphereCollider.IsTrigger, value => { sphereCollider.IsTrigger = value; onDirty?.Invoke(); });
            }

            var rigidbody = _gameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                var section = _tree.Add("Rigidbody");
                AddBool(section, "IsStatic", rigidbody.IsStatic, value => { rigidbody.IsStatic = value; onDirty?.Invoke(); });
                AddBool(section, "Gravity", rigidbody.Gravity, value => { rigidbody.Gravity = value; onDirty?.Invoke(); });
                AddBool(section, "IsKinematic", rigidbody.IsKinematic, value => { rigidbody.IsKinematic = value; onDirty?.Invoke(); });
                AddVector3(section, "Velocity", rigidbody.Velocity, value => { rigidbody.Velocity = value; onDirty?.Invoke(); });
                AddVector3(section, "AngularVelocity", rigidbody.AngularVelocity, value => { rigidbody.AngularVelocity = value; onDirty?.Invoke(); });
            }

            _tree.ExpandAll();
        }

        private static void AddString(Properties parent, string label, string value, Action<string> changed)
        {
            var control = new StringControl(parent);
            control.SetValue(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddBool(Properties parent, string label, bool value, Action<bool> changed)
        {
            var control = new BoolControl(parent);
            control.SetBool(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddFloat(Properties parent, string label, float value, Action<float> changed)
        {
            var control = new FloatControl(parent);
            control.SetFloat(value);
            control.Changed += changed;
            parent.Add(label, control);
        }

        private static void AddVector3(Properties parent, string label, Vector3 value, Action<Vector3> changed)
        {
            var control = new Vector3Control(parent);
            control.SetVector(value);
            control.Vector3Changed += (_, x, y, z) => changed(new Vector3(x, y, z));
            parent.Add(label, control);
        }

        private static void AddEnum(Properties parent, string label, string selected, string[] values, Action<string> changed)
        {
            var control = new EnumControl(parent, values);
            control.SetSelected(selected);
            control.Changed += changed;
            parent.Add(label, control);
        }
    }
}
