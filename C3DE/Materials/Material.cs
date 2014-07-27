using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class Material
    {
        private static int MaterialCounter = 0;

        private Scene _scene;
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
            get { return _scene.Materials.IndexOf(this); }
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

        public Material(Scene scene)
        {
            diffuseColor = Color.White;
            emissiveColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            mainTexture = null;
            effect = null;
            _scene = scene;
            _scene.Add(this);
            Id = MaterialCounter++;
            Name = "Material_" + Id;
        }

        public void Dispose()
        {
            _scene = null;
            mainTexture.Dispose();
            effect.Dispose();
        }
    }
}
