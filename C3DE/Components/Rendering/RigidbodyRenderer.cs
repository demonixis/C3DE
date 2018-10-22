using C3DE.Components.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Components.Rendering
{
    [DataContract]
    public class RigidbodyRenderer : Renderer
    {
        private static VertexPositionColor[] _vertices = new VertexPositionColor[8];
        private static short[] _indices = new short[]
        {
            0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4
        };

        private static BasicEffect _effect;
        private Renderer _renderer;
        private Rigidbody _rigidbody;

        public Color LineColor { get; set; }

        public RigidbodyRenderer()
            : base()
        {
            LineColor = Color.Green;
            CastShadow = false;
            ReceiveShadow = false;
        }

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
            _rigidbody = GetComponent<Rigidbody>();

            if (_renderer == null)
            {
                m_GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            var shape = _rigidbody.Shape;
            var box = new BoundingBox();
            box.Min = Rigidbody.ToVector3(shape.BoundingBox.Min);
            box.Max = Rigidbody.ToVector3(shape.BoundingBox.Max);
            var _corners = box.GetCorners();

            for (int i = 0; i < 8; i++)
            {
                _vertices[i].Position = _corners[i] * m_Transform.LocalScale;
                _vertices[i].Color = LineColor;
            }

            _effect.World = Matrix.CreateFromYawPitchRoll(m_Transform.LocalRotation.Y, m_Transform.LocalRotation.X, m_Transform.LocalRotation.Z) * Matrix.CreateTranslation(m_Transform.LocalPosition);
            _effect.View = Camera.Main.m_ViewMatrix;
            _effect.Projection = Camera.Main.m_ProjectionMatrix;
            _effect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserIndexedPrimitives(PrimitiveType.LineList, _vertices, 0, 8, _indices, 0, _indices.Length / 2);
        }
    }
}