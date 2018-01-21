using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts.Editor;
using C3DE.Editor.Core;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNAGizmo;

namespace C3DE.Demo.Scenes
{
    public class EditorDemo : SimpleDemo
    {
        public enum Primitive
        {
            Cube, Cylinder, Quad, Plane, Pyramid, Sphere, Torus
        }

        private const string EditorTag = "Editor_Object";
        private EditorApp m_EditorApp;
        private GizmoComponent m_Gizmo;
        private ObjectSelector m_Selector;

        public EditorDemo() : base("3D Editor")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var controller = m_Camera.GetComponent<FirstPersonController>();
            controller.MouseEnabled = false;

            m_EditorApp = m_Camera.AddComponent<EditorApp>();
            m_EditorApp.Clicked += M_EditorApp_Clicked;
            m_Gizmo = new GizmoComponent(Application.Engine, Application.GraphicsDevice);
            m_Gizmo.ActiveMode = GizmoMode.Translate;

            Application.Engine.Components.Add(m_Gizmo);

            m_Gizmo.TranslateEvent += M_Gizmo_TranslateEvent;
            m_Gizmo.RotateEvent += M_Gizmo_RotateEvent;
            m_Gizmo.ScaleEvent += M_Gizmo_ScaleEvent;

            m_Selector = new ObjectSelector();

            SetControlMode(Scripts.ControllerSwitcher.ControllerType.FPS, new Vector3(0, 2, -10), Vector3.Zero, true);

            // Grid
            var gridMaterial = new UnlitMaterial();
            gridMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(0.6f, 0.6f, 0.6f), new Color(0.95f, 0.95f, 0.95f), 256, 256); ;
            gridMaterial.Tiling = new Vector2(24);

            var terrain = GameObjectFactory.CreateTerrain();
            terrain.Tag = EditorTag;

            var grid = terrain.GetComponent<Terrain>();
            grid.Renderer.Material = gridMaterial;
            grid.Renderer.ReceiveShadow = true;
            grid.Renderer.CastShadow = false;
            grid.Flatten();
        }

        private void M_EditorApp_Clicked(string name)
        {
            GameObject selected = null;
            switch (name)
            {
                case "Cube": selected = CreatePrimitive(Primitive.Cube); break;
                case "Sphere": selected = CreatePrimitive(Primitive.Sphere);break;
            }

            if (selected != null)
            {
                m_Selector.Set(selected);
                m_Gizmo.Selection.Add(selected.Transform);
            }
        }

        public override void Unload()
        {
            base.Unload();
            Application.Engine.Components.Remove(m_Gizmo);
        }

        private void M_Gizmo_ScaleEvent(Transform target, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value;

            if (m_Gizmo.ActiveMode == GizmoMode.UniformScale)
                target.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                target.LocalScale += delta;

            target.LocalScale = Vector3.Clamp(target.LocalScale, Vector3.Zero, target.LocalScale);
        }

        private void M_Gizmo_RotateEvent(Transform target, TransformationEventArgs e)
        {
            //m_Gizmo.RotationHelper(target, e);
        }

        private void M_Gizmo_TranslateEvent(Transform target, TransformationEventArgs e)
        {
            var value = (Vector3)e.Value;
    
            if (Input.Keys.Pressed(Keys.LeftControl))
            {
                if (Vector3.Distance((Vector3)e.Value, target.Position) > 0.05f)
                {
                    var x = target.Position.X + Math.Sign(value.X);
                    var y = target.Position.Y + Math.Sign(value.Y);
                    var z = target.Position.Z + Math.Sign(value.Z);
                    target.SetPosition(x, y, z);
                }
            }
            else
                target.LocalPosition += value;
        }

        private GameObject CreatePrimitive(Primitive type)
        {
            var go = new GameObject();
            var mr = go.AddComponent<MeshRenderer>();

            switch (type)
            {
                case Primitive.Cube: mr.Geometry = new CubeMesh(); break;
                case Primitive.Cylinder: mr.Geometry = new CylinderMesh(); break;
                case Primitive.Plane: mr.Geometry = new PlaneMesh(); break;
                case Primitive.Pyramid: mr.Geometry = new PyramidMesh(); break;
                case Primitive.Quad: mr.Geometry = new QuadMesh(); break;
                case Primitive.Sphere: mr.Geometry = new SphereMesh(); break;
                case Primitive.Torus: mr.Geometry = new TorusMesh(); break;
            }

            mr.Geometry.Build();
            return go;
        }
    }
}
