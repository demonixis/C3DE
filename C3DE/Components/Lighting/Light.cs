using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Components.Lighting
{
    public enum LightType
    {
        Ambient = 0, Directional, Point, Spot
    }

    [DataContract]
    public class Light : Component
    {
        internal protected Matrix m_ViewMatrix;
        internal protected Matrix m_ProjectionMatrix;
        internal protected ShadowGenerator m_ShadowGenerator;
        internal protected Vector3 m_Color = Color.White.ToVector3();
        private Effect m_DeferredDirLightEffect;
        private Effect m_DeferredPointLightEffect;
        private QuadRenderer m_QuadRenderer;
        private SphereMesh m_SphereMesh;

        public Matrix View => m_ViewMatrix;

        public Matrix Projection => m_ProjectionMatrix;

        public Vector3 Direction => Vector3.Normalize(m_Transform.LocalRotation);

        [DataMember]
        public bool EnableShadow
        {
            get => m_ShadowGenerator.Enabled;
            set { m_ShadowGenerator.Enabled = value; }
        }

        [DataMember]
        public ShadowGenerator ShadowGenerator
        {
            get => m_ShadowGenerator;
            protected set { m_ShadowGenerator = value; }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        [DataMember]
        public Color Color
        {
            get => new Color(m_Color);
            set { m_Color = value.ToVector3(); }
        }

        /// <summary>
        /// The intensity of the light.
        /// </summary>
        [DataMember]
        public float Intensity { get; set; } = 1.0f;

        /// <summary>
        /// The maximum distance of emission.
        /// </summary>
        [DataMember]
        public float Range { get; set; } = 25;

        [DataMember]
        public float FallOf { get; set; } = 5.0f;

        /// <summary>
        /// The type of the light.
        /// </summary>
        [DataMember]
        public LightType TypeLight { get; set; } = LightType.Directional;

        /// <summary>
        /// The angle used by the Spot light.
        /// </summary>
        [DataMember]
        public float Angle { get; set; } = MathHelper.PiOver4;

        public Light()
            : base()
        {
            m_ViewMatrix = Matrix.Identity;
            m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 1000);
            m_ViewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            m_ShadowGenerator = new ShadowGenerator(this);
        }

        public override void Start()
        {
            m_ShadowGenerator.Initialize();

            m_QuadRenderer = new QuadRenderer(Application.GraphicsDevice);

            var content = Application.Content;
            m_DeferredDirLightEffect = content.Load<Effect>("Shaders/Deferred/DirectionalLight");
            m_DeferredPointLightEffect = content.Load<Effect>("Shaders/Deferred/PointLight");

            m_SphereMesh = new SphereMesh(1, 8);
            m_SphereMesh.Build();
        }

        // Need to be changed quickly !
        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - m_GameObject.Transform.LocalPosition;
            dir.Normalize();

            m_ViewMatrix = Matrix.CreateLookAt(m_Transform.LocalPosition, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(m_Transform.LocalPosition, sphere.Center);
            m_ProjectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }

        public void RenderDeferred(RenderTarget2D colorMap, RenderTarget2D normalMap, RenderTarget2D depthMap, Camera camera)
        {
            var graphics = Application.GraphicsDevice;
            var invertViewProjection = Matrix.Invert(camera.m_ViewMatrix * camera.m_ProjectionMatrix);

            if (TypeLight == LightType.Directional)
            {
                m_DeferredDirLightEffect.Parameters["ColorMap"].SetValue(colorMap);
                m_DeferredDirLightEffect.Parameters["NormalMap"].SetValue(normalMap);
                m_DeferredDirLightEffect.Parameters["DepthMap"].SetValue(depthMap);
                m_DeferredDirLightEffect.Parameters["Color"].SetValue(m_Color);
                m_DeferredDirLightEffect.Parameters["Intensity"].SetValue(Intensity);
                m_DeferredDirLightEffect.Parameters["CameraPosition"].SetValue(camera.m_Transform.Position);
                m_DeferredDirLightEffect.Parameters["InvertViewProjection"].SetValue(invertViewProjection);
                m_DeferredDirLightEffect.Parameters["LightPosition"].SetValue(m_Transform.LocalPosition);
                m_DeferredDirLightEffect.Parameters["World"].SetValue(m_Transform.m_WorldMatrix);
                m_DeferredDirLightEffect.CurrentTechnique.Passes[0].Apply();
            }
            else
            {
                var sphereWorldMatrix = Matrix.CreateScale(Range) * Matrix.CreateTranslation(m_Transform.Position);

                m_DeferredPointLightEffect.Parameters["ColorMap"].SetValue(colorMap);
                m_DeferredPointLightEffect.Parameters["NormalMap"].SetValue(normalMap);
                m_DeferredPointLightEffect.Parameters["DepthMap"].SetValue(depthMap);
                m_DeferredPointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
                m_DeferredPointLightEffect.Parameters["LightPosition"].SetValue(m_Transform.Position);
                m_DeferredPointLightEffect.Parameters["Color"].SetValue(m_Color);
                m_DeferredPointLightEffect.Parameters["Radius"].SetValue(Range);
                m_DeferredPointLightEffect.Parameters["Intensity"].SetValue(Intensity);
                m_DeferredPointLightEffect.Parameters["View"].SetValue(camera.m_ViewMatrix);
                m_DeferredPointLightEffect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
                m_DeferredPointLightEffect.Parameters["InvertViewProjection"].SetValue(invertViewProjection);
                m_DeferredPointLightEffect.Parameters["CameraPosition"].SetValue(camera.m_Transform.Position);

                var inside = Vector3.Distance(camera.m_Transform.Position, m_Transform.Position) < Range;
                graphics.RasterizerState = inside ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;

                m_DeferredPointLightEffect.CurrentTechnique.Passes[0].Apply();

                graphics.SetVertexBuffer(m_SphereMesh.VertexBuffer);
                graphics.Indices = m_SphereMesh.IndexBuffer;
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_SphereMesh.IndexBuffer.IndexCount / 3);
            }

            m_QuadRenderer.RenderFullscreenQuad(Application.GraphicsDevice);
        }

        public override void Dispose()
        {
            m_ShadowGenerator.Dispose();
        }

        public override int CompareTo(object obj)
        {
            var light = obj as Light;

            if (light == null)
                return 1;

            if (TypeLight == light.TypeLight)
                return 0;
            else
                return -1;
        }
    }
}
