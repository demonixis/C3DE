﻿using C3DE.Components.Renderers;
using C3DE.Geometries;
using System;
using System.Collections.Generic;
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
            get
            {
                if (meshRenderer != null)
                    return GetGeometryIndex(meshRenderer.Geometry);

                return 0;
            }
            set
            {
                if (meshRenderer != null)
                {
                    var typeName = String.Format("C3DE.Geometries.{0}Geometry,C3DE", KnownGeometries[value]);
                    var type = Type.GetType(typeName);

                    if (type == null)
                        return;

                    var geometry = Activator.CreateInstance(type);

                    if (geometry != null)
                    {
                        meshRenderer.Geometry = geometry as Geometry;
                        meshRenderer.Geometry.Buid();
                    }
                }
            }
        }

        public bool CastShadow
        {
            get
            {
                if (meshRenderer != null)
                    return meshRenderer.CastShadow;

                return false;
            }
            set
            {
                if (meshRenderer != null)
                    meshRenderer.CastShadow = value;
            }
        }

        public bool ReceiveShadow
        {
            get
            {
                if (meshRenderer != null)
                    return meshRenderer.ReceiveShadow;

                return false;
            }
            set
            {
                if (meshRenderer != null)
                    meshRenderer.ReceiveShadow = value;
            }
        }

        public int Material
        {
            get
            {
                if (meshRenderer != null)
                    return Scene.current.Materials.IndexOf(meshRenderer.Material);

                return 0;
            }
            set
            {
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
        }

        public MeshRendererControl(MeshRenderer renderer)
            : this()
        {
            meshRenderer = renderer;
            DataContext = this;
        }

        public void Set(MeshRenderer mRenderer)
        {
            meshRenderer = mRenderer;
            this.DataContext = this;
        }

        private int GetGeometryIndex(Geometry geometry)
        {
            var tmp = geometry.ToString().Split('.');
            var geo = tmp[tmp.Length - 1];
            var name = geo.Replace("Geometry", "");
            var index = Array.IndexOf(KnownGeometries, name);
            return index == -1 ? KnownGeometries.Length - 1 : index;
        }
    }
}
