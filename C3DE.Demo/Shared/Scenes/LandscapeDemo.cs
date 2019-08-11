using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Diagnostic;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class LandscapeDemo : BaseDemo
    {
        public LandscapeDemo() : base("Landscape")
        {
        }

        protected override void SceneSetup()
        {
            // We don't want the default behaviour so we leave this override empty.
        }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 0.75f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            lightGo.AddComponent<DemoBehaviour>();

            var player = GameObjectFactory.CreatePlayer();
            player.AddComponent<FirstPersonController>();

            var probe = GameObjectFactory.CreateReflectionProbe(new Vector3(0, 25, 0));

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.BlueSkybox);

            var terrain = GameObjectFactory.CreateTerrain();

            terrain.SetWeightData(0.1f, 0.2f, 5f, 18f);
            terrain.Randomize(1, 2);
            terrain.Renderer.Material = GetTerrainMaterial(content, terrain.GenerateWeightMap());
            terrain.Renderer.ReceiveShadow = true;
            terrain.AddComponent<StatsDisplay>();

            var treeModel = content.Load<Model>("Models/Tree/Tree");

            GameObject tree = null;
            Vector3 position;

            var scale = 0.01f;
            var range = 30;

            for (var i = 0; i < 50; i++)
            {
                tree = treeModel.ToMeshRenderers(PreferePBRMaterials);
                tree.Transform.LocalScale = new Vector3(scale);

                position.X = RandomHelper.Range(-range, range);
                position.Y = 0;
                position.Z = RandomHelper.Range(-range, range);

                position.Y = terrain.GetTerrainHeight(position) - 1;

                tree.Transform.Position = position * scale;
            }

            var renderers = tree.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
                PatchMaterial(renderer.Material, renderer.Material.MainTexture.Name.ToLower().Contains("branch"));

            AddLightGroundTest(100);
        }

        private void PatchMaterial(Material material, bool cutout)
        {
            var std = material as StandardMaterial;
            if (std == null) // PBR Shader doesn't support cutout for now.
                return;

            std.DiffuseColor = Color.White;
            std.NormalMap = null;
            std.SpecularColor = Color.White;
            std.SpecularMap = null;
            std.SpecularPower = 32;

            if (cutout)
            {
                std.CutoutEnabled = true;
                std.Cutout = 0.25f;
            }
        }
    }
}
