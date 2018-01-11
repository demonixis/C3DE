using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public abstract class Material : IDisposable
    {
        protected internal Scene scene;
        protected internal Vector3 m_DiffuseColor;
        protected internal Effect m_Effect;
        protected internal bool m_hasAlpha;
        protected internal ShaderMaterial m_ShaderMaterial;

        [DataMember]
        public string Id { get; private set; }

        [DataMember]
        public string Name { get; set; }

        internal int Index
        {
            get { return scene.Materials.IndexOf(this); }
        }

        [DataMember]
        public Color DiffuseColor
        {
            get { return new Color(m_DiffuseColor); }
            set { m_DiffuseColor = value.ToVector3(); }
        }

        public Texture2D MainTexture { get;set; }

        [DataMember]
        public Vector2 Tiling { get; set; }

        [DataMember]
        public Vector2 Offset { get; set; }

		public Material()
		{
			m_DiffuseColor = Color.White.ToVector3();
			Id = "MAT-" + Guid.NewGuid();
			Name = "Material_" + Id;
			Tiling = Vector2.One;
			Offset = Vector2.Zero;
			m_hasAlpha = false;
            Application.Engine.RendererChanged += SetupShaderMaterial;
        }

        protected abstract void SetupShaderMaterial(Rendering.BaseRenderer renderer);

        public Material(Scene mainScene)
            : this()
        {
            scene = mainScene;

            if (scene == null)
                scene = Scene.current;

            scene.AddMaterial(this);
        }

        public abstract void LoadContent(ContentManager content);

        public abstract void PrePass(Camera camera);

        public abstract void Pass(Renderer renderable);

        public virtual void Dispose() { }
    }
}
