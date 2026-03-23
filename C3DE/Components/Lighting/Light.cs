using C3DE.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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

        /// <summary>
        /// Marks this directional light as the preferred post-processing sun source.
        /// </summary>
        public bool IsSun { get; set; }

        public Vector3 Direction
        {
            get
            {
                var rotation = _transform.Rotation;
                var matrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                // Unity-style convention: a light points along its local +Z axis.
                // MonoGame's Vector3.Forward is -Z, so use Backward here to preserve
                // Unity-like authoring semantics from Transform rotation.
                return Vector3.Normalize(Vector3.Transform(Vector3.Backward, matrix));
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
            var direction = Direction;
            var up = Math.Abs(Vector3.Dot(direction, Vector3.Up)) > 0.999f ? Vector3.Right : Vector3.Up;

            if (Type == LightType.Directional)
            {
                var size = Math.Max(sphere.Radius, 1.0f);
                var distance = size * 2.0f;
                var position = sphere.Center - direction * distance;
                _viewMatrix = Matrix.CreateLookAt(position, sphere.Center, up);
                _projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, 0.1f, distance + size * 2.0f);
                return;
            }

            if (Type == LightType.Spot)
            {
                var position = _transform.Position;
                _viewMatrix = Matrix.CreateLookAt(position, position + direction, up);
                var fov = MathHelper.Clamp(Angle * 2.0f, 0.1f, MathHelper.Pi - 0.01f);
                _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov, 1.0f, 0.1f, Math.Max(Radius, 1.0f));
                return;
            }

            var pointPosition = _transform.Position;
            var target = sphere.Center;
            if (Vector3.DistanceSquared(pointPosition, target) < 0.0001f)
                target = pointPosition + Vector3.Forward;

            _viewMatrix = Matrix.CreateLookAt(pointPosition, target, up);
            float fallbackSize = Math.Max(sphere.Radius, 1.0f);
            float fallbackDistance = Vector3.Distance(pointPosition, target);
            _projectionMatrix = Matrix.CreateOrthographicOffCenter(-fallbackSize, fallbackSize, fallbackSize, -fallbackSize, Math.Max(0.1f, fallbackDistance - sphere.Radius), fallbackDistance + sphere.Radius * 2.0f);
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
