using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gwen.Renderer.MonoGame
{
    public class GwenEffect : Effect
	{
		protected GwenEffect(GwenEffect clone)
            : base(clone)
        {
			CacheEffectParams();
		}

		public GwenEffect(Effect cloneSource)
            : base(cloneSource)
        {
			CacheEffectParams();
		}

		private void CacheEffectParams()
		{
			m_MatrixTransformParam = Parameters["MatrixTransform"];
			m_TextureParam = Parameters["Texture"];
			m_UseTextureParam = Parameters["UseTexture"];
		}

		public Matrix MatrixTransform { get { return m_MatrixTransformParam.GetValueMatrix(); ; } set { m_MatrixTransformParam.SetValue(value); } }
		public Texture2D Texture { get { return m_TextureParam.GetValueTexture2D(); } set { m_TextureParam.SetValue(value); } }
		public bool UseTexture { get { return m_UseTextureParam.GetValueBoolean(); } set { m_UseTextureParam.SetValue(value); } }

		private EffectParameter m_MatrixTransformParam;
		private EffectParameter m_TextureParam;
		private EffectParameter m_UseTextureParam;
	}
}
