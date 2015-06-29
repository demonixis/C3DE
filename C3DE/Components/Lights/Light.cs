using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Lights
{
    public enum LightType
    {
        Ambient = 0, Directional, Point, Spot
    }

	public enum LightRenderMode
	{
		RealTime = 0, Backed
	}

    public class Light : Component
    {
        internal protected Matrix viewMatrix;
        internal protected Matrix projectionMatrix;
        internal protected ShadowGenerator shadowGenerator;
        internal protected Vector3 diffuseColor;

        public Matrix View
        {
            get { return viewMatrix; }
        }

        public Matrix Projection
        {
            get { return projectionMatrix; }
        }

        public bool EnableShadow
        {
            get { return shadowGenerator.Enabled; }
            set { shadowGenerator.Enabled = value; }
        }

        public ShadowGenerator ShadowGenerator
        {
            get { return shadowGenerator; }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        public Color DiffuseColor
        {
            get { return new Color(diffuseColor); }
            set { diffuseColor = value.ToVector3(); }
        }

        /// <summary>
        /// The intensity of the light.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// The maximum distance of emission.
        /// </summary>
        public float Range { get; set; }

		public LightRenderMode Backing { get; set; }

        public float FallOf { get; set; }

        /// <summary>
        /// The type of the light.
        /// </summary>
        public LightType Type { get; set; }

        /// <summary>
        /// The direction of the directional light.
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// The angle used by the Spot light.
        /// </summary>
        public float Angle { get; set; }

        public Light()
            : base()
        {
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            Intensity = 1.0f;
            Direction = new Vector3(1, 1, 0);
            Type = LightType.Ambient;
            Range = 5000.0f;
            FallOf = 2.0f;
			Backing = LightRenderMode.RealTime;
            shadowGenerator = new ShadowGenerator(this);
        }

        public override void Start()
        {
            shadowGenerator.Initialize();
        }

        // Need to be changed quickly !
        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - sceneObject.Transform.Position;
            dir.Normalize();

            viewMatrix = Matrix.CreateLookAt(sceneObject.Transform.Position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(sceneObject.Transform.Position, sphere.Center);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }

        public void DrawShadowMap(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            sb.Draw(shadowGenerator.ShadowMap, new Rectangle(0, 0, 100, 100), Color.White);
            sb.End();
        }

        public override void Dispose()
        {
            shadowGenerator.Dispose();
        }
    }
}
