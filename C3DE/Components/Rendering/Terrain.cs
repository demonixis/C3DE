using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Components.Rendering
{
    [DataContract]
    public struct TerrainWeightData
    {
        public float SandLayer { get; set; }
        public float GroundLayer { get; set; }
        public float RockLayer { get; set; }
        public float SnowLayer { get; set; }
    }

    public class Terrain : Component
    {
        protected MeshRenderer m_Renderer;
        protected TerrainMesh m_Geometry;
        protected TerrainWeightData m_WeightData;

        public MeshRenderer Renderer => m_Renderer;
        public TerrainWeightData WeightData => m_WeightData;
        public TerrainMesh Geometry => m_Geometry;

        public int Width => (int)(m_Geometry.Width * m_Geometry.Size.X);
        public int Height => (int)(m_Geometry.Height * m_Geometry.Size.Y);
        public int Depth => (int)(m_Geometry.Depth * m_Geometry.Size.Z);

        public override void Awake()
        {
            base.Awake();

            m_Geometry = new TerrainMesh(100, 100, 1);
            m_Renderer = GetComponent<MeshRenderer>();

            if (m_Renderer == null)
                m_Renderer = AddComponent<MeshRenderer>();

            m_Renderer.Geometry = m_Geometry;
            m_Renderer.CastShadow = false;
            m_Renderer.ReceiveShadow = true;

            m_WeightData = new TerrainWeightData()
            {
                SandLayer = 9,
                GroundLayer = 18,
                RockLayer = 23,
                SnowLayer = 27
            };
        }

        /// <summary>
        /// Genearate the terrain with a heightmap texture.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="heightmap"></param>
        public void LoadHeightmap(string heightmapName)
        {
            var heightmap = Application.Content.Load<Texture2D>(heightmapName);

            m_Geometry.Width = heightmap.Width;
            m_Geometry.Depth = heightmap.Height;

            Color[] colors = new Color[m_Geometry.Width * m_Geometry.Depth];
            heightmap.GetData(colors);

            m_Geometry.Data = new float[m_Geometry.Width, m_Geometry.Depth];

            for (int x = 0; x < m_Geometry.Width; x++)
                for (int y = 0; y < m_Geometry.Depth; y++)
                    m_Geometry.Data[x, y] = colors[x + y * m_Geometry.Width].R / 10.0f; // Max height 25.5f

            Build();
        }

        /// <summary>
        /// Randomize the heightdata with the perlin noise algorithm.
        /// </summary>
        /// <param name="octaves"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="persistence"></param>
        public void Randomize(int octaves = 2, int amplitude = 22, double frequency = 0.085, double persistence = 0.3, bool limit = false)
        {
            m_Geometry.Data = new float[m_Geometry.Width, m_Geometry.Depth];

            NoiseGenerator.GenerateNoise(octaves, amplitude, frequency, persistence);

            for (int x = 0; x < m_Geometry.Width; x++)
            {
                for (int z = 0; z < m_Geometry.Depth; z++)
                    m_Geometry.Data[x, z] = (float)NoiseGenerator.Noise(x, z, limit);
            }

            Build();
        }

        public void Flatten()
        {
            Build();
        }

        public void Build()
        {
            m_Renderer.Geometry.Build();
        }

        public void ApplyCollision(Transform tr)
        {
            var y = (GetTerrainHeight(tr.LocalPosition.X, 0, tr.LocalPosition.Z) + 2 * m_Geometry.Size.Y - tr.LocalPosition.Y) * 0.2f;
            tr.Translate(0.0f, y, 0.0f);
        }

        public void ApplyCollision(ref Vector3 position)
        {
            var y = (GetTerrainHeight(position) - position.Y) * 0.2f;
            position.Y += y;
        }

        public virtual float GetTerrainHeight(Vector3 position)
        {
            return GetTerrainHeight(position.X, position.Y, position.Z);
        }

        public virtual float GetTerrainHeight(float x, float y, float z)
        {
            // Terrain space.
            x -= m_Transform.LocalPosition.X;
            y -= m_Transform.LocalPosition.Y;
            z -= m_Transform.LocalPosition.Z;

            float terrainHeigth = 0.0f;

            float sizedPosX = (x / m_Geometry.Size.X) / m_Transform.LocalScale.X;
            float sizedPosZ = (z / m_Geometry.Size.Z) / m_Transform.LocalScale.Z;

            int px = (int)((x / m_Geometry.Size.X) / m_Transform.LocalScale.X);
            int pz = (int)((z / m_Geometry.Size.Z) / m_Transform.LocalScale.Z);

            if (px < 0 || px >= m_Geometry.Data.GetLength(0) - 1 || pz < 0 || pz >= m_Geometry.Data.GetLength(1) - 1)
                terrainHeigth = y;
            else
            {
                float triangleY0 = m_Geometry.Data[px, pz];
                float triangleY1 = m_Geometry.Data[px + 1, pz];
                float triangleY2 = m_Geometry.Data[px, pz + 1];
                float triangleY3 = m_Geometry.Data[px + 1, pz + 1];

                // Determine where are the point
                float segX = sizedPosX - px;
                float segZ = sizedPosZ - pz;

                // We are on the first triangle
                if ((segX + segZ) < 1)
                {
                    terrainHeigth = triangleY0;
                    terrainHeigth += (triangleY1 - triangleY0) * segX;
                    terrainHeigth += (triangleY2 - triangleY0) * segZ;
                }
                else // Second triangle
                {
                    terrainHeigth = triangleY3;
                    terrainHeigth += (triangleY1 - triangleY3) * segX;
                    terrainHeigth += (triangleY2 - triangleY3) * segZ;
                }
            }

            return (terrainHeigth * m_Geometry.Size.Y * m_Transform.LocalScale.Y);
        }

        public void SetWeightData(float sand, float ground, float rock, float snow)
        {
            m_WeightData.SandLayer = sand;
            m_WeightData.GroundLayer = ground;
            m_WeightData.RockLayer = rock;
            m_WeightData.SnowLayer = snow;
        }

        public Texture2D GenerateWeightMap()
        {
            var width = m_Geometry.Width;
            var depth = m_Geometry.Depth;

            var wMap = new Texture2D(Application.GraphicsDevice, width, depth, false, SurfaceFormat.Color);
            var colors = new Color[width * depth];
            float data = 0;

            for (int x = 0; x < m_Geometry.Width; x++)
            {
                for (int z = 0; z < m_Geometry.Depth; z++)
                {
                    data = m_Geometry.Data[x, z];

                    if (data < m_WeightData.SandLayer)
                        colors[x + z * width] = Color.Red;

                    else if (data >= m_WeightData.SandLayer && data < m_WeightData.GroundLayer)
                        colors[x + z * width] = Color.Black;

                    else if (data >= m_WeightData.GroundLayer && data < m_WeightData.RockLayer)
                        colors[x + z * width] = Color.Green;

                    else
                        colors[x + z * width] = Color.Blue;
                }
            }

            wMap.SetData(colors);

            return wMap;
        }
    }
}
