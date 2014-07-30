using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public abstract class Material
    {
        private static int MaterialCounter = 0;

        protected Scene scene;
        protected Color diffuseColor;
        protected Texture2D mainTexture;
        protected Color emissiveColor;
        protected Effect effect;

        public int Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            protected set;
        }

        internal int Index
        {
            get { return scene.Materials.IndexOf(this); }
        }

        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        public Texture2D MainTexture
        {
            get { return mainTexture; }
            set { mainTexture = value; }
        }

        public Color EmissiveColor
        {
            get { return emissiveColor; }
            set { emissiveColor = value; }
        }

        public Material(Scene mainScene)
        {
            diffuseColor = Color.White;
            emissiveColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            scene = mainScene;
            scene.Add(this);
            Id = MaterialCounter++;
            Name = "Material_" + Id;
        }

        public abstract void LoadContent(ContentManager content);

        public abstract void PrePass();

        public abstract void Pass(Transform transform);

        public void Dispose()
        {
            scene = null;
            mainTexture.Dispose();
            effect.Dispose();
        }
    }
}
