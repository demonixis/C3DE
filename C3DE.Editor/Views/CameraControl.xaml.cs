using C3DE.Components;
using System.Windows.Controls;
using System.Windows.Media;

namespace C3DE.Editor.Controls
{
    public partial class CameraControl : UserControl
    {
        private Camera camera { get; set; }

        public Color ClearColor
        {
            get
            {
                if (camera != null)
                    return Color.FromArgb(camera.ClearColor.A, camera.ClearColor.R, camera.ClearColor.G, camera.ClearColor.B);

                return Color.FromRgb(0, 0, 0);
            }
            set
            {
                CamClearColor.Fill = new SolidColorBrush(value);

                if (camera != null)
                    camera.ClearColor = new Microsoft.Xna.Framework.Color(value.R, value.G, value.B);
            }
        }

        public CameraProjectionType Projection
        {
            get { return CamProjection.SelectedIndex == 0 ? CameraProjectionType.Orthographic : CameraProjectionType.Perspective; }
            set
            {
                CamProjection.SelectedIndex = value == CameraProjectionType.Orthographic ? 0 : 1;

                if (camera != null)
                    camera.ProjectionType = value == 0 ? CameraProjectionType.Orthographic : CameraProjectionType.Perspective;
            }
        }

        public float FOV
        {
            get { return float.Parse(CamFOV.Text); }
            set
            {
                CamFOV.Text = value.ToString();

                if (camera != null)
                    camera.FieldOfView = value;
            }
        }

        public float NearClip
        {
            get { return float.Parse(CamNearClip.Text); }
            set
            {
                CamNearClip.Text = value.ToString();

                if (camera != null)
                    camera.Near = value;
            }
        }

        public float FarClip
        {
            get { return float.Parse(CamFarClip.Text); }
            set
            {
                CamFarClip.Text = value.ToString();

                if (camera != null)
                    camera.Far = value;
            }
        }

        public CameraControl()
        {
            InitializeComponent();
        }

        public void Set(Camera camComponent)
        {
            camera = camComponent;
            CamProjection.SelectedIndex = camComponent.ProjectionType == CameraProjectionType.Orthographic ? 0 : 1;
            CamFOV.Text = camComponent.FieldOfView.ToString();
            CamNearClip.Text = camComponent.Near.ToString();
            CamFarClip.Text = camComponent.Far.ToString();
            CamClearColor.Fill = new SolidColorBrush(Color.FromArgb(camComponent.ClearColor.A, camComponent.ClearColor.R, camComponent.ClearColor.G, camComponent.ClearColor.B));
        }
    }
}
