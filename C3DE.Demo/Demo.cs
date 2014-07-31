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

            StandardMaterial material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/tech_box");
            materials.Add("box", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/tech_box2");
            materials.Add("box2", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/huleShip");
            materials.Add("huleShip", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Models/texv1");
            materials.Add("spaceShip", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/marsTexture");
            materials.Add("mars", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/heightmapTexture");
            materials.Add("terrain", material);

            material = new StandardMaterial(scene);
            material.MainTexture = content.Load<Texture2D>("Textures/terrainTexture");
            materials.Add("terrain2", material);

            var skyboxMaterial = new SkyboxMaterial(scene);
            skyboxMaterial.Textures = new Texture2D[6] {
                content.Load<Texture2D>("Textures/Skybox/nx"),
                content.Load<Texture2D>("Textures/Skybox/ny"),
                content.Load<Texture2D>("Textures/Skybox/nz"),
                content.Load<Texture2D>("Textures/Skybox/px"),
                content.Load<Texture2D>("Textures/Skybox/py"),
                content.Load<Texture2D>("Textures/Skybox/pz")
            };

            return materials;
        }
    }

    // Entry point.
    static class Program
    {
        static void Main(string[] args)
        {
            using (var game = new ShaderDemo())
                game.Run();
        }
    }
}
