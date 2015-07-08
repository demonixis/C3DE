using C3DE.Components;
using System.Windows.Controls;
using System.Windows.Media;

namespace C3DE.Editor.Controls
{
    public partial class CameraControl : UserControl
    {
        private Camera camera;

        public string ClearColor
        {
            get
            {
                if (camera != null)
                {
                    var color = Color.FromArgb(camera.ClearColor.A, camera.ClearColor.R, camera.ClearColor.G, camera.ClearColor.B);
                    return string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
                }

                return "#ffffff";
            }
            set
            {
                if (camera != null)
                {
                    var strColor = value;
                    var color = (Color)ColorConverter.ConvertFromString(strColor);
                    camera.ClearColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                    ClearColorRect.Fill = new SolidColorBrush(color);
                }
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
