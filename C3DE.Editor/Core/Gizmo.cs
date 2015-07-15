using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Inputs;
using C3DE.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Editor.Core
{
    public class Gizmo : SceneObject
    {
        private Renderer[] renderers;

        public Gizmo()
            : base("Editor_Gizmo")
        {
            Tag = EDScene.EditorTag;
        }

        public override void Initialize()
        {
            base.Initialize();

            renderers = new Renderer[3];

            var rotations = new Vector3[3]
            {
                Vector3.Zero,
                new Vector3(MathHelper.PiOver2, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, MathHelper.PiOver2)
            };

            var materials = new Material[3]
            {
                new UnlitMaterial(EDScene.current) { Texture = GraphicsHelper.CreateTexture(Color.Green, 8, 8) },
                new UnlitMaterial(EDScene.current) { Texture = GraphicsHelper.CreateTexture(Color.Red, 8, 8) },
                new UnlitMaterial(EDScene.current) { Texture = GraphicsHelper.CreateTexture(Color.Blue, 8, 8) }
            };

            for (int i = 0; i < 3; i++)
            {
                renderers[i] = BuildCylinder();
                renderers[i].Material = materials[i];
                renderers[i].Transform.Rotation = rotations[i];
                Add(renderers[i].SceneObject);
            }

            Enabled = false;
        }

        private MeshRenderer BuildCylinder()
        {
            var so = new SceneObject();
            so.Tag = EDScene.EditorTag;
            var rdr = so.AddComponent<MeshRenderer>();
            rdr.CastShadow = false;
            rdr.ReceiveShadow = false;
            rdr.Geometry = new CylinderGeometry();
            rdr.Geometry.Size = new Vector3(0.1f, 5.0f, 0.1f);
            rdr.Geometry.Build();
            return rdr;
        }

        public override void Update()
        {
            base.Update();
            return;
            if (EDRegistry.Mouse.Down(MouseButton.Left))
            {
                var ray = EDRegistry.Camera.GetRay(EDRegistry.Mouse.Position);

                for (int i = 0; i < 3; i++)
                {
                    renderers[i].ComputeBoundingInfos();
                    var value = renderers[i].BoundingSphere.Intersects(ray);

                    if (value.HasValue)
                    {
                        if (i == 0)
                            transform.Parent.Translate(0, -EDRegistry.Mouse.Delta.Y, 0);
                        else if (i == 1)
                            transform.Parent.Translate(-EDRegistry.Mouse.Delta.X, 0, 0);
                        else if (i == 2)
                            transform.Parent.Translate(0, 0, -EDRegistry.Mouse.Delta.Y);
                    }
                }
            }
        }

        public void SetVisible(Transform target)
        {
            if (target == null)
                Enabled = false;

            transform.Parent = target;
            Enabled = true;
        }
    }
}
