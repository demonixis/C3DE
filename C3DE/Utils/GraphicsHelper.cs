using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Utils
{
    public class GraphicsHelper
    {
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
    }
}
