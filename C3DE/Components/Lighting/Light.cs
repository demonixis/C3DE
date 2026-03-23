using C3DE.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Lighting
{
    public enum LightType
    {
        Ambient = 0, Directional, Point, Spot
    }

    public enum LightPrority
    {
        Auto = 0, High
    }

    public class Light : Component
    {
        internal protected Matrix _viewMatrix;
        internal protected Matrix _projectionMatrix;
        internal protected ShadowGenerator _shadowGenerator;
        internal protected Vector3 _color = Color.White.ToVector3();
        private BoundingSphere _boundingSphere;

        public Matrix View => _viewMatrix;

        public Matrix Projection => _projectionMatrix;

        public LightPrority Priority { get; set; } = LightPrority.Auto;

        public Vector3 Direction
        {
            get
            {
                var position = _transform.Position;
                var rotation = _transform.Rotation;
                var matrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                return position + Vector3.Transform(Vector3.Forward, matrix);
            }
        }

        public BoundingSphere BoundingSphere => _boundingSphere;

        public bool ShadowEnabled
        {
            get => _shadowGenerator.Enabled;
            set { _shadowGenerator.Enabled = value; }
        }

        public ShadowGenerator ShadowGenerator
        {
            get => _shadowGenerator;
            protected set { _shadowGenerator = value; }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        public Color Color
        {
            get => new Color(_color);
            set { _color = value.ToVector3(); }
        }

        /// <summary>
        /// The intensity of the light.
        /// </summary>
        public float Intensity { get; set; } = 1.0f;

        /// <summary>
        /// The maximum distance of emission.
        /// </summary>
        public float Radius { get; set; } = 25;

        public float FallOf { get; set; } = 2.0f;

        /// <summary>
        /// The type of the light.
        /// </summary>
        public LightType Type { get; set; } = LightType.Directional;

        /// <summary>
        /// The angle used by the Spot light.
        /// </summary>
        public float Angle { get; set; } = MathHelper.PiOver4;

        public Light()
            : base()
        {
            _viewMatrix = Matrix.Identity;
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 1000);
            _viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            _shadowGenerator = new ShadowGenerator();
        }

        public override void Start()
        {
            base.Start();

            _shadowGenerator.Initialize();

            if (_transform != null)
                _boundingSphere = new BoundingSphere(_transform.Position, Radius);
        }

        public override void Update()
        {
            base.Update();

            if (!_gameObject.IsStatic)
            {
                _boundingSphere.Radius = Radius;
                _boundingSphere.Center = _transform.Position;
            }
        }

        // Need to be changed quickly !
        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - _gameObject.Transform.Position;
            dir.Normalize();

            _viewMatrix = Matrix.CreateLookAt(_transform.Position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(_transform.LocalPosition, sphere.Center);
            _projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }

        public override void Dispose()
        {
            _shadowGenerator.Dispose();
        }

        public override int CompareTo(object obj)
        {
            var light = obj as Light;

            if (light == null)
                return -1;

            if (Type == LightType.Directional || Priority == LightPrority.High)
                return 0;
            else
                return 1;
        }
    }
}
