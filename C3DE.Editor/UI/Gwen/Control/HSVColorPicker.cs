using System;
using Gwen.Control.Internal;
using Gwen.Control.Layout;

namespace Gwen.Control
{
	/// <summary>
	/// HSV color picker with "before" and "after" color boxes.
	/// </summary>
	[Xml.XmlControl(ElementName = "ColorPicker")]
	public class HSVColorPicker : ControlBase, IColorPicker
	{
		private readonly ColorLerpBox m_LerpBox;
		private readonly ColorSlider m_ColorSlider;
		private readonly ColorDisplay m_Before;
		private readonly ColorDisplay m_After;
		private readonly NumericUpDown m_Red;
		private readonly NumericUpDown m_Green;
		private readonly NumericUpDown m_Blue;

		private bool m_enableDefaultColor;

		/// <summary>
		/// Invoked when the selected color has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> ColorChanged;

		/// <summary>
		/// The "before" color.
		/// </summary>
		[Xml.XmlProperty]
		public Color DefaultColor { get { return m_Before.Color; } set { m_Before.Color = value; } }

		/// <summary>
		/// Selected color.
		/// </summary>
		public Color SelectedColor { get { return m_LerpBox.SelectedColor; } set { SetColor(value); } }

		/// <summary>
		/// Show / hide default color box
		/// </summary>
		[Xml.XmlProperty]
		public bool EnableDefaultColor { get { return m_enableDefaultColor; } set { m_enableDefaultColor = value; UpdateChildControlVisibility(); } }
		/// <summary>
		/// Initializes a new instance of the <see cref="HSVColorPicker"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public HSVColorPicker(ControlBase parent)
			: base(parent)
		{
			MouseInputEnabled = true;

			int baseSize = BaseUnit;

			m_LerpBox = new ColorLerpBox(this);
			m_LerpBox.Margin = Margin.Two;
			m_LerpBox.ColorChanged += ColorBoxChanged;
			m_LerpBox.Dock = Dock.Fill;

			ControlBase values = new VerticalLayout(this);
			values.Dock = Dock.Right;
			{
				m_After = new ColorDisplay(values);
				m_After.Size = new Size(baseSize * 5, baseSize * 2);

				m_Before = new ColorDisplay(values);
				m_Before.Margin = new Margin(2, 0, 2, 2);
				m_Before.Size = new Size(baseSize * 5, baseSize * 2);

				GridLayout grid = new GridLayout(values);
				grid.Margin = new Margin(2, 0, 2, 2);
				grid.SetColumnWidths(GridLayout.AutoSize, GridLayout.Fill);
				{
					{
						Label label = new Label(grid);
						label.Text = "R: ";
						label.Alignment = Alignment.Left | Alignment.CenterV;

						m_Red = new NumericUpDown(grid);
						m_Red.Min = 0;
						m_Red.Max = 255;
						m_Red.SelectAllOnFocus = true;
						m_Red.ValueChanged += NumericTyped;
					}

					{
						Label label = new Label(grid);
						label.Text = "G: ";
						label.Alignment = Alignment.Left | Alignment.CenterV;

						m_Green = new NumericUpDown(grid);
						m_Green.Min = 0;
						m_Green.Max = 255;
						m_Green.SelectAllOnFocus = true;
						m_Green.ValueChanged += NumericTyped;
					}

					{
						Label label = new Label(grid);
						label.Text = "B: ";
						label.Alignment = Alignment.Left | Alignment.CenterV;

						m_Blue = new NumericUpDown(grid);
						m_Blue.Min = 0;
						m_Blue.Max = 255;
						m_Blue.SelectAllOnFocus = true;
						m_Blue.ValueChanged += NumericTyped;
					}
				}
			}

			m_ColorSlider = new ColorSlider(this);
			m_ColorSlider.Margin = Margin.Two;
			m_ColorSlider.ColorChanged += ColorSliderChanged;
			m_ColorSlider.Dock = Dock.Right;

			EnableDefaultColor = false;

			SetColor(DefaultColor);
		}

		private void NumericTyped(ControlBase control, EventArgs args)
		{
			NumericUpDown box = control as NumericUpDown;
			if (box == null) return;

			int value = (int)box.Value;
			if (value < 0) value = 0;
			if (value > 255) value = 255;

			Color newColor = SelectedColor;

			if (box == m_Red)
				newColor = new Color(SelectedColor.A, value, SelectedColor.G, SelectedColor.B);
			else if (box == m_Green)
				newColor = new Color(SelectedColor.A, SelectedColor.R, value, SelectedColor.B);
			else if (box == m_Blue)
				newColor = new Color(SelectedColor.A, SelectedColor.R, SelectedColor.G, value);
			//else if (box.Name.Contains("Alpha"))
			//    newColor = Color.FromArgb(textValue, SelectedColor.R, SelectedColor.G, SelectedColor.B);

			m_ColorSlider.SetColor(newColor, false);
			m_LerpBox.SetColor(newColor, false, false);
			m_After.Color = newColor;

			if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
		}

		private void UpdateControls(Color color)
		{
			m_Red.SetValue(color.R, false);
			m_Green.SetValue(color.G, false);
			m_Blue.SetValue(color.B, false);
			m_After.Color = color;
		}

		/// <summary>
		/// Sets the selected color.
		/// </summary>
		/// <param name="color">Color to set.</param>
		/// <param name="onlyHue">Determines whether only the hue should be set.</param>
		/// <param name="reset">Determines whether the "before" color should be set as well.</param>
		public void SetColor(Color color, bool onlyHue = false, bool reset = false)
		{
			UpdateControls(color);

			if (reset)
				m_Before.Color = color;

			m_ColorSlider.SetColor(color, false);
			m_LerpBox.SetColor(color, onlyHue, false);
			m_After.Color = color;

			if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
		}

		private void ColorBoxChanged(ControlBase control, EventArgs args)
		{
			UpdateControls(SelectedColor);
			//Invalidate();

			if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
		}

		private void ColorSliderChanged(ControlBase control, EventArgs args)
		{
			m_LerpBox.SetColor(m_ColorSlider.SelectedColor, true, false);
			UpdateControls(SelectedColor);
			//Invalidate();

			if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
		}

		private void UpdateChildControlVisibility()
		{
			if (m_enableDefaultColor)
			{
				m_After.Margin = new Margin(2, 2, 2, 0);
				m_Before.Margin = new Margin(2, 0, 2, 2);
				m_After.Height = BaseUnit * 2;
				m_Before.Show();
			}
			else
			{
				m_After.Margin = Margin.Two;
				m_Before.Collapse();
				m_After.Height = BaseUnit * 4;
			}
		}
	}
}
