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
        protected MeshRenderer _renderer;
        protected TerrainMesh _geometry;
        protected TerrainWeightData _weightData;

        public MeshRenderer Renderer => _renderer;
        public TerrainWeightData WeightData => _weightData;
        public TerrainMesh Geometry => _geometry;

        public int Width => (int)(_geometry.Width * _geometry.Size.X);
        public int Height => (int)(_geometry.Height * _geometry.Size.Y);
        public int Depth => (int)(_geometry.Depth * _geometry.Size.Z);

        public override void Awake()
        {
            base.Awake();

            _geometry = new TerrainMesh(100, 100, 1);
            _renderer = GetComponent<MeshRenderer>();

            if (_renderer == null)
                _renderer = AddComponent<MeshRenderer>();

            _renderer.Mesh = _geometry;
            _renderer.CastShadow = false;
            _renderer.ReceiveShadow = true;

            _weightData = new TerrainWeightData()
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

            _geometry.Width = heightmap.Width;
            _geometry.Depth = heightmap.Height;

            Color[] colors = new Color[_geometry.Width * _geometry.Depth];
            heightmap.GetData(colors);

            _geometry.Data = new float[_geometry.Width, _geometry.Depth];

            for (int x = 0; x < _geometry.Width; x++)
                for (int y = 0; y < _geometry.Depth; y++)
                    _geometry.Data[x, y] = colors[x + y * _geometry.Width].R / 10.0f; // Max height 25.5f

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
            _geometry.Data = new float[_geometry.Width, _geometry.Depth];

            NoiseGenerator.GenerateNoise(octaves, amplitude, frequency, persistence);

            for (int x = 0; x < _geometry.Width; x++)
            {
                for (int z = 0; z < _geometry.Depth; z++)
                    _geometry.Data[x, z] = (float)NoiseGenerator.Noise(x, z, limit);
            }

            Build();
        }

        public void Flatten()
        {
            Build();
        }

        public void Build()
        {
            _renderer.Mesh.Build();
        }

        public void ApplyCollision(Transform tr)
        {
            var y = (GetTerrainHeight(tr.LocalPosition.X, 0, tr.LocalPosition.Z) + 2 * _geometry.Size.Y - tr.LocalPosition.Y) * 0.2f;
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
            x -= _transform.LocalPosition.X;
            y -= _transform.LocalPosition.Y;
            z -= _transform.LocalPosition.Z;

            float terrainHeigth = 0.0f;

            float sizedPosX = (x / _geometry.Size.X) / _transform.LocalScale.X;
            float sizedPosZ = (z / _geometry.Size.Z) / _transform.LocalScale.Z;

            int px = (int)((x / _geometry.Size.X) / _transform.LocalScale.X);
            int pz = (int)((z / _geometry.Size.Z) / _transform.LocalScale.Z);

            if (px < 0 || px >= _geometry.Data.GetLength(0) - 1 || pz < 0 || pz >= _geometry.Data.GetLength(1) - 1)
                terrainHeigth = y;
            else
            {
                float triangleY0 = _geometry.Data[px, pz];
                float triangleY1 = _geometry.Data[px + 1, pz];
                float triangleY2 = _geometry.Data[px, pz + 1];
                float triangleY3 = _geometry.Data[px + 1, pz + 1];

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

            return (terrainHeigth * _geometry.Size.Y * _transform.LocalScale.Y);
        }

        public void SetWeightData(float sand, float ground, float rock, float snow)
        {
            _weightData.SandLayer = sand;
            _weightData.GroundLayer = ground;
            _weightData.RockLayer = rock;
            _weightData.SnowLayer = snow;
        }

        public Texture2D GenerateWeightMap()
        {
            var width = _geometry.Width;
            var depth = _geometry.Depth;

            var wMap = new Texture2D(Application.GraphicsDevice, width, depth, false, SurfaceFormat.Color);
            var colors = new Color[width * depth];
            float data = 0;

            for (int x = 0; x < _geometry.Width; x++)
            {
                for (int z = 0; z < _geometry.Depth; z++)
                {
                    data = _geometry.Data[x, z];

                    if (data < _weightData.SandLayer)
                        colors[x + z * width] = Color.Red;

                    else if (data >= _weightData.SandLayer && data < _weightData.GroundLayer)
                        colors[x + z * width] = Color.Black;

                    else if (data >= _weightData.GroundLayer && data < _weightData.RockLayer)
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
