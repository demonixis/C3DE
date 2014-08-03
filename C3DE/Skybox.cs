using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE
{
    public class Skybox
    {
        private Matrix _world;
        private CubeGeometry _geometry;
        private TextureCube _texture;
        private Effect _effect;

        public bool Enabled { get; set; }
    
        public Skybox()
        {
            _geometry = new CubeGeometry();
            _world = Matrix.Identity;
        }

        public void Generate(GraphicsDevice device, ContentManager content, string[] textureNames, float size = 250.0f)
        {
            if (textureNames.Length != 6)
                throw new Exception("The array of texture names must contain 6 elements.");

            _effect = content.Load<Effect>("FX/SkyboxEffect");

            _geometry.Size = new Vector3(size);
            _geometry.Generate(device);

            Texture2D[] textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
                textures[i] = content.Load<Texture2D>(textureNames[i]);

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

        public void Draw(GraphicsDevice device, Camera camera)
        {
            _world = Matrix.CreateScale(1) * Matrix.CreateTranslation(camera.SceneObject.Transform.Position);

            _effect.Parameters["World"].SetValue(_world);
            _effect.Parameters["View"].SetValue(camera.view);
            _effect.Parameters["Projection"].SetValue(camera.projection);
            _effect.Parameters["SkyboxTexture"].SetValue(_texture);
            _effect.Parameters["CameraPosition"].SetValue(camera.SceneObject.Transform.Position);
            _effect.CurrentTechnique.Passes[0].Apply();

            device.SetVertexBuffer(_geometry.VertexBuffer);
            device.Indices = _geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _geometry.Vertices.Length, 0, _geometry.Indices.Length / 3);
        }
    }
}
