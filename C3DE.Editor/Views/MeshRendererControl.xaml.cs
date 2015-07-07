using C3DE.Components.Renderers;
using C3DE.Geometries;
using System;
using System.Windows.Controls;

namespace C3DE.Editor.Controls
{
    /// <summary>
    /// Logique d'interaction pour MeshRendererControl.xaml
    /// </summary>
    public partial class MeshRendererControl : UserControl
    {
        public MeshRenderer meshRenderer { get; set; }

        private string[] KnownGeometries = new string[]
        {
            "Cube", "Cylinder", "Plane", 
            "Pyramid", "Quad", "Sphere", 
            "Terrain", "Torus", "Other"
        };

        public int Geometry
        {
            get { return SelectedGeometry.SelectedIndex; }
            set
            {
                SelectedGeometry.SelectedIndex = value;

                if (meshRenderer != null)
                {
                    var geometry = Activator.CreateInstance(Type.GetType(String.Format("C3DE.Geometries.{0}Geometry", KnownGeometries[value])));

                    if (geometry != null)
                        meshRenderer.Geometry = geometry as Geometry;
                }
            }
        }

        public bool CastShadow
        {
            get { return IsCastShadow.IsChecked.HasValue ? IsCastShadow.IsChecked.Value : false; }
            set
            {
                IsCastShadow.IsChecked = value;

                if (meshRenderer != null)
                    meshRenderer.CastShadow = value;
            }
        }

        public bool ReceiveShadow
        {
            get { return IsReceiveShadow.IsChecked.HasValue ? IsReceiveShadow.IsChecked.Value : false; }
            set
            {
                IsReceiveShadow.IsChecked = value;

                if (meshRenderer != null)
                    meshRenderer.ReceiveShadow = value;
            }
        }

        public int Material
        {
            get { return SelectedMaterials.SelectedIndex; }
            set
            {
                SelectedMaterials.SelectedIndex = value;

                if (meshRenderer != null)
                    meshRenderer.Material = Scene.current.Materials[value];
            }
        }

        public MeshRendererControl()
        {
            InitializeComponent();

            SelectedGeometry.Items.Clear();
            for (int i = 0; i < KnownGeometries.Length; i++)
                SelectedGeometry.Items.Add(KnownGeometries[i]);

            SelectedMaterials.Items.Clear();
            for (int i = 0; i < Scene.current.Materials.Count; i++)
                SelectedMaterials.Items.Add(Scene.current.Materials[i].Name);

            SelectedGeometry.SelectedIndex = 0;
            SelectedMaterials.SelectedIndex = 0;

            this.DataContext = this;
        }

        public void Set(MeshRenderer mRenderer)
        {
            meshRenderer = mRenderer;

            var tmp = mRenderer.Geometry.ToString().Split('.');
            var geo = tmp[tmp.Length - 1];
            var name = geo.Replace("Geometry", "");
            var index = Array.IndexOf(KnownGeometries, name);
            index = index == -1 ? KnownGeometries.Length - 1 : index;
            SelectedGeometry.SelectedIndex = index;

            IsCastShadow.IsChecked = mRenderer.CastShadow;
            IsReceiveShadow.IsChecked = mRenderer.ReceiveShadow;
            SelectedMaterials.SelectedIndex = Scene.current.Materials.IndexOf(mRenderer.Material);
        }
    }
}
