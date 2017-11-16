using Microsoft.Xna.Framework;
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
        internal protected Matrix viewMatrix;
        internal protected Matrix projectionMatrix;
        internal protected ShadowGenerator shadowGenerator;
        internal protected Vector3 color = Color.White.ToVector3();

        public Matrix View => viewMatrix;

        public Matrix Projection => projectionMatrix;

        public Vector3 Direction => Vector3.Normalize(sceneObject.Transform.Position);

        [DataMember]
        public bool EnableShadow
        {
            get => shadowGenerator.Enabled;
            set { shadowGenerator.Enabled = value; }
        }

        [DataMember]
        public ShadowGenerator ShadowGenerator
        {
            get => shadowGenerator;
            protected set { shadowGenerator = value; }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        [DataMember]
        public Color Color
        {
            get => new Color(color);
            set { color = value.ToVector3(); }
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
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 1000);
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            shadowGenerator = new ShadowGenerator(this);
        }

        public override void Start()
        {
            shadowGenerator.Initialize();
        }

        // Need to be changed quickly !
        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - sceneObject.Transform.LocalPosition;
            dir.Normalize();

            viewMatrix = Matrix.CreateLookAt(transform.LocalPosition, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(transform.LocalPosition, sphere.Center);
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
            else
                return -1;
        }
    }
}
