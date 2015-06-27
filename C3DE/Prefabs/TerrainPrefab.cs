using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Prefabs
{
    public struct TerrainWeightData
    {
        public float SandLayer { get; set; }
        public float GroundLayer { get; set; }
        public float RockLayer { get; set; }
        public float SnowLayer { get; set; }
    }

    public class TerrainPrefab : SceneObject
    {
        protected MeshRenderer renderer;
        protected TerrainGeometry geometry;
        protected TerrainWeightData weightData;
        // protected TerrainCollider collider;

        public MeshRenderer Renderer
        {
            get { return renderer; }
        }

        public TerrainWeightData WeightData 
        {
            get { return weightData; }
        }

        public int Width
        {
            get { return (int)(geometry.Width * geometry.Size.X); }
        }

        public int Height
        {
            get { return (int)(geometry.Height * geometry.Size.Y); }
        }

        public int Depth
        {
            get { return (int)(geometry.Depth * geometry.Size.Z); }
        }

        public TerrainPrefab(string name)
            : this()
        {
            Name = name;
        }

        public TerrainPrefab()
            : base()
        {
            Name = "TerrainPrefab-" + System.Guid.NewGuid();
            geometry = new TerrainGeometry(100, 100, 1);

            renderer = AddComponent<MeshRenderer>();
            renderer.Geometry = geometry;
            renderer.CastShadow = false;
            renderer.ReceiveShadow = true;

            weightData = new TerrainWeightData()
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

            geometry.Width = heightmap.Width;
            geometry.Depth = heightmap.Height;

            Color[] colors = new Color[geometry.Width * geometry.Depth];
            heightmap.GetData(colors);

            geometry.Data = new float[geometry.Width, geometry.Depth];

            for (int x = 0; x < geometry.Width; x++)
                for (int y = 0; y < geometry.Depth; y++)
                    geometry.Data[x, y] = colors[x + y * geometry.Width].R / 10.0f; // Max height 25.5f

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
            geometry.Data = new float[geometry.Width, geometry.Depth];

            NoiseGenerator.GenerateNoise(octaves, amplitude, frequency, persistence);

            for (int x = 0; x < geometry.Width; x++)
            {
                for (int z = 0; z < geometry.Depth; z++)
                    geometry.Data[x, z] = (float)NoiseGenerator.Noise(x, z, limit);
            }

            Build();
        }

        public void Flat()
        {
            Build();
        }

        public void Build()
        {
            renderer.Geometry.Generate();
        }

        public void ApplyCollision(Transform tr)
        {
            var y = (GetTerrainHeight(tr.Position.X, 0, tr.Position.Z) + 2 * geometry.Size.Y - tr.Position.Y) * 0.2f;
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
            x -= transform.Position.X;
            y -= transform.Position.Y;
            z -= transform.Position.Z;

            float terrainHeigth = 0.0f;

            float sizedPosX = (x / geometry.Size.X) / transform.LocalScale.X;
            float sizedPosY = (y / geometry.Size.Y) / transform.LocalScale.Y;
            float sizedPosZ = (z / geometry.Size.Z) / transform.LocalScale.Z;

            int px = (int)((x / geometry.Size.X) / transform.LocalScale.X);
            int pz = (int)((z / geometry.Size.Z) / transform.LocalScale.Z);

            if (px < 0 || px >= geometry.Data.GetLength(0) - 1 || pz < 0 || pz >= geometry.Data.GetLength(1) - 1)
                terrainHeigth = y;
            else
            {
                float triangleY0 = geometry.Data[px, pz];
                float triangleY1 = geometry.Data[px + 1, pz];
                float triangleY2 = geometry.Data[px, pz + 1];
                float triangleY3 = geometry.Data[px + 1, pz + 1];

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

            return (terrainHeigth * geometry.Size.Y * transform.LocalScale.Y);
        }

        public void SetWeightData(float sand, float ground, float rock, float snow)
        {
            weightData.SandLayer = sand;
            weightData.GroundLayer = ground;
            weightData.RockLayer = rock;
            weightData.SnowLayer = snow;
        }

        public Texture2D GenerateWeightMap()
        {
            var width = geometry.Width;
            var depth = geometry.Depth;

            var wMap = new Texture2D(Application.GraphicsDevice, width, depth, false, SurfaceFormat.Color);
            var colors = new Color[width * depth];
            float data = 0;

            for (int x = 0; x < geometry.Width; x++)
            {
                for (int z = 0; z < geometry.Depth; z++)
                {
                    data = geometry.Data[x, z];

                    if (data < weightData.SandLayer)
                        colors[x + z * width] = Color.Red;

                    else if (data >= weightData.SandLayer && data < weightData.GroundLayer)
                        colors[x + z * width] = Color.Black;

                    else if (data >= weightData.GroundLayer && data < weightData.RockLayer)
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
