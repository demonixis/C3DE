using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.Materials
{
    public abstract class Material : IDisposable
    {
        protected internal Vector3 _diffuseColor;
        protected internal bool _hasAlpha;
        protected internal ShaderMaterial _shaderMaterial;

        public string Name { get; set; }

        public Color DiffuseColor
        {
            get => new Color(_diffuseColor);
            set => _diffuseColor = value.ToVector3();
        }

        public Texture2D MainTexture { get; set; }

        public Vector2 Tiling { get; set; }

        public Vector2 Offset { get; set; }

        public Material()
        {
            Name = $"Material_{Guid.NewGuid()}";
            Tiling = Vector2.One;
            Offset = Vector2.Zero;
            _diffuseColor = Color.White.ToVector3();
            _hasAlpha = false;
            Scene.current?.AddMaterial(this);
        }

        public virtual void LoadContent(ContentManager content)
        {
            var engine = Application.Engine;
            SetupShaderMaterial(engine.Renderer);
            engine.RendererChanged += SetupShaderMaterial;
        }

        protected abstract void SetupShaderMaterial(BaseRenderer renderer);

        public virtual void Dispose() { }
    }
}
