using C3DE.Components;
using Gwen;
using Gwen.Control;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Editor.UI.Items
{
    public class TransformControl : ControlBase
    {
        private Vector3Control _position;
        private Vector3Control _rotation;
        private Vector3Control _scale;
        private Transform _transform;

        public event Action<string, string, float> ValueChanged = null;

        public TransformControl(ControlBase parent = null)
            : base(parent)
        {
            var tree = new PropertyTree(this);
            tree.Dock = Dock.Top;
            tree.Width = 300;
            tree.AutoSizeToContent = true;

            var transform = tree.Add("Transform");
            _position = AddControl(transform, "Translation", UpdateTranslation);
            _rotation = AddControl(transform, "Rotation", UpdateRotation);
            _scale = AddControl(transform, "Scale", UpdateScale);
        }

        private Vector3Control AddControl(Properties parent, string label, Action<Vector3Control, float, float, float> callback)
        {
            var control = new Vector3Control(parent);
            control.Vector3Changed += callback;
            control.UserData = label;
            parent.Add(label, control);
            return control;
        }

        private void UpdateTranslation(Vector3Control control, float x, float y, float z)
        {
            if (_transform == null)
                return;

            _transform.SetLocalPosition(x, y, z);
        }

        private void UpdateRotation(Vector3Control control, float x, float y, float z)
        {
            if (_transform == null)
                return;

            _transform.SetLocalRotation(x, y, z);
        }

        private void UpdateScale(Vector3Control control, float x, float y, float z)
        {
            if (_transform == null)
                return;

            _transform.SetLocalScale(x, y, z);
        }

        public void SetGameObject(GameObject gameObject)
        {
            _transform = gameObject?.Transform;

            if (_transform == null)
            {
                _position.SetVector(Vector3.Zero);
                _rotation.SetVector(Vector3.Zero);
                _scale.SetVector(Vector3.Zero);
            }
            else
            {
                _position.SetVector(_transform.LocalPosition);
                _rotation.SetVector(_transform.LocalRotation);
                _scale.SetVector(_transform.LocalScale);
            }
        }
    }
}
