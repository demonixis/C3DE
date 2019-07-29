using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.Graphics;
using C3DE.Components.Rendering.Particles;
using System;
using C3DE.Utils;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : SimpleDemo
    {
        public TestDemo() : base("Test")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.Red, 10f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(0, 10, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            var _camera = camera.GetComponent<Camera>();
            _camera.AddComponent<DemoBehaviour>();
            var _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.BlueSkybox, 256);

            var content = Application.Content;
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.Black, Color.White);
            terrainMaterial.Tiling = new Vector2(16);
            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;

            var particleSystem = new GameObject("ParticleSystem");
            _particleSystem = particleSystem.AddComponent<ParticleSystem>();
            _particleSystem.Setup(Application.GraphicsDevice, content, SmokePlumeSettings);
        }

        private ParticleSystem _particleSystem;

        public override void Update()
        {
            base.Update();

            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
                _particleSystem.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }

        private Vector3 RandomPointOnCircle()
        {
            const float radius = 30;
            const float height = 40;

            double angle = RandomHelper.ValueF * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }

        public static ParticleSettings SmokePlumeSettings = new ParticleSettings()
        {
            BlendState = BlendState.NonPremultiplied,
            TextureName = "Textures/Particles/smoke",
            MaxParticles = 600,
            Duration = TimeSpan.FromSeconds(10),
            DurationRandomness = 0,
            EmitterVelocitySensitivity = 1,
            MinHorizontalVelocity = 0,
            MaxHorizontalVelocity = 15,
            MinVerticalVelocity = 10,
            MaxVerticalVelocity = 20,
            Gravity = new Vector3(-20, -5, 0),
            EndVelocity = 0.75f,
            MinColor = Color.White,
            MaxColor = Color.White,
            MinRotateSpeed = -1,
            MaxRotateSpeed = 1,
            MinStartSize = 5,
            MaxStartSize = 10,
            MinEndSize = 50,
            MaxEndSize = 200
        };

        public static ParticleSettings ExplosionSettings = new ParticleSettings()
        {
            BlendState = BlendState.Additive,
            TextureName = "Textures/Particles/explosion",
            MaxParticles = 100,
            Duration = TimeSpan.FromSeconds(2),
            DurationRandomness = 1,
            EmitterVelocitySensitivity = 1,
            MinHorizontalVelocity = 20,
            MaxHorizontalVelocity = 30,
            MinVerticalVelocity = -20,
            MaxVerticalVelocity = 20,
            Gravity = new Vector3(0),
            EndVelocity = 0,
            MinColor = new Color(128, 128, 128),
            MaxColor = new Color(169, 169, 169),
            MinRotateSpeed = -1,
            MaxRotateSpeed = 1,
            MinStartSize = 10,
            MaxStartSize = 10,
            MinEndSize = 100,
            MaxEndSize = 200
        };

        public static ParticleSettings ExplosionSmokeSettings = new ParticleSettings()
        {
            BlendState = BlendState.NonPremultiplied,
            TextureName = "Textures/Particles/smoke",
            MaxParticles = 200,
            Duration = TimeSpan.FromSeconds(4),
            DurationRandomness = 0,
            EmitterVelocitySensitivity = 1,
            MinHorizontalVelocity = 0,
            MaxHorizontalVelocity = 40,
            MinVerticalVelocity = 10,
            MaxVerticalVelocity = 50,
            Gravity = new Vector3(0, -20, 0),
            EndVelocity = 0,
            MinColor = new Color(211, 111, 211),
            MaxColor = new Color(255, 255, 255),
            MinRotateSpeed = -2,
            MaxRotateSpeed = 2,
            MinStartSize = 10,
            MaxStartSize = 10,
            MinEndSize = 100,
            MaxEndSize = 200
        };

        public static ParticleSettings FireSettings = new ParticleSettings()
        {
            BlendState = BlendState.Additive,
            TextureName = "Textures/Particles/fire",
            MaxParticles = 2400,
            Duration = TimeSpan.FromSeconds(2),
            DurationRandomness = 1,
            EmitterVelocitySensitivity = 1,
            MinHorizontalVelocity = 0,
            MaxHorizontalVelocity = 15,
            MinVerticalVelocity = -10,
            MaxVerticalVelocity = 10,
            Gravity = new Vector3(0, 15, 0),
            EndVelocity = 1,
            MinColor = new Color(255, 255, 255, 10),
            MaxColor = new Color(255, 255, 255, 40),
            MinRotateSpeed = 0,
            MaxRotateSpeed = 0,
            MinStartSize = 5,
            MaxStartSize = 10,
            MinEndSize = 10,
            MaxEndSize = 40
        };

        public static ParticleSettings ProjectileTrailSettings = new ParticleSettings()
        {
            BlendState = BlendState.NonPremultiplied,
            TextureName = "Textures/Particles/smoke",
            MaxParticles = 1000,
            Duration = TimeSpan.FromSeconds(2),
            DurationRandomness = 1.5f,
            EmitterVelocitySensitivity = 0.1f,
            MinHorizontalVelocity = 0,
            MaxHorizontalVelocity = 1,
            MinVerticalVelocity = -1,
            MaxVerticalVelocity = 1,
            Gravity = new Vector3(0, 0, 0),
            EndVelocity = 1,
            MinColor = new Color(64, 96, 128, 10),
            MaxColor = new Color(255, 255, 255, 128),
            MinRotateSpeed = -4,
            MaxRotateSpeed = 4,
            MinStartSize = 2,
            MaxStartSize = 4,
            MinEndSize = 5,
            MaxEndSize = 15
        };
    }
}
