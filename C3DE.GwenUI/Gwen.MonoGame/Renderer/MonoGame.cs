using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Gwen.Loader;
using Gwen.Loader.MonoGame;

namespace Gwen.Renderer.MonoGame
{
    public class MonoGameRenderer : RendererBase
	{
		private const int MaxVerts = 4096;

		private GraphicsDevice m_GraphicsDevice;
		private ContentManager m_ContentManager;
		private GwenEffect m_Effect;

		private VertexBuffer m_VertexBuffer;
		private IndexBuffer m_IndexBuffer;

		private readonly VertexPositionColorTexture[] m_VertexArray;
		private readonly short[] m_IndexArray;

		private Color m_Color;

		private readonly Dictionary<Tuple<String, Font>, Texture> m_StringCache;

		private bool m_ClipEnabled;
		private bool m_TextureEnabled;
		private Texture2D m_LastTexture;

		private int m_VertNum;
		private int m_TotalVertNum;
		private int m_DrawCallCount;

		private SpriteFont m_DefaultFont;

		public MonoGameRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager, Effect effect)
		{
			m_GraphicsDevice = graphicsDevice;
			m_ContentManager = contentManager;
			m_Effect = new GwenEffect(effect);

			m_VertexArray = new VertexPositionColorTexture[MaxVerts];
			m_IndexArray = new short[MaxVerts * 3 / 2];

			for (int i = 0; i < MaxVerts / 4; i++)
			{
				/*
                 *   1----2
                 *   |   /|
                 *   |  / |
                 *   | /  |
                 *   |/   |
                 *   0----3
                 */
				// Triangle 1
				m_IndexArray[i * 6 + 0] = (short)(i * 4);
				m_IndexArray[i * 6 + 1] = (short)(i * 4 + 1);
				m_IndexArray[i * 6 + 2] = (short)(i * 4 + 2);
				// Triangle 2
				m_IndexArray[i * 6 + 3] = (short)(i * 4 + 2);
				m_IndexArray[i * 6 + 4] = (short)(i * 4 + 3);
				m_IndexArray[i * 6 + 5] = (short)(i * 4);
			}

			m_VertexBuffer = new VertexBuffer(m_GraphicsDevice, typeof(VertexPositionColorTexture), m_VertexArray.Length, BufferUsage.WriteOnly);

			m_IndexBuffer = new IndexBuffer(m_GraphicsDevice, IndexElementSize.SixteenBits, m_IndexArray.Length, BufferUsage.WriteOnly);
			m_IndexBuffer.SetData<short>(m_IndexArray);

			m_StringCache = new Dictionary<Tuple<String, Font>, Texture>();

			m_DefaultFont = null;
		}

		public override void Dispose()
		{
			FlushTextCache();

			m_VertexBuffer.Dispose();
			m_IndexBuffer.Dispose();

			base.Dispose();
		}

		public int TextCacheSize { get { return m_StringCache.Count; } }

		public int DrawCallCount { get { return m_DrawCallCount; } }

		public int VertexCount { get { return m_TotalVertNum; } }

		public override void Begin()
		{
			m_VertNum = 0;
			m_TotalVertNum = 0;
			m_DrawCallCount = 0;
			m_ClipEnabled = false;
			m_TextureEnabled = false;
			m_LastTexture = null;
		}

		public override void End()
		{
			Flush();
		}

		private void Flush()
		{
			if (m_VertNum == 0)
				return;

			m_Effect.UseTexture = m_TextureEnabled;

			m_VertexBuffer.SetData<VertexPositionColorTexture>(m_VertexArray, 0, m_VertNum);

			m_GraphicsDevice.SetVertexBuffer(m_VertexBuffer);
			m_GraphicsDevice.Indices = m_IndexBuffer;

			foreach (EffectPass pass in m_Effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				m_GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_VertNum / 2);
			}

			m_DrawCallCount++;
			m_TotalVertNum += m_VertNum;
			m_VertNum = 0;
		}

		public void FlushTextCache()
		{
			foreach (var textRenderer in m_StringCache.Values)
			{
				textRenderer.Dispose();
			}
			m_StringCache.Clear();
		}

		public override void DrawFilledRect(Rectangle rect)
		{
			if (m_TextureEnabled)
			{
				Flush();
				m_Effect.UseTexture = false;
				m_TextureEnabled = false;
			}

			rect = Translate(rect);

			DrawRect(rect);
		}

		public override Color DrawColor
		{
			get { return m_Color; }
			set
			{
				m_Color = value;
			}
		}

		public override void StartClip()
		{
			m_ClipEnabled = true;
		}

		public override void EndClip()
		{
			m_ClipEnabled = false;
		}

		public override void DrawTexturedRect(Texture t, Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
		{
			Texture2D tex = t.RendererData as Texture2D;
			if (tex == null)
			{
				DrawMissingImage(rect);
				return;
			}

			rect = Translate(rect);

			bool differentTexture = (tex != m_LastTexture);
			if (!m_TextureEnabled || differentTexture)
			{
				Flush();
			}

			if (!m_TextureEnabled)
			{
				m_Effect.UseTexture = true;
				m_TextureEnabled = true;
			}

			if (differentTexture)
			{
				m_Effect.Texture = tex;
				m_LastTexture = tex;
			}

			DrawRect(rect, u1, v1, u2, v2);
		}

		protected void DrawRect(Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
		{
			if (m_VertNum + 4 >= MaxVerts)
			{
				Flush();
			}

			if (m_ClipEnabled)
			{
				// cpu scissors test

				if (rect.Y < ClipRegion.Y)
				{
					int oldHeight = rect.Height;
					int delta = ClipRegion.Y - rect.Y;
					rect.Y = ClipRegion.Y;
					rect.Height -= delta;

					if (rect.Height <= 0)
					{
						return;
					}

					float dv = (float)delta / (float)oldHeight;

					v1 += dv * (v2 - v1);
				}

				if ((rect.Y + rect.Height) > (ClipRegion.Y + ClipRegion.Height))
				{
					int oldHeight = rect.Height;
					int delta = (rect.Y + rect.Height) - (ClipRegion.Y + ClipRegion.Height);

					rect.Height -= delta;

					if (rect.Height <= 0)
					{
						return;
					}

					float dv = (float)delta / (float)oldHeight;

					v2 -= dv * (v2 - v1);
				}

				if (rect.X < ClipRegion.X)
				{
					int oldWidth = rect.Width;
					int delta = ClipRegion.X - rect.X;
					rect.X = ClipRegion.X;
					rect.Width -= delta;

					if (rect.Width <= 0)
					{
						return;
					}

					float du = (float)delta / (float)oldWidth;

					u1 += du * (u2 - u1);
				}

				if ((rect.X + rect.Width) > (ClipRegion.X + ClipRegion.Width))
				{
					int oldWidth = rect.Width;
					int delta = (rect.X + rect.Width) - (ClipRegion.X + ClipRegion.Width);

					rect.Width -= delta;

					if (rect.Width <= 0)
					{
						return;
					}

					float du = (float)delta / (float)oldWidth;

					u2 -= du * (u2 - u1);
				}
			}

			int vertexIndex = m_VertNum;
			m_VertexArray[vertexIndex].Position.X = rect.X;
			m_VertexArray[vertexIndex].Position.Y = rect.Y;
			m_VertexArray[vertexIndex].TextureCoordinate.X = u1;
			m_VertexArray[vertexIndex].TextureCoordinate.Y = v1;
			m_VertexArray[vertexIndex].Color.R = m_Color.R;
			m_VertexArray[vertexIndex].Color.G = m_Color.G;
			m_VertexArray[vertexIndex].Color.B = m_Color.B;
			m_VertexArray[vertexIndex].Color.A = m_Color.A;

			vertexIndex++;
			m_VertexArray[vertexIndex].Position.X = rect.X + rect.Width;
			m_VertexArray[vertexIndex].Position.Y = rect.Y;
			m_VertexArray[vertexIndex].TextureCoordinate.X = u2;
			m_VertexArray[vertexIndex].TextureCoordinate.Y = v1;
			m_VertexArray[vertexIndex].Color.R = m_Color.R;
			m_VertexArray[vertexIndex].Color.G = m_Color.G;
			m_VertexArray[vertexIndex].Color.B = m_Color.B;
			m_VertexArray[vertexIndex].Color.A = m_Color.A;

			vertexIndex++;
			m_VertexArray[vertexIndex].Position.X = rect.X + rect.Width;
			m_VertexArray[vertexIndex].Position.Y = rect.Y + rect.Height;
			m_VertexArray[vertexIndex].TextureCoordinate.X = u2;
			m_VertexArray[vertexIndex].TextureCoordinate.Y = v2;
			m_VertexArray[vertexIndex].Color.R = m_Color.R;
			m_VertexArray[vertexIndex].Color.G = m_Color.G;
			m_VertexArray[vertexIndex].Color.B = m_Color.B;
			m_VertexArray[vertexIndex].Color.A = m_Color.A;

			vertexIndex++;
			m_VertexArray[vertexIndex].Position.X = rect.X;
			m_VertexArray[vertexIndex].Position.Y = rect.Y + rect.Height;
			m_VertexArray[vertexIndex].TextureCoordinate.X = u1;
			m_VertexArray[vertexIndex].TextureCoordinate.Y = v2;
			m_VertexArray[vertexIndex].Color.R = m_Color.R;
			m_VertexArray[vertexIndex].Color.G = m_Color.G;
			m_VertexArray[vertexIndex].Color.B = m_Color.B;
			m_VertexArray[vertexIndex].Color.A = m_Color.A;

			m_VertNum += 4;
		}

		public override void LoadTextureStream(Texture t, System.IO.Stream data)
		{
			Texture2D tex;
			try
			{
				tex = Texture2D.FromStream(m_GraphicsDevice, data);
			}
			catch (Exception)
			{
				t.Failed = true;
				return;
			}

			t.Width = tex.Width;
			t.Height = tex.Height;
			t.RendererData = tex;
		}

		public override void LoadTexture(Texture t)
		{
			Texture2D tex;
			try
			{
				if (LoaderBase.Loader is MonoGameAssetLoader)
					tex = ((MonoGameAssetLoader)LoaderBase.Loader).LoadTexture(t);
				else
					tex = m_ContentManager.Load<Texture2D>(t.Name);
			}
			catch (Exception)
			{
				t.Failed = true;
				return;
			}

			t.Width = tex.Width;
			t.Height = tex.Height;
			t.RendererData = tex;
		}

		public override void LoadTextureRaw(Texture t, byte[] pixelData)
		{
			Texture2D tex;
			try
			{
				tex = new Texture2D(m_GraphicsDevice, t.Width, t.Height);
				tex.SetData<byte>(pixelData);
			}
			catch (Exception)
			{
				t.Failed = true;
				return;
			}

			t.RendererData = tex;
		}

		public override void FreeTexture(Texture t)
		{
			Texture2D tex = t.RendererData as Texture2D;
			if (tex != null)
			{
				tex.Dispose();
				t.RendererData = null;
			}
		}

		public override Color PixelColor(Texture texture, uint x, uint y, Color defaultColor)
		{
			Texture2D tex = texture.RendererData as Texture2D;
			if (tex == null)
				return defaultColor;

			Color[] pixel = new Color[1];

			tex.GetData<Color>(0, new Microsoft.Xna.Framework.Rectangle((int)x, (int)y, 1, 1), pixel, 0, 1);

			return pixel[0];
		}

		public override bool LoadFont(Font font)
		{
			font.RealSize = (float)Math.Ceiling(font.Size * Scale);

			SpriteFont sysFont;
			try
			{
				if (LoaderBase.Loader is MonoGameAssetLoader)
					sysFont = ((MonoGameAssetLoader)LoaderBase.Loader).LoadFont(font);
				else
					sysFont = m_ContentManager.Load<SpriteFont>(font.FaceName);

				if (m_DefaultFont == null)
					m_DefaultFont = sysFont;
			}
			catch
			{
				sysFont = m_DefaultFont;
			}

			font.RendererData = sysFont;

			return true;
		}

		public override void FreeFont(Font font)
		{
			font.RendererData = null;
		}

		public override FontMetrics GetFontMetrics(Font font)
		{
			if (font.RendererData == null)
				LoadFont(font);

			return new FontMetrics(font);
		}

		public override Size MeasureText(Font font, string text)
		{
			SpriteFont sysFont = font.RendererData as SpriteFont;
			if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
			{
				FreeFont(font);
				LoadFont(font);
				sysFont = font.RendererData as SpriteFont;
			}

			if (sysFont == null)
			{
				return Size.Zero;
			}

			Texture tex;
			var key = new Tuple<String, Font>(text, font);

			if (m_StringCache.TryGetValue(key, out tex))
			{
				return new Size(tex.Width, tex.Height);
			}

			text = text.Replace("\t", "    ");

			Vector2 size = sysFont.MeasureString(text);

			return new Size(Util.Ceil(size.X), Util.Ceil(size.Y));
		}

		public override void RenderText(Font font, Point position, string text)
		{
			Flush();

			SpriteFont sysFont = font.RendererData as SpriteFont;
			if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
			{
				FreeFont(font);
				LoadFont(font);
				sysFont = font.RendererData as SpriteFont;
			}

			if (sysFont == null)
			{
				DrawMissingImage(new Rectangle(position.X, position.Y, 8, 8));
				return;
			}

			Texture tex;
			var key = new Tuple<String, Font>(text, font);
			if (!m_StringCache.TryGetValue(key, out tex))
			{
				Size size = MeasureText(font, text);

				RenderTarget2D target = new RenderTarget2D(m_GraphicsDevice, size.Width, size.Height);
				m_GraphicsDevice.SetRenderTarget(target);
				m_GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

				SpriteBatch spriteBatch = new SpriteBatch(m_GraphicsDevice);
				spriteBatch.Begin();
				spriteBatch.DrawString(sysFont, text, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
				spriteBatch.End();
				spriteBatch.Dispose();

				m_GraphicsDevice.SetRenderTarget(null);

				tex = new Texture(this) { Width = size.Width, Height = size.Height, RendererData = target };
				DrawTexturedRect(tex, new Rectangle(position.X, position.Y, tex.Width, tex.Height));

				m_StringCache[key] = tex;
			}
			else
			{
				DrawTexturedRect(tex, new Rectangle(position.X, position.Y, tex.Width, tex.Height));
			}
		}

		public void Resize(int width, int height)
		{
			var viewport = m_GraphicsDevice.Viewport;

			var projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
			m_Effect.MatrixTransform = projection;
		}
	}
}
