//#define USE_KNUTH_PLASS_LINE_BREAKING

using System;
using System.Collections.Generic;
using Gwen.RichText;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Multiline label with text chunks having different color/font.
	/// </summary>
	[Xml.XmlControl]
	public class RichLabel : ControlBase
	{
		private Document m_Document;
		private bool m_NeedsRebuild;
		private int m_BuildWidth;
		private Size m_TextSize;
		private bool m_Updating;

		public Document Document { get { return m_Document; } set { m_Document = value; m_NeedsRebuild = true; Invalidate(); } }

		[Xml.XmlEvent]
		public event ControlBase.GwenEventHandler<LinkClickedEventArgs> LinkClicked;

		/// <summary>
		/// Initializes a new instance of the <see cref="RichLabel"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public RichLabel(ControlBase parent)
			: base(parent)
		{
			m_BuildWidth = 0;
			m_TextSize = Size.Zero;
			m_Updating = false;
		}

		protected void Rebuild()
		{
			m_Updating = true;

			DeleteAllChildren();

			Size size = Size.Zero;

			if (m_Document != null && m_Document.Paragraphs.Count > 0)
			{
#if USE_KNUTH_PLASS_LINE_BREAKING
				LineBreaker lineBreaker = new RichText.KnuthPlass.LineBreaker(Skin.Renderer, Skin.DefaultFont);
#else
				LineBreaker lineBreaker = new RichText.Simple.LineBreaker(Skin.Renderer, Skin.DefaultFont);
#endif

				int y = 0;
				int x;
				int width;
				int height;

				foreach (Paragraph paragraph in m_Document.Paragraphs)
				{
					if (paragraph is ImageParagraph)
					{
						ImageParagraph imageParagraph = paragraph as ImageParagraph;

						ImagePanel image = new ImagePanel(this);
						image.ImageName = imageParagraph.ImageName;
						if (imageParagraph.ImageSize != null)
							image.Size = (Size)imageParagraph.ImageSize;
						if (imageParagraph.TextureRect != null)
							image.TextureRect = (Rectangle)imageParagraph.TextureRect;
						if (imageParagraph.ImageColor != null)
							image.ImageColor = (Color)imageParagraph.ImageColor;

						image.Measure(Size.Infinity);
						image.Arrange(paragraph.Margin.Left, y + paragraph.Margin.Top, image.MeasuredSize.Width, image.MeasuredSize.Height);

						size.Width = Math.Max(size.Width, image.MeasuredSize.Width + paragraph.Margin.Left + paragraph.Margin.Right);

						y += image.MeasuredSize.Height + paragraph.Margin.Top + paragraph.Margin.Bottom;
					}
					else
					{
						List<TextBlock> textBlocks = lineBreaker.LineBreak(paragraph, m_BuildWidth);

						if (textBlocks != null)
						{
							x = paragraph.Margin.Left;
							y += paragraph.Margin.Top;
							width = 0;
							height = 0;

							foreach (TextBlock textBlock in textBlocks)
							{
								if (textBlock.Part is LinkPart)
								{
									LinkPart linkPart = textBlock.Part as LinkPart;

									LinkLabel link = new LinkLabel(this);
									link.Text = textBlock.Text;
									link.Link = linkPart.Link;
									link.Font = linkPart.Font;
									link.LinkClicked += OnLinkClicked;
									if (linkPart.Color != null)
										link.TextColor = (Color)linkPart.Color;
									if (linkPart.HoverColor != null)
										link.HoverColor = (Color)linkPart.HoverColor;
									if (linkPart.HoverFont != null)
										link.HoverFont = linkPart.HoverFont;

									link.Measure(Size.Infinity);
									link.Arrange(new Rectangle(x + textBlock.Position.X, y + textBlock.Position.Y, textBlock.Size.Width, textBlock.Size.Height));

									width = Math.Max(width, link.ActualRight);
									height = Math.Max(height, link.ActualBottom);
								} else if (textBlock.Part is TextPart)
								{
									TextPart textPart = textBlock.Part as TextPart;

									Text text = new Text(this);
									text.String = textBlock.Text;
									text.Font = textPart.Font;
									if (textPart.Color != null)
										text.TextColor = (Color)textPart.Color;

									text.Measure(Size.Infinity);
									text.Arrange(new Rectangle(x + textBlock.Position.X, y + textBlock.Position.Y, textBlock.Size.Width, textBlock.Size.Height));

									width = Math.Max(width, text.ActualRight + 1);
									height = Math.Max(height, text.ActualBottom + 1);
								}
							}

							size.Width = Math.Max(size.Width, width + paragraph.Margin.Right);

							y = height + paragraph.Margin.Bottom;
						}
					}
				}

				size.Height = y;
			}

			m_TextSize = size;

			m_NeedsRebuild = false;

			m_Updating = false;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			if (m_NeedsRebuild || availableSize.Width != m_BuildWidth)
			{
				m_BuildWidth = availableSize.Width;
				Rebuild();
			}

			return m_TextSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (m_NeedsRebuild || finalSize.Width != m_BuildWidth)
			{
				m_BuildWidth = finalSize.Width;
				Rebuild();
			}

			return m_TextSize;
		}

		private void OnLinkClicked(ControlBase control, LinkClickedEventArgs args)
		{
			if (LinkClicked != null)
				LinkClicked(this, args);
		}

		public override void Invalidate()
		{
			// We don't want to cause the re-layout when creating text objects in the layout
			if (m_Updating)
				return;

			base.Invalidate();
		}
	}
}
