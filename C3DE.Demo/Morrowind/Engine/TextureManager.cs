using C3DE;
using C3DE.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TES3Unity
{
    /// <summary>
    /// Manages loading and instantiation of Morrowind textures.
    /// </summary>
    public class TextureManager
    {
        private static Dictionary<string, Texture2D> TextureStore = new Dictionary<string, Texture2D>();
        private static Dictionary<Color, Texture2D> MaskTextureStore = new Dictionary<Color, Texture2D>();
        private static Dictionary<Texture2D, Texture2D> NormalMapsStore = new Dictionary<Texture2D, Texture2D>();

        private TES3DataReader _dataReader;

        public TextureManager(TES3DataReader reader)
        {
            _dataReader = reader;
        }

        /// <summary>
        /// Create a mast texture used by HDRP material.
        /// </summary>
        /// <param name="r">Metallic</param>
        /// <param name="g">Occlusion</param>
        /// <param name="b">Detail Mask</param>
        /// <param name="a">Smoothness</param>
        /// <returns>A mask texture.</returns>
        public static Texture2D CreateMaskTexture(float r, float g, float b, float a)
        {
            var color = new Color(r, g, b, a);

            if (MaskTextureStore.ContainsKey(color))
            {
                return MaskTextureStore[color];
            }

            var texture = new Texture2D(Application.GraphicsDevice, 1, 1);
            texture.SetData(new Color[1] { color });

            MaskTextureStore.Add(color, texture);

            return texture;
        }

        // https://gamedev.stackexchange.com/questions/106703/create-a-normal-map-using-a-script-unity
        public static Texture2D CreateNormalMapTexture(Texture2D source, float strength = 0.75f)
        {
            return null;
            if (NormalMapsStore.ContainsKey(source))
            {
                return NormalMapsStore[source];
            }

            strength = Math.Clamp(strength, 0.0F, 100.0f);

            float xLeft;
            float xRight;
            float yUp;
            float yDown;
            float yDelta;
            float xDelta;

            var normalTexture = new Texture2D(source.GraphicsDevice, source.Width, source.Height, true, SurfaceFormat.Color);
            var normalData = new Color[source.Width * source.Height];

            var sourceData = new Color[source.Width * source.Height];
            source.GetData(sourceData);

            for (int y = 0; y < normalTexture.Height; y++)
            {
                for (int x = 0; x < normalTexture.Width; x++)
                {
                    xLeft = GrayScale(sourceData[x - 1 + y * normalTexture.Width]) * strength;
                    xRight = GrayScale(sourceData[x + 1 + y * normalTexture.Width]) * strength;
                    yUp = GrayScale(sourceData[x + y * normalTexture.Width - 1]) * strength;
                    yDown = GrayScale(sourceData[x + y * normalTexture.Width + 1]) * strength;
                    xDelta = ((xLeft - xRight) + 1) * 0.5f;
                    yDelta = ((yUp - yDown) + 1) * 0.5f;
                    normalData[x + y * normalTexture.Width] = new Color(xDelta, yDelta, 1.0f, yDelta);
                }
            }

            byte GrayScale(Color c)
            {
                var cc = c.R + c.G + c.B;
                return (byte)(cc / 3);
            }

            return normalTexture;
        }

        public Texture2D LoadTexture(string texturePath, bool flipVertically = false)
        {
            if (TextureStore.ContainsKey(texturePath))
            {
                return TextureStore[texturePath];
            }

            var texture = _dataReader.LoadTexture(texturePath);

            if (flipVertically)
            {
                FlipTexture2DVertically(texture);
            }

            TextureStore.Add(texturePath, texture);

            return texture;
        }

        public static void FlipTexture2DVertically(Texture2D texture2D)
        {
            var pixels = new byte[texture2D.Height * texture2D.Width];
            texture2D.GetData(pixels);

            ArrayUtils.Flip2DArrayVertically(pixels, texture2D.Height, texture2D.Width);

            texture2D.SetData(pixels);
        }
    }
}