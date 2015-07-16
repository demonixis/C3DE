using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace C3DE
{
    public static class Asset
    {
        public static Texture2D LoadTexture(string path, bool isExternal = false)
        {
            Texture2D texture = null;

            if (!isExternal)
                texture = Application.Content.Load<Texture2D>(path);
            else
                texture = Texture2D.FromStream(Application.GraphicsDevice, File.Open(path, FileMode.Open));

            if (texture != null)
                texture.Tag = String.Format("{0}|{1}", path, isExternal);

            return texture;
        }

        public static Model LoadModel(string path)
        {
            var model = Application.Content.Load<Model>(path);

            if (model != null)
                model.Tag = path;

            return model;
        }
    }
}
