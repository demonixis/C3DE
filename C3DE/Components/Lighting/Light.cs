using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Lighting
{
    public enum LightType
    {
        Ambient = 0, Directional, Point, Spot
    }

	public enum LightRenderMode
	{
		RealTime = 0, Backed
	}

    [DataContract]
    public class Light : Component
    {
        internal protected Matrix viewMatrix;
        internal protected Matrix projectionMatrix;
        internal protected ShadowGenerator shadowGenerator;
        internal protected Vector3 color;

        public Matrix View
        {
            get { return viewMatrix; }
        }

        public Matrix Projection
        {
            get { return projectionMatrix; }
        }

        [DataMember]
        public bool EnableShadow
        {
            get { return shadowGenerator.Enabled; }
            set { shadowGenerator.Enabled = value; }
        }

        [DataMember]
        public ShadowGenerator ShadowGenerator
        {
            get { return shadowGenerator; }
            protected set { shadowGenerator = value; }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        [DataMember]
        public Color Color
        {
            get { return new Color(color); }
            set { color = value.ToVector3(); }
        }

        /// <summary>
        /// The intensity of the light.
        /// </summary>
        [DataMember]
        public float Intensity { get; set; }

        /// <summary>
        /// The maximum distance of emission.
        /// </summary>
        [DataMember]
        public float Range { get; set; }

        [DataMember]
		public LightRenderMode Backing { get; set; }

        [DataMember]
        public float FallOf { get; set; }

        /// <summary>
        /// The type of the light.
        /// </summary>
        [DataMember]
        public LightType TypeLight { get; set; }

        /// <summary>
        /// The angle used by the Spot light.
        /// </summary>
        [DataMember]
        public float Angle { get; set; }

        public Light()
            : base()
        {
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            color = new Vector3(1.0f, 1.0f, 1.0f);
            Intensity = 1.0f;
            TypeLight = LightType.Ambient;
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

            viewMatrix = Matrix.CreateLookAt(transform.Position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(transform.Position, sphere.Center);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }

        public override void Dispose()
        {
            shadowGenerator.Dispose();
        }

        public override int CompareTo(object obj)
        {
            var light = obj as Light;

            if (light == null)
                return 1;

            if (TypeLight == light.TypeLight)
                return 0;
            else if (Backing == LightRenderMode.RealTime && light.Backing == LightRenderMode.Backed)
                return 1;
            else
                return -1;
        }
    }
}
