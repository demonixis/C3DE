using C3DE.Components;
using C3DE.Editor.Events;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    public partial class TransformControl : UserControl
    {
        private Transform transform;

        public float PositionX
        {
            get
            {
                if (transform != null)
                    return transform.Position.X;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetPosition(value, null, null);
            }
        }

        public float PositionY
        {
            get
            {
                if (transform != null)
                    return transform.Position.Y;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetPosition(null, value, null);
            }
        }

        public float PositionZ
        {
            get
            {
                if (transform != null)
                    return transform.Position.Z;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetPosition(null, null, value);
            }
        }

        public float RotationX
        {
            get
            {
                if (transform != null)
                    return transform.Rotation.X;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetRotation(value, null, null);
            }
        }

        public float RotationY
        {
            get
            {
                if (transform != null)
                    return transform.Rotation.Y;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetRotation(null, value, null);
            }
        }

        public float RotationZ
        {
            get
            {
                if (transform != null)
                    return transform.Rotation.Z;
                return 0.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetRotation(null, null, value);
            }
        }

        public float ScaleX
        {
            get
            {
                if (transform != null)
                    return transform.LocalScale.X;
                return 1.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetScale(value, null, null);
            }
        }

        public float ScaleY
        {
            get
            {
                if (transform != null)
                    return transform.LocalScale.Y;
                return 1.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetScale(null, value, null);
            }
        }

        public float ScaleZ
        {
            get
            {
                if (transform != null)
                    return transform.LocalScale.Z;
                return 1.0f;
            }
            set
            {
                if (transform != null)
                    transform.SetScale(null, null, value);
            }
        }

        public TransformControl()
        {
            InitializeComponent();
        }

        public TransformControl(Transform tr)
            : this()
        {
            transform = tr;
            DataContext = this;
        }
    }
}
