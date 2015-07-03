using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Logique d'interaction pour MeshRendererControl.xaml
    /// </summary>
    public partial class MeshRendererControl : UserControl
    {
        private string[] KnownGeometries = new string[]
        {
            "Cube", "Cylinder", "Plane", 
            "Pyramid", "Quad", "Sphere", 
            "Terrain", "Torus", "Other"
        };

        public int Geometry
        {
            get { return SelectedGeometry.SelectedIndex; }
            set { SelectedGeometry.SelectedIndex = value; }
        }

        public bool CastShadow
        {
            get { return IsCastShadow.IsChecked.HasValue ? IsCastShadow.IsChecked.Value : false; }
            set { IsCastShadow.IsChecked = value; }
        }

        public bool ReceiveShadow
        {
            get { return IsReceiveShadow.IsChecked.HasValue ? IsReceiveShadow.IsChecked.Value : false; }
            set { IsReceiveShadow.IsChecked = value; }
        }

        public int Materials
        {
            get { return SelectedMaterials.SelectedIndex; }
            set { SelectedMaterials.SelectedIndex = value; }
        }

        public MeshRendererControl()
        {
            InitializeComponent();

            SelectedGeometry.Items.Clear();

            for (int i = 0; i < KnownGeometries.Length; i++)
                SelectedGeometry.Items.Add(KnownGeometries[i]);

            SelectedMaterials.SelectedIndex = KnownGeometries.Length - 1;
        }

        public void Set(string geometry, bool castShadow, bool receiveShadow, int material)
        {
            var tmp = geometry.Split('.');
            var geo = tmp[tmp.Length - 1];
            var name = geo.Replace("Geometry", "");
            var index = Array.IndexOf(KnownGeometries, name);
            index = index == -1 ? KnownGeometries.Length - 1 : index;
            SelectedGeometry.SelectedIndex = index;

            IsCastShadow.IsChecked = castShadow;
            IsReceiveShadow.IsChecked = receiveShadow;
            SelectedMaterials.SelectedIndex = material;
        }
    }
}
