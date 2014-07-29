using C3DE.Components.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE
{
    public class Skybox
    {
        private VertexBuffer[] _vertexBuffers;
        private IndexBuffer[] _indexBuffers;
        private Texture2D[] _textures;
        private BasicEffect _skyboxFX;
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        public Skybox(GraphicsDevice device)
        {
            _skyboxFX = new BasicEffect(device);
            _view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Zero, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.Width / device.Viewport.Height, 1.0f, 100.0f);
            _world = Matrix.Identity;

            GenerateSkybox(device);
        }

        private void GenerateSkybox(GraphicsDevice device)
        {
            var size = 50;

            Vector3 topLeftFront = new Vector3(-1.0f * size, 1.0f * size, 1.0f * size);
            Vector3 bottomLeftFront = new Vector3(-1.0f * size, -1.0f * size, 1.0f * size);
            Vector3 topRightFront = new Vector3(1.0f * size, 1.0f * size, 1.0f * size);
            Vector3 bottomRightFront = new Vector3(1.0f * size, -1.0f * size, 1.0f * size);
            Vector3 topLeftBack = new Vector3(-1.0f * size, 1.0f * size, -1.0f * size);
            Vector3 topRightBack = new Vector3(1.0f * size, 1.0f * size, -1.0f * size);
            Vector3 bottomLeftBack = new Vector3(-1.0f * size, -1.0f * size, -1.0f * size);
            Vector3 bottomRightBack = new Vector3(1.0f * size, -1.0f * size, -1.0f * size);

            //Texture Positions
            Vector2 textureCoordinates0 = new Vector2(0.0f, 0.0f);
            Vector2 textureCoordinates1 = new Vector2(1.0f, 0.0f);
            Vector2 textureCoordinates2 = new Vector2(0.0f, 1.0f);
            Vector2 textureCoordinates3 = new Vector2(1.0f, 1.0f);


            VertexPositionTexture[][] vertices = new VertexPositionTexture[6][];

            vertices[0] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(bottomLeftFront, textureCoordinates0),
                    new VertexPositionTexture(topLeftFront ,textureCoordinates1), 
                    new VertexPositionTexture(bottomRightFront,textureCoordinates2),
                    new VertexPositionTexture(topRightFront,textureCoordinates3),  
                };

            vertices[1] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(bottomRightBack,textureCoordinates0),
                    new VertexPositionTexture(topRightBack,textureCoordinates1), 
                    new VertexPositionTexture(bottomLeftBack,textureCoordinates2),
                    new VertexPositionTexture(topLeftBack,textureCoordinates3), 
                };

            vertices[2] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(bottomLeftBack,textureCoordinates0),
                    new VertexPositionTexture(topLeftBack,textureCoordinates1),
                    new VertexPositionTexture(bottomLeftFront,textureCoordinates2),
                    new VertexPositionTexture(topLeftFront,textureCoordinates3),
                };

            vertices[3] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(bottomRightFront,textureCoordinates0),
                    new VertexPositionTexture(topRightFront,textureCoordinates1),
                    new VertexPositionTexture(bottomRightBack,textureCoordinates2),
                    new VertexPositionTexture(topRightBack,textureCoordinates3),
                };

            vertices[4] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(topLeftFront,textureCoordinates0),
                    new VertexPositionTexture(topLeftBack,textureCoordinates1),
                    new VertexPositionTexture(topRightFront,textureCoordinates2),
                    new VertexPositionTexture(topRightBack,textureCoordinates3),
                };

            vertices[5] = new VertexPositionTexture[4]
                {
                    new VertexPositionTexture(bottomLeftBack,textureCoordinates0),
                    new VertexPositionTexture(bottomLeftFront,textureCoordinates1),
                    new VertexPositionTexture(bottomRightBack,textureCoordinates2),
                    new VertexPositionTexture(bottomRightFront,textureCoordinates3)
                };

            short[][] indices = new short[6][]; 
            indices[0] = new short[] { 0, 1, 2, 2, 1, 3 }; 
            indices[1] = new short[] { 4, 5, 6, 6, 5, 7 };
            indices[2] = new short[] { 8, 9, 10, 10, 9, 11 }; 
            indices[3] = new short[] { 12, 13, 14, 14, 13, 15 }; 
            indices[4] = new short[] { 16, 17, 18, 18, 17, 19 };
            indices[5] = new short[] { 20, 21, 22, 22, 21, 23 };

            _vertexBuffers = new VertexBuffer[6];
            _indexBuffers = new IndexBuffer[6];

            for (int i = 0; i < 6; i++)
            {
                _vertexBuffers[i] = new VertexBuffer(device, typeof(VertexPositionTexture), 4, BufferUsage.None);
                _vertexBuffers[i].SetData(vertices[i]);

                _indexBuffers[i] = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
                _indexBuffers[i].SetData(indices[i]);
            }
        }

        public void LoadContent(string[] textures, ContentManager content)
        {
            if (textures.Length == 6)
            {
                _textures = new Texture2D[6];

                for (int i = 0; i < 6; i++)
                    _textures[i] = content.Load<Texture2D>("Textures/Skybox/" + textures[i]);
            }
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            _skyboxFX.World = Matrix.Identity;
            _skyboxFX.View = camera.view;
            _skyboxFX.Projection = camera.projection;
            _skyboxFX.TextureEnabled = true;
            _skyboxFX.LightingEnabled = false;
            _skyboxFX.EnableDefaultLighting();

            for (var i = 0; i < 6; i++)
            {
                _skyboxFX.Texture = _textures[i];
                device.SetVertexBuffer(_vertexBuffers[i]);
                device.Indices = _indexBuffers[i];
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
        }
    }
}
