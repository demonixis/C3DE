using C3DE.Components.Renderers;
using C3DE.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Materials
{
    public enum ShaderQuality
    {
        Low, Normal
    }

    [Serializable]
    public abstract class Material : IDisposable, ISerializable
    {
        protected internal Scene scene;
        protected Vector3 diffuseColor;
        protected Texture2D mainTexture;
        protected internal Effect effect;

        public string Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            set;
        }

        internal int Index
        {
            get { return scene.Materials.IndexOf(this); }
        }

        public Color DiffuseColor
        {
            get { return new Color(diffuseColor); }
            set { diffuseColor = value.ToVector3(); }
        }

        public Texture2D MainTexture
        {
            get { return mainTexture; }
            set { mainTexture = value; }
        }

        public Vector2 Tiling { get; set; }

        public Vector2 Offset { get; set; }

        public ShaderQuality ShaderQuality { get; set; }

        public Material()
        {
            diffuseColor = Color.White.ToVector3();
            Id = "MAT-" + Guid.NewGuid();
            Name = "Material_" + Id;
            Tiling = Vector2.One;
            Offset = Vector2.Zero;
            ShaderQuality = ShaderQuality.Normal;

#if ANDROID || OPENGL
            ShaderQuality = ShaderQuality.Low;
#endif
        }

        public Material(Scene mainScene)
            : this()
        {
            scene = mainScene;
            scene.Add(this);
        }

        public abstract void LoadContent(ContentManager content);

        public abstract void PrePass();

        public abstract void Pass(RenderableComponent renderable);

        public virtual void Dispose() { }

        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            data.Add("Id", Id);
            data.Add("Name", Name);
            data.Add("Type", GetType().FullName);
            data.Add("DiffuseColor", SerializerHelper.ToFloat(diffuseColor));
            data.Add("Tiling", SerializerHelper.ToFloat(Tiling));
            data.Add("Offset", SerializerHelper.ToFloat(Offset));
            
            return data;
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            Id = (string)data["Id"];
            Name = (string)data["Name"];
            DiffuseColor = SerializerHelper.ToColor((float[])data["DiffuseColor"]);
            Tiling = SerializerHelper.ToVector2((float[])data["Tiling"]);
            Offset = SerializerHelper.ToVector2((float[])data["Offset"]);
        }
    }
}
