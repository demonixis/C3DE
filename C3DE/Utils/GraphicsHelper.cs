using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Utils
{
    public class GraphicsHelper
    {
        public static Texture2D CreateGradiantTexture(Color start, Color end, int width = 128, int height = 128)
        {
            Texture2D texture = new Texture2D(Application.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[x + y * width] = Color.Lerp(start, end, (float)y / (float)height);
                }
            }

            texture.SetData<Color>(colors);

            return texture;
        }

        public static Texture2D CreateCheckboardTexture(Color firstTile, Color secondTile, int width = 128, int height = 128)
        {
            Texture2D texture = new Texture2D(Application.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];

            int segX = width >> 1;
            int segY = height >> 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((x < segX && y < segY) || (x >= segX && y >= segY))
                        colors[x + y * width] = firstTile;
                    else
                        colors[x + y * width] = secondTile;
                }
            }

            texture.SetData<Color>(colors);

            return texture;
        }

        public static Texture2D CreateCircleTexture(Color circleColor, int radius)
        {
            Texture2D texture = new Texture2D(Application.GraphicsDevice, radius, radius);
            Color[] colors = new Color[radius * radius];

            // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
            float diam = radius / 2f;
            float diamsq = diam * diam;
            int index = 0;
            Vector2 pos = Vector2.Zero;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    index = x * radius + y;
                    pos.X = x - diam;
                    pos.Y = y - diam;

                    if (pos.LengthSquared() <= diamsq)
                        colors[index] = circleColor;
                    else
                        colors[index] = Color.Transparent;
                }
            }

            texture.SetData(colors);

            return texture;
        }

        public static Texture2D CreateBorderTexture(Color borderColor, Color color, int width, int height, int thickness)
        {
            Texture2D texture = new Texture2D(Application.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y == 0 || y < thickness || y == (height - 1) || y > (height - thickness - 1) || (x == 0 || x < thickness || x == (width - 1) || x > (width - thickness - 1)))
                        colors[x + y * width] = borderColor;
                    else
                        colors[x + y * width] = color;
                }
            }

            texture.SetData<Color>(colors);

            return texture;
        }

        public static Color[] CreateColor(Color desiredColor, int width, int height)
        {
            Color[] color = new Color[width * height];
            for (int i = 0; i < color.Length; i++)
                color[i] = desiredColor;
            return color;
        }

        public static Texture2D CreateTexture(Color color, int width, int height)
        {
            Texture2D texture2D = new Texture2D(Application.GraphicsDevice, width, height);
            texture2D.SetData(CreateColor(color, width, height));
            return texture2D;
        }

        public static Texture2D CreateRandomTexture(int width, int height = 0)
        {
            if (height == 0)
                height = width;

            Color[] noisyColors = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    noisyColors[x + y * width] = new Color(new Vector3(RandomHelper.Range(0.0f, 1000.0f) / 1000.0f, 0.0f, 0.0f));
            }

            Texture2D noiseImage = new Texture2D(Application.GraphicsDevice, width, height, false, SurfaceFormat.Color);

            noiseImage.SetData(noisyColors);

            return noiseImage;
        }

        public static TextureCube CreateCubeMap(Texture2D texture)
        {
            var cubeMap = new TextureCube(Application.GraphicsDevice, texture.Width, false, SurfaceFormat.Color);

            Color[] textureData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(textureData);

            for (int i = 0; i < 6; i++)
                cubeMap.SetData<Color>((CubeMapFace)i, textureData);

            return cubeMap;
        }

        /// <summary>
        /// Combine two texture into one.
        /// </summary>
        /// <param name="texture1"></param>
        /// <param name="texture2"></param>
        /// <returns></returns>
        public static Texture2D Combine(Texture2D texture1, Texture2D texture2)
        {
            var renderTarget = new RenderTarget2D(Application.GraphicsDevice, texture1.Width, texture1.Height);
            var tex2Position = new Vector2(texture1.Width / 2 - texture2.Width / 2, texture1.Height / 2 - texture2.Height / 2);
            var spriteBatch = new SpriteBatch(Application.GraphicsDevice);
            var previousRTs = Application.GraphicsDevice.GetRenderTargets();

            Application.GraphicsDevice.SetRenderTarget(renderTarget);
            Application.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(texture1, Vector2.Zero, Color.White);
            spriteBatch.Draw(texture2, tex2Position, Color.White);
            spriteBatch.End();

            Application.GraphicsDevice.SetRenderTargets(previousRTs);

            spriteBatch.Dispose();

            return (Texture2D)renderTarget;
        }
    }
}
