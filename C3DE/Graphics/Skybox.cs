using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics
{
    public enum SkyboxMode
    {
        Cubemap = 0,
        Procedural = 1
    }

    public sealed class ProceduralSkySettings
    {
        public bool Enabled { get; set; }
        public float TimeOfDay { get; set; } = 0.35f;
        public bool AutoCycle { get; set; } = true;
        public float CycleSpeed { get; set; } = 0.01f;
        public float CloudCoverage { get; set; } = 0.55f;
        public float CloudSpeed { get; set; } = 0.015f;
        public float CloudScale { get; set; } = 1.35f;
        public float StarIntensity { get; set; } = 1.0f;
        public float MoonIntensity { get; set; } = 0.4f;
        public float SunIntensity { get; set; } = 1.0f;
        public Color NightTint { get; set; } = new Color(0.45f, 0.5f, 0.7f);
        public Color DayTopColor { get; set; } = new Color(0.22f, 0.52f, 0.95f);
        public Color DayHorizonColor { get; set; } = new Color(0.85f, 0.92f, 1.0f);
        public Color NightTopColor { get; set; } = new Color(0.02f, 0.03f, 0.09f);
        public Color NightHorizonColor { get; set; } = new Color(0.08f, 0.1f, 0.18f);
    }

    public class Skybox
    {
        private ShaderMaterial _shaderMaterial;
        internal Matrix _worldMatrix;
        private Matrix _scaleMatrix;
        private readonly CubeMesh _cubeMesh;
        private TextureCube _environmentMap;
        private TextureCube _irradianceMap;
        private readonly RasterizerState _skyboxRasterizerState;
        private RasterizerState _currentRasterizerState;
        private Vector4 _customFogData;
        private bool _overrideFog;
        private bool _contentLoaded;
        private Texture2D _cloudNoiseTexture;
        private Texture2D _starNoiseTexture;
        private Vector3 _sunSkyDirection;
        private Vector3 _moonSkyDirection;
        private Vector3 _activeLightDirection;
        private float _dayFactor;

        public TextureCube Texture
        {
            get => _environmentMap;
            set
            {
                if (_environmentMap == value)
                    return;

                _environmentMap = value;
                GenerateIrradianceMap();
            }
        }

        public TextureCube IrradianceTexture => _irradianceMap;
        public Matrix WorldMatrix => _worldMatrix;
        public SkyboxMode Mode { get; private set; } = SkyboxMode.Cubemap;
        public ProceduralSkySettings ProceduralSettings { get; } = new ProceduralSkySettings();
        public Texture2D CloudNoiseTexture => _cloudNoiseTexture;
        public Texture2D StarNoiseTexture => _starNoiseTexture;
        public Vector3 SunSkyDirection => _sunSkyDirection;
        public Vector3 MoonSkyDirection => _moonSkyDirection;
        public Vector3 ActiveLightDirection => _activeLightDirection;
        public float DayFactor => _dayFactor;
        public bool FogSupported { get; set; }
        public bool Enabled { get; set; }
        public bool OverrideFog => _overrideFog;
        public Vector4 CustomFogData => _customFogData;

        public Skybox()
        {
            _cubeMesh = new CubeMesh();
            _worldMatrix = Matrix.Identity;
            _scaleMatrix = Matrix.CreateScale(2.0f);
            _skyboxRasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace,
                ScissorTestEnable = false,
                DepthClipEnable = false
            };
        }

        protected void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = Mode == SkyboxMode.Procedural
                ? new ForwardProceduralSkybox(this)
                : new ForwardSkybox(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }

        public void LoadContent(ContentManager content)
        {
            _contentLoaded = true;
            var engine = Application.Engine;
            SetupShaderMaterial(engine.Renderer);
            engine.RendererChanged += SetupShaderMaterial;
        }

        public void OverrideSkyboxFog(FogMode mode, float density, float start, float end)
        {
            _customFogData.X = (float)mode;
            _customFogData.Y = density;
            _customFogData.Z = start;
            _customFogData.W = end;
            _overrideFog = mode != FogMode.None;
        }

        public void GenerateProcedural(float size = 250.0f)
        {
            _cubeMesh.Size = new Vector3(size);
            _cubeMesh.Build();
            _cubeMesh.ComputeNormals();
            Mode = SkyboxMode.Procedural;
            ProceduralSettings.Enabled = true;
            EnsureProceduralTextures();
            Enabled = true;

            if (_contentLoaded)
                SetupShaderMaterial(Application.Engine.Renderer);
        }

        public void UseProcedural(float size = 250.0f)
        {
            GenerateProcedural(size);
        }

        public void Generate(GraphicsDevice device, Texture2D[] textures, float size = 250.0f)
        {
            if (textures.Length != 6)
                throw new Exception("The array of texture names must contains 6 elements.");

            _cubeMesh.Size = new Vector3(size);
            _cubeMesh.Build();
            _cubeMesh.ComputeNormals();
            _environmentMap = TextureFactory.CreateCubeMap(textures);
            Mode = SkyboxMode.Cubemap;
            ProceduralSettings.Enabled = false;

            GenerateIrradianceMap();
            Enabled = true;

            if (_contentLoaded)
                SetupShaderMaterial(Application.Engine.Renderer);
        }

        public void Generate(GraphicsDevice device, string[] textureNames, float size = 250.0f)
        {
            var textures = new Texture2D[6];

            for (var i = 0; i < 6; i++)
                textures[i] = Application.Content.Load<Texture2D>(textureNames[i]);

            Generate(device, textures, size);
        }

        public void Generate(float size = 250.0f)
        {
            var skyTop = TextureFactory.CreateColor(new Color(168, 189, 255), 64, 64);
            var skySide = TextureFactory.CreateGradiant(new Color(168, 189, 255), Color.White, 64, 64);
            var skyBottom = TextureFactory.CreateColor(Color.White, 64, 64);

            Generate(Application.GraphicsDevice, new[]
            {
                skySide,
                skySide,
                skyTop,
                skyBottom,
                skySide,
                skySide
            }, size);
        }

        public void Update(Scene scene)
        {
            if (Mode != SkyboxMode.Procedural || !ProceduralSettings.Enabled)
                return;

            EnsureProceduralTextures();

            if (ProceduralSettings.AutoCycle)
                ProceduralSettings.TimeOfDay = (ProceduralSettings.TimeOfDay + ProceduralSettings.CycleSpeed * Time.DeltaTime) % 1.0f;

            if (ProceduralSettings.TimeOfDay < 0.0f)
                ProceduralSettings.TimeOfDay += 1.0f;

            UpdateCelestials();

            var primaryLight = ResolvePrimaryLight(scene);
            if (primaryLight != null)
                ApplyPrimaryLight(primaryLight);

            ApplyAmbient(scene);
        }

        private void UpdateCelestials()
        {
            var angle = ProceduralSettings.TimeOfDay * MathHelper.TwoPi - MathHelper.PiOver2;
            var orbit = new Vector3(
                (float)Math.Sin(angle) * 0.35f,
                (float)Math.Cos(angle),
                (float)Math.Cos(angle) * 0.85f);

            _sunSkyDirection = Vector3.Normalize(orbit);
            _moonSkyDirection = -_sunSkyDirection;
            _dayFactor = MathHelper.Clamp(MathHelper.SmoothStep(0.0f, 1.0f, (_sunSkyDirection.Y + 0.15f) / 0.55f), 0.0f, 1.0f);

            var activeSkyDirection = Vector3.Normalize(Vector3.Lerp(_moonSkyDirection, _sunSkyDirection, _dayFactor));
            _activeLightDirection = -activeSkyDirection;
        }

        private void ApplyPrimaryLight(Light light)
        {
            var direction = Vector3.Normalize(_activeLightDirection);
            var pitch = -(float)Math.Asin(MathHelper.Clamp(direction.Y, -1.0f, 1.0f));
            var yaw = (float)Math.Atan2(direction.X, direction.Z);

            light.Transform.SetLocalRotation(pitch, yaw, 0.0f);
            light.Color = Color.Lerp(new Color(0.55f, 0.65f, 0.9f), new Color(1.0f, 0.95f, 0.82f), _dayFactor);
            light.Intensity = MathHelper.Lerp(ProceduralSettings.MoonIntensity, ProceduralSettings.SunIntensity, _dayFactor);
        }

        private void ApplyAmbient(Scene scene)
        {
            var top = Color.Lerp(ProceduralSettings.NightTopColor, ProceduralSettings.DayTopColor, _dayFactor).ToVector3();
            var horizon = Color.Lerp(ProceduralSettings.NightHorizonColor, ProceduralSettings.DayHorizonColor, _dayFactor).ToVector3();
            var nightTint = ProceduralSettings.NightTint.ToVector3();
            var ambient = Vector3.Lerp(horizon * nightTint, top, 0.45f) * MathHelper.Lerp(0.18f, 0.35f, _dayFactor);
            scene.RenderSettings.AmbientColor = new Color(ambient);
        }

        private Light ResolvePrimaryLight(Scene scene)
        {
            Light fallback = null;

            for (var i = 0; i < scene._lights.Count; i++)
            {
                var light = scene._lights[i];
                if (!light.Enabled || !light.GameObject.Enabled || light.Type != LightType.Directional)
                    continue;

                if (light.IsSun)
                    return light;

                fallback ??= light;
            }

            return fallback;
        }

        private void EnsureProceduralTextures()
        {
            if (_cloudNoiseTexture == null)
                _cloudNoiseTexture = TextureFactory.CreateNoise(256);

            if (_starNoiseTexture == null)
                _starNoiseTexture = TextureFactory.CreateNoise(512, 256);
        }

        public void GenerateIrradianceMap()
        {
            if (_environmentMap == null)
                return;

            var graphics = Application.GraphicsDevice;
            var renderTargets = graphics.GetRenderTargets();
            var faces = new RenderTarget2D[6];
            var quad = new QuadRenderer(graphics);
            var size = _environmentMap.Size;

            var effect = Application.Content.Load<Effect>("Shaders/IrradianceConvolution");
            effect.Parameters["EnvironmentMap"].SetValue(_environmentMap);
            effect.Parameters["WorldPos"].SetValue(Vector3.Zero);

            for (var i = 0; i < 6; i++)
            {
                faces[i] = new RenderTarget2D(graphics, size, size, true, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
                graphics.SetRenderTarget(faces[i]);
                graphics.Clear(Color.Black);
                effect.CurrentTechnique.Passes[0].Apply();
                quad.RenderFullscreenQuad();
            }

            graphics.SetRenderTargets(renderTargets);

            _irradianceMap = TextureFactory.CreateCubeMap(faces);
        }

        public void Draw(GraphicsDevice device, ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _currentRasterizerState = device.RasterizerState;
            device.RasterizerState = _skyboxRasterizerState;

            _worldMatrix = _scaleMatrix * Matrix.CreateTranslation(cameraPosition);
            _shaderMaterial.PrePass(ref cameraPosition, ref viewMatrix, ref projectionMatrix);

            device.SetVertexBuffer(_cubeMesh.VertexBuffer);
            device.Indices = _cubeMesh.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _cubeMesh.Indices.Length / 3);
            device.RasterizerState = _currentRasterizerState;
        }

        public void DrawNoEffect(GraphicsDevice device)
        {
            _currentRasterizerState = device.RasterizerState;
            device.RasterizerState = _skyboxRasterizerState;
            device.SetVertexBuffer(_cubeMesh.VertexBuffer);
            device.Indices = _cubeMesh.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _cubeMesh.Indices.Length / 3);
            device.RasterizerState = _currentRasterizerState;
        }
    }
}
