using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Demo.Scenes
{
    public class SponzaDemo : Scene
    {
        public SponzaDemo() : base("Sponza Demo") { }

        public SponzaDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            // And a camera with some components
            var cameraGo = GameObjectFactory.CreateCamera();
            var controller = cameraGo.AddComponent<ControllerSwitcher>();
            cameraGo.AddComponent<DemoBehaviour>();
            cameraGo.AddComponent<LightSpawner>();
            Add(cameraGo);

            controller.DefaultPosition = new Vector3(-10.0f, 2.0f, 0.45f);
            controller.DefaultRotation = new Vector3(0.0f, -1.4f, 0.0f);
            controller.SetControllerActive(ControllerSwitcher.ControllerType.FPS);
            controller.FlyMode = true;

            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f);
            lightGo.Name = "DirectionalLight";
            lightGo.Transform.LocalPosition = new Vector3(250, 500, 100);
            lightGo.Transform.LocalRotation = new Vector3(1, 0.5f, 0);
            Add(lightGo);

            // Sponza Model
            var content = Application.Content;
            var sponzaModel = content.Load<Model>("Models/Sponza/sponza");
            var sponzaGo = sponzaModel.ToMeshRenderers(this);
            sponzaGo.Transform.Translate(0.0f, 1.0f, 0.0f);
            PatchMaterials(sponzaGo, content);

            // Sun Flares
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };
            var direction = lightGo.GetComponent<Light>().Direction;

            var sunflares = cameraGo.AddComponent<LensFlare>();
            sunflares.LightDirection = direction;
            sunflares.Setup(glowTexture, flareTextures);

            Screen.ShowCursor = true;

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.FogColor = Color.FloralWhite;
        }

        private void PatchMaterials(GameObject sponzaGo, ContentManager content)
        {
            // Of course, one day, we'll have a custom material importer ;)
            var matching = new Dictionary<string, string[]>();
            AddToCollection(matching, "background", "background_ddn", string.Empty);
            AddToCollection(matching, "chain_texture", "chain_texture_ddn", string.Empty);
            AddToCollection(matching, "lion", "lion_ddn", string.Empty);
            AddToCollection(matching, "spnza_bricks_a_diff", "spnza_bricks_a_ddn", "spnza_bricks_a_spec");
            AddToCollection(matching, "sponza_arch_diff", "sponza_arch_ddn", "sponza_arch_spec");
            AddToCollection(matching, "sponza_ceiling_a_diff", string.Empty, "sponza_ceiling_a_spec");
            AddToCollection(matching, "sponza_column_a_diff", "sponza_column_a_ddn", "sponza_column_a_spec");
            AddToCollection(matching, "sponza_column_b_diff", "sponza_column_a_ddn", "sponza_column_a_spec");
            AddToCollection(matching, "sponza_column_c_diff", "sponza_column_a_ddn", "sponza_column_a_spec");
            AddToCollection(matching, "sponza_curtain_blue_diff", string.Empty, "sponza_curtain_blue_spec");
            AddToCollection(matching, "sponza_curtain_diff", string.Empty, "sponza_curtain_blue_spec");
            AddToCollection(matching, "sponza_curtain_green_diff", string.Empty, "sponza_curtain_blue_spec");
            AddToCollection(matching, "sponza_details_diff", string.Empty, "sponza_details_spec");
            AddToCollection(matching, "sponza_fabric_blue_diff", string.Empty, "sponza_fabric_spec");
            AddToCollection(matching, "sponza_fabric_diff", string.Empty, "sponza_fabric_spec");
            AddToCollection(matching, "sponza_fabric_green_diff", string.Empty, "sponza_fabric_spec");
            AddToCollection(matching, "sponza_flagpole_diff", string.Empty, "sponza_flagpole_spec");
            AddToCollection(matching, "sponza_floor_a_diff", "sponza_floor_a_ddn", "sponza_floor_a_spec");
            AddToCollection(matching, "sponza_thorn_diff", "sponza_thorn_ddn", "sponza_thorn_spec");
            AddToCollection(matching, "vase_dif", string.Empty, "vase_ddn");
            AddToCollection(matching, "vase_plant", string.Empty, "vase_plant_spec");
            AddToCollection(matching, "vase_round", "vase_round_ddn", "vase_round_spec");

            var renderers = sponzaGo.GetComponentsInChildren<MeshRenderer>();
            var material = (StandardMaterial)null;
            var name = string.Empty;
            var temp = (string[])null;

            foreach (var renderer in renderers)
            {
                material = (StandardMaterial)renderer.Material;
                material.ReflectionTexture = RenderSettings.Skybox.Texture;

                if (material.MainTexture == null)
                    continue;

                name = System.IO.Path.GetFileNameWithoutExtension(material.MainTexture.Name);
                name = name.Replace("_0", "");

                if (matching.ContainsKey(name))
                {
                    temp = matching[name];

                    if (temp[0] != string.Empty)
                        material.NormalTexture = content.Load<Texture2D>($"Models/Sponza/textures/{temp[0]}");

                    if (temp[1] != string.Empty)
                        material.SpecularTexture = content.Load<Texture2D>($"Models/Sponza/textures/{temp[1]}");
                }
            }
        }

        private void AddToCollection(Dictionary<string, string[]> dico, string key, string normal, string specular)
        {
            dico.Add(key, new string[] { normal, specular });
        }
    }
}
