using C3DE.Materials;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Demo
{
    public class Demo : Engine
    {
        public static Dictionary<string, Material> CreateMaterials(ContentManager content, Scene scene)
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>(10);

            var material = new DiffuseSpecularMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/tech_box2");
            materials.Add("box2", material);

            material = new DiffuseSpecularMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/huleShip");
            materials.Add("huleShip", material);

            material = new DiffuseSpecularMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/heightmapTexture");
            materials.Add("terrain", material);

            material = new DiffuseSpecularMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/terrainTexture");
            materials.Add("terrain2", material);

            return materials;
        }
    }

    // Entry point.
    static class Program
    {
        static void Main(string[] args)
        {
            using (var game = new TerrainDemo())
                game.Run();
        }
    }
}
