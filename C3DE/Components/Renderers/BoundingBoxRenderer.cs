using C3DE.Components.Colliders;
using C3DE.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Renderers
{
    /// <summary>
    /// A debug renderer which can draw a bouding box. The component allow you to use 
    /// a bounding box or a bounding sphere in input.
    /// </summary>
    public class BoundingBoxRenderer : RenderableComponent
    {
        private VertexPositionColor[] _vertices;
        private short[] _indices;
        private BasicEffect effect;
        private BoxCollider _box;
        private SphereCollider _sphere;
        private Color _color;

        /// <summary>
        /// Create a bounding box renderer.
        /// </summary>
        public BoundingBoxRenderer()
            : base()
        {
            _vertices = new VertexPositionColor[8];
            _indices = new short[] { 0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4 };
        }

        /// <summary>
        /// Gets the collider attached on the object and uses it for rendering.
        /// </summary>
        public override void Start()
        {
            _box = GetComponent<BoxCollider>();
            _sphere = GetComponent<SphereCollider>();

            if (_box == null && _sphere == null)
                throw new Exception("You need to attach a box or sphere collider first before using this script.");

            _color = RandomHelper.GetColor();
            effect = new BasicEffect(Application.GraphicsDevice);
            Material = new DummyMaterial(sceneObject.Scene);
        }

        public override void ComputeBoundingSphere()
        {
        }

        public override void Update()
        {
            if (_box != null)
            {
                Vector3[] corners = _box.Box.GetCorners();

                for (int i = 0; i < 8; i++)
                {
                    _vertices[i].Position = corners[i];
                    _vertices[i].Color = _color;
                }
            }

            else if (_sphere != null)
            {
                BoundingBox box = BoundingBox.CreateFromSphere(_sphere.Sphere);

                Vector3[] corners = box.GetCorners();

                for (int i = 0; i < 8; i++)
                {
                    _vertices[i].Position = corners[i];
                    _vertices[i].Color = _color;
                }
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;
            effect.View = sceneObject.Scene.MainCamera.view;
            effect.Projection = sceneObject.Scene.MainCamera.projection;
            effect.World = Matrix.Identity;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Application.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, _vertices, 0, 8, _indices, 0, _indices.Length / 2);
            }
        }
    }
}
