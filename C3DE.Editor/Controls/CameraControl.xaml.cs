using C3DE.Components;
using System.Windows.Controls;
using System.Windows.Media;

namespace C3DE.Editor.Controls
{
    public partial class CameraControl : UserControl
    {
        private Camera camera;

        public bool Enabled
        {
            get
            {
                if (camera != null)
                    return camera.Enabled;

                return false;
            }
            set
            {
                if (camera != null)
                    camera.Enabled = value;
            }
        }

        public Color ClearColor
        {
            get
            {
                if (camera != null)
                    return Color.FromArgb(camera.ClearColor.A, camera.ClearColor.R, camera.ClearColor.G, camera.ClearColor.B);;

                return Color.FromArgb(1, 0, 0, 0);
            }
            set
            {
                if (camera != null)
                    camera.ClearColor = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A);
            }
        }

        public int Projection
        {
            get
            {
                if (camera != null)
                    return (int)camera.ProjectionType;

                return 0;
            }
            set
            {
                if (camera != null)
                    camera.ProjectionType = (CameraProjectionType)value;
            }
        }

        public float FOV
        {
            get
            {
                if (camera != null)
                    return camera.FieldOfView;

                return 0.0f;
            }
            set
            {
                if (camera != null)
                    camera.FieldOfView = value;
            }
        }

        public float NearClip
        {
            get
            {
                if (camera != null)
                    return camera.Near;

                return 0.0f;
            }
            set
            {
                if (camera != null)
                    camera.Near = value;
            }
        }

        public float FarClip
        {
            get
            {
                if (camera != null)
                    return camera.Far;

                return 0.0f;
            }
            set
            {
                if (camera != null)
                    camera.Far = value;
            }
        }

        public CameraControl()
        {
            InitializeComponent();
        }

        public CameraControl(Camera cam)
            : this()
        {
            camera = cam;
            DataContext = this;
        }
    }
}
