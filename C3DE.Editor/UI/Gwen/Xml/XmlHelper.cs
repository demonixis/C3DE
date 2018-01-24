using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gwen.Xml
{
	public class XmlHelper
	{
		internal static void RegisterDefaultHandlers()
		{
			foreach (var converter in m_AttributeValueConverters)
				Parser.RegisterAttributeValueConverter(converter.Key, converter.Value);

			foreach (var converter in m_EventHandlerConverters)
				Parser.RegisterEventHandlerConverter(converter.Key, converter.Value);

			Component.Register<CommonDialog.FileDialog>();
			Component.Register<CommonDialog.OpenFileDialog>();
			Component.Register<CommonDialog.SaveFileDialog>();
			Component.Register<CommonDialog.FolderBrowserDialog>();
		}

		public static Int32[] ParseArrayInt32(string valueStr)
		{
			string[] values = valueStr.Split(Parser.ArraySeparator);
			Int32[] newValues = new Int32[values.Length];
			int index = 0;
			foreach (string value in values)
			{
				if (!Int32.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out newValues[index++]))
					return null;
			}

			return newValues;
		}

		private static Dictionary<Type, EventHandlerConverter> m_EventHandlerConverters = new Dictionary<Type, EventHandlerConverter>
		{
			{ typeof(EventArgs), (attribute, value) =>
				{
					return new Control.ControlBase.GwenEventHandler<EventArgs>(new XmlEventHandler<EventArgs>(value, attribute).OnEvent);
				}
			},
			{ typeof(Control.ClickedEventArgs), (attribute, value) =>
				{
					return new Control.ControlBase.GwenEventHandler<Control.ClickedEventArgs>(new XmlEventHandler<Control.ClickedEventArgs>(value, attribute).OnEvent);
				}
			},
			{ typeof(Control.ItemSelectedEventArgs), (attribute, value) =>
				{
					return new Control.ControlBase.GwenEventHandler<Control.ItemSelectedEventArgs>(new XmlEventHandler<Control.ItemSelectedEventArgs>(value, attribute).OnEvent);
				}
			},
			{ typeof(Control.LinkClickedEventArgs), (attribute, value) =>
				{
					return new Control.ControlBase.GwenEventHandler<Control.LinkClickedEventArgs>(new XmlEventHandler<Control.LinkClickedEventArgs>(value, attribute).OnEvent);
				}
			},
		};

		private static Dictionary<Type, AttributeValueConverter> m_AttributeValueConverters = new Dictionary<Type, AttributeValueConverter>
		{
			{ typeof(bool), (element, value) =>
				{
					bool result;
					if (bool.TryParse(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as boolean failed.");
				}
			},
			{ typeof(int), (element, value) =>
				{
					int result;
					if (Int32.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as integer failed.");
				}
			},
			{ typeof(uint), (element, value) =>
				{
					uint result;
					if (UInt32.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as unsigned integer failed.");
				}
			},
			{ typeof(long), (element, value) =>
				{
					long result;
					if (Int64.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as long failed.");
				}
			},
			{ typeof(ulong), (element, value) =>
				{
					ulong result;
					if (UInt64.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as unsigned long failed.");
				}
			},
			{ typeof(float), (element, value) =>
				{
					float result;
					if (Single.TryParse(value, NumberStyles.Float, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as single failed.");
				}
			},
			{ typeof(double), (element, value) =>
				{
					double result;
					if (Double.TryParse(value, NumberStyles.Float, Parser.NumberFormatInfo, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as double failed.");
				}
			},
			{ typeof(string), (element, value) =>
				{
					return value;
				}
			},
			{ typeof(char), (element, value) =>
				{
					if (value.Length == 1)
						return value[0];
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as char failed.");
				}
			},
			{ typeof(object), (element, value) =>
				{
					if (value.IndexOf(Parser.NumberFormatInfo.NumberDecimalSeparator) >= 0)
					{
						float valueFloat;
						if (Single.TryParse(value, NumberStyles.Float, Parser.NumberFormatInfo, out valueFloat))
						{
							return valueFloat;
						}
					}

					int valueInt32;
					if (Int32.TryParse(value, NumberStyles.Integer, Parser.NumberFormatInfo, out valueInt32))
					{
						return valueInt32;
					}

					return value;
 				}
			},
			{ typeof(Size), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 2)
							return new Size(values[0], values[1]);
					}

					throw new System.Xml.XmlException("Attribute value error. Parsing the value as size failed.");
				}
			},
			{ typeof(Point), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 2)
							return new Point(values[0], values[1]);
					}

					throw new System.Xml.XmlException("Attribute value error. Parsing the value as point failed.");
				}
			},
			{ typeof(Margin), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 1)
							return new Margin(values[0], values[0], values[0], values[0]);
						else if (values.Length == 2)
							return new Margin(values[0], values[1], values[0], values[1]);
						else if (values.Length == 4)
							return new Margin(values[0], values[1], values[2], values[3]);
					}

					throw new System.Xml.XmlException("Attribute value error. Parsing the value as margin failed.");
				}
			},
			{ typeof(Padding), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 1)
							return new Padding(values[0], values[0], values[0], values[0]);
						else if (values.Length == 2)
							return new Padding(values[0], values[1], values[0], values[1]);
						else if (values.Length == 4)
							return new Padding(values[0], values[1], values[2], values[3]);
					}

					throw new System.Xml.XmlException("Attribute value error. Parsing the value as padding failed.");
				}
			},
			{ typeof(Anchor), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 2)
							return new Anchor((byte)values[0], (byte)values[1], (byte)values[0], (byte)values[1]);
						else if (values.Length == 4)
							return new Anchor((byte)values[0], (byte)values[1], (byte)values[2], (byte)values[3]);
					}
					throw new System.Xml.XmlException("Attribute value error. Parsing the value as anchor failed.");
				}
			},
			{ typeof(Rectangle), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 4)
							return new Rectangle(values[0], values[1], values[2], values[3]);
					}
					throw new System.Xml.XmlException("Attribute value error. Parsing the value as rectangle failed.");
				}
			},
			{ typeof(Color), (element, value) =>
				{
					int[] values = ParseArrayInt32(value);
					if (values != null)
					{
						if (values.Length == 4)
							return new Color(values[0], values[1], values[2], values[3]);
						else if (values.Length == 3)
							return new Color(values[0], values[1], values[2]);
						else
							throw new System.Xml.XmlException("Attribute value error. Parsing the value as color failed.");
					}
					else
					{
						value = value.Trim();

						string hex = null;
						if (value.StartsWith("0x") || value.StartsWith("0X"))
							hex = value.Substring(2);
						else if (value.StartsWith("#"))
							hex = value.Substring(1);

						if (hex != null)
						{
							uint color;
							if (UInt32.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
								return new Color(color);
							else
								throw new System.Xml.XmlException("Attribute value error. Parsing the value as color failed.");
						}
						else
						{
							return Color.FromName(value);
						}
					}
				}
			},
			{ typeof(Alignment), (element, value) =>
				{
					Alignment result;
					if (Enum.TryParse<Alignment>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as alignment failed.");
				}
			},
			{ typeof(HorizontalAlignment), (element, value) =>
				{
					HorizontalAlignment result;
					if (Enum.TryParse<HorizontalAlignment>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as horizontal alignment failed.");
				}
			},
			{ typeof(VerticalAlignment), (element, value) =>
				{
					VerticalAlignment result;
					if (Enum.TryParse<VerticalAlignment>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as vertical alignment failed.");
				}
			},
			{ typeof(Dock), (element, value) =>
				{
					Dock result;
					if (Enum.TryParse<Dock>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as dock failed.");
				}
			},
			{ typeof(Control.ImageAlign), (element, value) =>
				{
					Control.ImageAlign result;
					if (Enum.TryParse<Control.ImageAlign>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as image alignment failed.");
				}
			},
			{ typeof(Control.BorderType), (element, value) =>
				{
					Control.BorderType result;
					if (Enum.TryParse<Control.BorderType>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as border type failed.");
				}
			},
			{ typeof(Control.StartPosition), (element, value) =>
				{
					Control.StartPosition result;
					if (Enum.TryParse<Control.StartPosition>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as start position failed.");
				}
			},
			{ typeof(Control.Resizing), (element, value) =>
				{
					Control.Resizing result;
					if (Enum.TryParse<Control.Resizing>(value, out result))
						return result;
					else
						throw new System.Xml.XmlException("Attribute value error. Parsing the value as resizing failed.");
				}
			},
			{ typeof(Font), (element, value) =>
				{
					string name;
					int size = 10;
					FontStyle style = FontStyle.Normal;

					string[] fontValues = value.Split(m_fontValueSeparator);
					if (fontValues.Length == 0)
						throw new System.Xml.XmlException("Attribute value error. No font face name specified.");

					name = fontValues[0].Trim();

					if (fontValues.Length >= 2)
					{
						if (!Int32.TryParse(fontValues[1], out size))
							throw new System.Xml.XmlException("Attribute value error. Font size parsing failed.");

						for (int i = 2; i < fontValues.Length; i++)
						{
							switch(fontValues[i].Trim().ToLower())
							{
								case "bold":
									style |= FontStyle.Bold;
									break;
								case "italic":
									style |= FontStyle.Italic;
									break;
								case "underline":
									style |= FontStyle.Underline;
									break;
								case "strikeout":
									style |= FontStyle.Strikeout;
									break;
								default:
									throw new System.Xml.XmlException("Attribute value error. Unknown font style.");
							}
						}
					}

					if (element is Gwen.Control.ControlBase)
						return Font.Create(name, size, style);
					else if (element is Gwen.Xml.Component)
						return Font.Create(name, size, style);
					else
						throw new Exception("Can't create a font. The renderer is unknown.");
				}
			},
			{ typeof(Control.Layout.GridCellSizes), (element, value) =>
				{
					string[] values = value.Split(Parser.ArraySeparator);
					float[] sizes = new float[values.Length];
					int index = 0;
					foreach (string val in values)
					{
						string cellSize = val.Trim();
						if (cellSize.ToLower() == "auto")
						{
							sizes[index++] = Control.Layout.GridLayout.AutoSize;
						}
						else if (cellSize.IndexOf('%') > 0)
						{
							float v;
							if (Single.TryParse(cellSize.Substring(0, cellSize.IndexOf('%')), out v))
								sizes[index++] = v / 100.0f;
							else
								throw new System.Xml.XmlException("Attribute value error. Parsing the value as cell size failed.");
						}
						else
						{
							float v;
							if (Single.TryParse(cellSize, out v))
								sizes[index++] = v;
							else
								throw new System.Xml.XmlException("Attribute value error. Parsing the value as cell size failed.");
						}
					}

					return new Control.Layout.GridCellSizes(sizes);
				}
			},
		};

		private static char[] m_fontValueSeparator = { ';' };
	}
}
