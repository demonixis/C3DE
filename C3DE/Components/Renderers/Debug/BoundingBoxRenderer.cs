using C3DE.Components.Colliders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Renderers.Debug
{
    public class BoundingBoxRenderer : RenderableComponent
    {
        private BoxCollider collider;
        private VertexPositionColor[] verts = new VertexPositionColor[8];
        private readonly short[] indices = new short[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            0, 4,
            1, 5,
            2, 6,
            3, 7,
            4, 5,
            5, 6,
            6, 7,
            7, 4,
        };

        private BasicEffect effect;

        public BoundingBoxRenderer()
            : this(null)
        {
        }

        public BoundingBoxRenderer(SceneObject sceneObject)
            : base(sceneObject)
        {

        }

        public override void LoadContent(ContentManager content)
        {
            effect = new BasicEffect(Application.GraphicsDevice);
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;
            collider = GetComponent<BoxCollider>();

            if (collider == null)
                throw new Exception("You need to attach a BoxCollider to this SceneObject to use the BoundingBoxRenderer.");
        }

        public override void Draw(GraphicsDevice device)
        {
            Vector3[] corners = collider.Box.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                verts[i].Position = corners[i];
                verts[i].Color = Color.Green;
            }

            effect.View = sceneObject.Scene.MainCamera.view;
            effect.Projection = sceneObject.Scene.MainCamera.projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(PrimitiveType.LineList, verts, 0, 8, indices, 0, indices.Length / 2);
            }
        }

        public override BoundingSphere GetBoundingSphere()
        {
            return new BoundingSphere();
        }
    }
}
