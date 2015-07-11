using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Components.Renderers
{
    [DataContract]
    public class BoundingBoxRenderer : Renderer
    {
        private VertexPositionColor[] _vertices = new VertexPositionColor[8];
        private static short[] _indices = new short[]
        {
            0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4
        };

        private static BasicEffect _effect;
        private Renderer _renderer;

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

            _renderer = GetComponent<Renderer>();

            if (_renderer == null)
                return;
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            _renderer.ComputeBoundingInfos();

            var corners = _renderer.boundingBox.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                _vertices[i].Position = corners[i];
                _vertices[i].Color = Color.Green;
            }

            _effect.World = Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, transform.Rotation.Z) * Matrix.CreateTranslation(transform.Position);
            _effect.View = Camera.main.view;
            _effect.Projection = Camera.main.projection;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Application.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, _vertices, 0, 8, _indices, 0, _indices.Length / 2);
            }
        }
    }
}