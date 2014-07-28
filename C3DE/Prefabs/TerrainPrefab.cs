using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Prefabs
{
    public class TerrainPrefab : SceneObject
    {
        protected MeshRenderer renderer;
        private TerrainGeometry geometry;
        private Vector2 repeatTexture;

        public MeshRenderer Renderer
        {
            get { return renderer; }
        }

        public Vector2 TextureRepeat
        {
            get { return geometry.RepeatTexture; }
            set { geometry.RepeatTexture = value; }
        }
        
        public TerrainPrefab(string name)
            : base()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;

            geometry = new TerrainGeometry(100, 100, 1);

            renderer = AddComponent<MeshRenderer>();
            renderer.Geometry = geometry;
        }

        /// <summary>
        /// Genearate the terrain with a heightmap texture.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="heightmap"></param>
        public void LoadHeightmap(GraphicsDevice device, Texture2D heightmap)
        {
            geometry.Width = heightmap.Width;
            geometry.Depth = heightmap.Height;

            Color[] colors = new Color[geometry.Width * geometry.Depth];
            heightmap.GetData(colors);

            geometry.Data = new float[geometry.Width, geometry.Depth];

            for (int x = 0; x < geometry.Width; x++)
                for (int y = 0; y < geometry.Depth; y++)
                    geometry.Data[x, y] = colors[x + y * geometry.Width].R / 10.0f; // Max height 25.5f

            Finalize(device);
        }

        /// <summary>
        /// Randomize the heightdata with the perlin noise algorithm.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="octaves"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="persistence"></param>
        public void Randomize(GraphicsDevice device, int octaves = 2, int amplitude = 22, double frequency = 0.085, double persistence = 0.3)
        {
            geometry.Data = new float[geometry.Width, geometry.Depth];

            NoiseGenerator.GenerateNoise(octaves, amplitude, frequency, persistence);

            for (int x = 0; x < geometry.Width; x++)
            {
                for (int z = 0; z < geometry.Depth; z++)
                    geometry.Data[x, z] = (float)NoiseGenerator.Noise(x, z);
            }

            Finalize(device);
        }

        public void Flat(GraphicsDevice device)
        {
            Finalize(device);
        }

        private void Finalize(GraphicsDevice device)
        {
            renderer.Geometry.Generate(device);
            renderer.ComputeBoundingSphere();
        }

        public void ApplyCollision(Transform tr)
        {
            var y = (GetTerrainHeight(tr.Position.X, 0, tr.Position.Z) + 15 - tr.Position.Y) * 0.2f;
            tr.Translate(0.0f, y, 0.0f);
        }

        public void ApplyCollision(ref Vector3 position)
        {
            var y = (GetTerrainHeight(position) + 2 - position.Y) * 0.2f;
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
    }
}
