using C3DE.Graphics;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Rendering;
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
        private Effect _deferredAmbientEffect;
        private Effect _deferredDirLightEffect;
        private Effect _deferredPointLightEffect;
        private QuadRenderer _quadRenderer;
        private SphereMesh _sphereMesh;
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

            _quadRenderer = new QuadRenderer(Application.GraphicsDevice);

            if (_transform != null)
                _boundingSphere = new BoundingSphere(_transform.Position, Radius);

            var content = Application.Content;
            _deferredAmbientEffect = content.Load<Effect>("Shaders/Deferred/AmbientLight");
            _deferredDirLightEffect = content.Load<Effect>("Shaders/Deferred/DirectionalLight");
            _deferredPointLightEffect = content.Load<Effect>("Shaders/Deferred/PointLight");
            _sphereMesh = new SphereMesh(1, 8);
            _sphereMesh.Build();
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

        public void RenderDeferred(RenderTarget2D colorMap, RenderTarget2D normalMap, RenderTarget2D depthMap, Camera camera)
        {
            var graphics = Application.GraphicsDevice;
            var invertViewProjection = Matrix.Invert(camera._viewMatrix * camera._projectionMatrix);
            var ambientColor = Scene.current.RenderSettings.AmbientColor.ToVector3();

            if (Type == LightType.Ambient)
            {
              /*  _deferredAmbientEffect.Parameters["Color"].SetValue(_color);
                _deferredDirLightEffect.Parameters["ColorMap"].SetValue(colorMap);
                _deferredAmbientEffect.Parameters["DepthMap"].SetValue(depthMap);
                _deferredPointLightEffect.Parameters["World"].SetValue(Matrix.Identity);
                _deferredAmbientEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderFullscreenQuad();*/
            }
            else if (Type == LightType.Directional)
            {
                _deferredDirLightEffect.Parameters["AmbientColor"].SetValue(ambientColor);
                _deferredDirLightEffect.Parameters["ColorMap"].SetValue(colorMap);
                _deferredDirLightEffect.Parameters["NormalMap"].SetValue(normalMap);
                _deferredDirLightEffect.Parameters["DepthMap"].SetValue(depthMap);
                _deferredDirLightEffect.Parameters["Color"].SetValue(_color);
                _deferredDirLightEffect.Parameters["Intensity"].SetValue(Intensity);
                _deferredDirLightEffect.Parameters["CameraPosition"].SetValue(camera._transform.Position);
                _deferredDirLightEffect.Parameters["InvertViewProjection"].SetValue(invertViewProjection);
                _deferredDirLightEffect.Parameters["LightPosition"].SetValue(_transform.LocalPosition);
                _deferredDirLightEffect.Parameters["World"].SetValue(_transform._worldMatrix);
                _deferredDirLightEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderFullscreenQuad();
            }
            else
            {
                var previousRS = graphics.RasterizerState;
                var sphereWorldMatrix = Matrix.CreateScale(Radius) * Matrix.CreateTranslation(_transform.Position);

                _deferredPointLightEffect.Parameters["AmbientColor"].SetValue(ambientColor);
                _deferredPointLightEffect.Parameters["ColorMap"].SetValue(colorMap);
                _deferredPointLightEffect.Parameters["NormalMap"].SetValue(normalMap);
                _deferredPointLightEffect.Parameters["DepthMap"].SetValue(depthMap);
                _deferredPointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
                _deferredPointLightEffect.Parameters["LightPosition"].SetValue(_transform.Position);
                _deferredPointLightEffect.Parameters["Color"].SetValue(_color);
                _deferredPointLightEffect.Parameters["Radius"].SetValue(Radius);
                _deferredPointLightEffect.Parameters["Intensity"].SetValue(Intensity);
                _deferredPointLightEffect.Parameters["View"].SetValue(camera._viewMatrix);
                _deferredPointLightEffect.Parameters["Projection"].SetValue(camera._projectionMatrix);
                _deferredPointLightEffect.Parameters["InvertViewProjection"].SetValue(invertViewProjection);
                _deferredPointLightEffect.Parameters["CameraPosition"].SetValue(camera._transform.Position);

                var inside = Vector3.Distance(camera._transform.Position, _transform.Position) < (Radius * 1.25f);
                graphics.RasterizerState = inside ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;

                _deferredPointLightEffect.CurrentTechnique.Passes[0].Apply();

                graphics.SetVertexBuffer(_sphereMesh.VertexBuffer);
                graphics.Indices = _sphereMesh.IndexBuffer;
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _sphereMesh.IndexBuffer.IndexCount / 3);

                graphics.RasterizerState = previousRS;
            }
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
