using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Editor.Core.Components
{
    public class EditorIconRenderer : MeshRenderer
    {
        private static Dictionary<string, UnlitMaterial> s_Materials = new Dictionary<string, UnlitMaterial>();

        public void Setup(string icon)
        {
            CastShadow = false;
            ReceiveShadow = false;
            m_Geometry = new QuadMesh();
            m_Geometry.Size = new Vector3(0.5f);
            m_Geometry.TextureRepeat = new Vector2(2);
            m_Geometry.Build();

            boundingBox = new BoundingBox(new Vector3(-0.25f), new Vector3(0.25f));

            UnlitMaterial iconMaterial = null;

            if (s_Materials.ContainsKey(icon))
            {
                iconMaterial = s_Materials[icon];

                // Mat is null if the scene was changed.
                if (iconMaterial == null)
                {
                    iconMaterial = CreateMaterial(icon);
                    s_Materials[icon] = iconMaterial;
                }
            }
            else
            {
                iconMaterial = CreateMaterial(icon);
                s_Materials.Add(icon, iconMaterial);
            }

            material = iconMaterial;
        }

        private UnlitMaterial CreateMaterial(string icon)
        {
            var iconMaterial = new UnlitMaterial(icon);
            iconMaterial.MainTexture = Application.Content.Load<Texture2D>($"Icons/{icon}");
            return iconMaterial;
        }
    }
}
