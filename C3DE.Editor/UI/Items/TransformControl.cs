using C3DE.Components;
using Gwen;
using Gwen.Control;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Editor.UI.Items
{
    public class TransformControl : ControlBase
    {
        private Vector3Control m_Position;
        private Vector3Control m_Rotation;
        private Vector3Control m_Scale;
        private Transform m_Transform;

        public event Action<string, string, float> ValueChanged = null;

        public TransformControl(ControlBase parent = null)
            : base(parent)
        {
            var tree = new PropertyTree(this);
            tree.Dock = Dock.Top;
            tree.Width = 300;
            tree.AutoSizeToContent = true;

            var transform = tree.Add("Transform");
            m_Position = AddControl(transform, "Translation", UpdateTranslation);
            m_Rotation = AddControl(transform, "Rotation", UpdateRotation);
            m_Scale = AddControl(transform, "Scale", UpdateScale);
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
            if (m_Transform == null)
                return;

            m_Transform.SetLocalPosition(x, y, z);
        }

        private void UpdateRotation(Vector3Control control, float x, float y, float z)
        {
            if (m_Transform == null)
                return;

            m_Transform.SetLocalRotation(x, y, z);
        }

        private void UpdateScale(Vector3Control control, float x, float y, float z)
        {
            if (m_Transform == null)
                return;

            m_Transform.SetLocalScale(x, y, z);
        }

        public void SetGameObject(GameObject gameObject)
        {
            m_Transform = gameObject?.Transform;

            if (m_Transform == null)
            {
                m_Position.SetVector(Vector3.Zero);
                m_Rotation.SetVector(Vector3.Zero);
                m_Scale.SetVector(Vector3.Zero);
            }
            else
            {
                m_Position.SetVector(m_Transform.LocalPosition);
                m_Rotation.SetVector(m_Transform.LocalRotation);
                m_Scale.SetVector(m_Transform.LocalScale);
            }
        }
    }
}
