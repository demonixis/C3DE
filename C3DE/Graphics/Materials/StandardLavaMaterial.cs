﻿using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardLavaMaterial : Material
    {
        public Texture2D NormalMap { get; set; }
        public Texture2D SpecularMap { get; set; }
        public Color SpecularColor { get; set; } = new Color(0.5f, 0.5f, 0.5f);
        public float SpecularIntensity { get; set; } = 1.0f;
        public int SpecularPower { get; set; } = 16;
        public float Speed { get; set; } = 0.25f;

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandardLava(this);
            else
                _shaderMaterial = new ForwardStandardLava(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
