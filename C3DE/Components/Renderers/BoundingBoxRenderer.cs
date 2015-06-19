using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Components;
using C3DE.Rendering;

namespace C3DE.Components.Renderers
{
    public class BoundingBoxRenderer : RenderableComponent
    {
        private VertexPositionColor[] _vertices = new VertexPositionColor[8];
        private static short[] _indices = new short[]
        {
            0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4
        };

        private static BasicEffect _effect;
        private RenderableComponent _renderer;

        public override void Awake()
        {
            base.Awake();

            if (_effect == null)
            {
                _effect = new BasicEffect(Application.GraphicsDevice);
                _effect.VertexColorEnabled = true;
                _effect.LightingEnabled = false;
            }
        }

        public override void Start()
        {
            base.Start();

            _renderer = GetComponent<RenderableComponent>();

            if (_renderer == null)
                return;
        }

        public override void ComputeBoundingSphere()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            _renderer.ComputeBoundingSphere();

            var box = BoundingBox.CreateFromSphere(_renderer.boundingSphere);
            var corners = box.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                _vertices[i].Position = corners[i];
                _vertices[i].Color = Color.Green;
            }

            _effect.View = Camera.Main.view;
            _effect.Projection = Camera.Main.projection;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Application.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, _vertices, 0, 8, _indices, 0, _indices.Length / 2);
            }
        }
    }
}