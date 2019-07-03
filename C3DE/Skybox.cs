using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE
{
    [DataContract]
    public class Skybox
    {
        private ShaderMaterial _shaderMaterial;
        private Matrix _worldMatrix;
        private Matrix _scaleMatrix;
        private CubeMesh _cubeMesh;
        private TextureCube _environmentMap;
        private TextureCube _irradianceMap;
        private RasterizerState _skyboxRasterizerState;
        private RasterizerState _currentRasterizerState;
        private Vector4 _customFogData;
        private bool _overrideFog;

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

        [DataMember]
        public bool FogSupported { get; set; } = false;

        [DataMember]
        public bool Enabled { get; set; }

        public bool OverrideFog => _overrideFog;
        public Vector4 CustomFogData => _customFogData;

        public Skybox()
        {
            _cubeMesh = new CubeMesh();
            _worldMatrix = Matrix.Identity;
            _scaleMatrix = Matrix.CreateScale(1.0f);
            _skyboxRasterizerState = new RasterizerState();
            _skyboxRasterizerState.CullMode = CullMode.None;
        }

        protected void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredSkybox(this);
            else
                _shaderMaterial = new ForwardSkybox(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }

        public void LoadContent(ContentManager content)
        {
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

        public void Generate(GraphicsDevice device, Texture2D[] textures, float size = 250.0f)
        {
            if (textures.Length != 6)
                throw new Exception("The array of texture names must contains 6 elements.");

            _cubeMesh.Size = new Vector3(size);
            _cubeMesh.Build();
            _cubeMesh.ComputeNormals();
            _environmentMap = TextureFactory.CreateCubeMap(textures);

            GenerateIrradianceMap();

            Enabled = true;
        }

        public void Generate(GraphicsDevice device, string[] textureNames, float size = 250.0f)
        {
            var textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
                textures[i] = Application.Content.Load<Texture2D>(textureNames[i]);

            Generate(device, textures, size);
        }

        public void Generate(float size = 250.0f)
        {
            var skyTop = TextureFactory.CreateColor(new Color(168, 189, 255), 64, 64);
            var skySide = TextureFactory.CreateGradiant(new Color(168, 189, 255), Color.White, 64, 64);
            var skyBottom = TextureFactory.CreateColor(Color.White, 64, 64);

            Generate(Application.GraphicsDevice, new [] 
            {
                skySide,
                skySide,
                skyTop,
                skyBottom,
                skySide,
                skySide
            }, size);
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

        public void Draw(GraphicsDevice device, Camera camera)
        {
            _currentRasterizerState = device.RasterizerState;
            device.RasterizerState = _skyboxRasterizerState;

            _worldMatrix = _scaleMatrix * Matrix.CreateTranslation(camera.Transform.LocalPosition);
            _shaderMaterial.PrePass(camera);

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
