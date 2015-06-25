using C3DE.Components;
using C3DE.Geometries;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE
{
    public class Skybox
    {
        private static Effect SkyboxEffect = null;
        private Matrix _world;
        private CubeGeometry _geometry;
        private TextureCube _texture;
        private RasterizerState _skyboxRasterizerState;
        private RasterizerState _currentRasterizerState;

        public TextureCube Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        public bool Enabled { get; set; }

        public Skybox()
        {
            _geometry = new CubeGeometry();
            _world = Matrix.Identity;
            _skyboxRasterizerState = new RasterizerState();
            _skyboxRasterizerState.CullMode = CullMode.None;
        }

        public void Generate(GraphicsDevice device, ContentManager content, Texture2D[] textures, float size = 250.0f)
        {
            if (textures.Length != 6)
                throw new Exception("The array of texture names must contains 6 elements.");

            if (SkyboxEffect == null)
                SkyboxEffect = content.Load<Effect>("FX/SkyboxEffect");

            _geometry.Size = new Vector3(size);
            _geometry.Generate();

            _texture = new TextureCube(device, textures[0].Width, false, SurfaceFormat.Color);
            Color[] textureData;

            for (int i = 0; i < 6; i++)
            {
                textureData = new Color[textures[i].Width * textures[i].Height];
                textures[i].GetData<Color>(textureData);
                _texture.SetData<Color>((CubeMapFace)i, textureData);
            }

            Enabled = true;
        }

        public void Generate(GraphicsDevice device, ContentManager content, string[] textureNames, float size = 250.0f)
        {
            Texture2D[] textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
                textures[i] = content.Load<Texture2D>(textureNames[i]);

            Generate(device, content, textures, size);
        }

        public void Generate()
        {
            var skyTop = GraphicsHelper.CreateTexture(new Color(168, 189, 255), 64, 64);
            var skySide = GraphicsHelper.CreateGradiantTexture(new Color(168, 189, 255), Color.White, 64, 64);
            var skyBottom = GraphicsHelper.CreateTexture(Color.White, 64, 64);

            Generate(Application.GraphicsDevice, Application.Content, new Texture2D[] {    
                skySide,
                skySide,
                skyTop,
                skyBottom,
                skySide,
                skySide
            }, 1000.0f);
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            _currentRasterizerState = device.RasterizerState;
            device.RasterizerState = _skyboxRasterizerState;

            _world = Matrix.CreateScale(1) * Matrix.CreateTranslation(camera.SceneObject.Transform.Position);

            SkyboxEffect.Parameters["World"].SetValue(_world);
            SkyboxEffect.Parameters["View"].SetValue(camera.view);
            SkyboxEffect.Parameters["Projection"].SetValue(camera.projection);
            SkyboxEffect.Parameters["SkyboxTexture"].SetValue(_texture);
            SkyboxEffect.Parameters["CameraPosition"].SetValue(camera.SceneObject.Transform.Position);
            SkyboxEffect.CurrentTechnique.Passes[0].Apply();

            device.SetVertexBuffer(_geometry.VertexBuffer);
            device.Indices = _geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _geometry.Vertices.Length, 0, _geometry.Indices.Length / 3);

            device.RasterizerState = _currentRasterizerState;
        }
    }
}
