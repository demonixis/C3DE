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
        protected internal Vector3 m_DiffuseColor;
        protected internal bool m_hasAlpha;
        protected internal ShaderMaterial m_ShaderMaterial;

        [DataMember]
        public string Id { get; private set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Color DiffuseColor
        {
            get { return new Color(m_DiffuseColor); }
            set { m_DiffuseColor = value.ToVector3(); }
        }

        public Texture2D MainTexture { get; set; }

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
            Scene.current?.AddMaterial(this);
        }

        public Material(string name) 
            : this()
        {
            Name = name;
        }

        public virtual void LoadContent(ContentManager content)
        {
            var engine = Application.Engine;
            SetupShaderMaterial(engine.Renderer);
            engine.RendererChanged += SetupShaderMaterial;
        }

        protected abstract void SetupShaderMaterial(Rendering.BaseRenderer renderer);

        public virtual void Dispose() { }
    }
}
