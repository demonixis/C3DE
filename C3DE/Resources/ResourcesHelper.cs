#define DIRECTX
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;

namespace C3DE.Resources
{
    public class ResourcesHelper
    {
        public static Assembly GetAssembly()
        {
#if NETFX_CORE
            return typeof(ResourcesHelper).GetTypeInfo().Assembly;
#else
            return Assembly.GetExecutingAssembly();
#endif
        }

        public static Effect LoadEffect(string name, string path = "C3DE.Resources.Shaders")
        {
            string suffix = "ogl";
#if DIRECTX
            suffix = "dx11";
#endif
            var stream = GetAssembly().GetManifestResourceStream(String.Format("{0}.{1}.{2}.mgfxo", path, name, suffix));
            byte[] shaderCode;

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                shaderCode = ms.ToArray();
            }

            return new Effect(Application.GraphicsDevice, shaderCode);
        }

        public static Texture2D LoadTexture(string name, string path = "C3DE.Resources.Images")
        {
#if ANDROID
            return null;
#else
            var stream = GetAssembly().GetManifestResourceStream(string.Concat("{0}.{1}", path, name));

            return Texture2D.FromStream(Application.GraphicsDevice, stream);
#endif
        }
    }
}
