using System;
using System.Collections.Generic;
using Gwen.Control.Internal;
using Gwen.Control.Layout;

namespace Gwen.Control
{
    /// <summary>
    /// RGBA color picker.
    /// </summary>
    public class ColorPicker : ControlBase, IColorPicker
    {
        private Color m_Color;

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor { get { return m_Color; } set { m_Color = value; UpdateControls(); } }

        /// <summary>
        /// Red value of the selected color.
        /// </summary>
        public int R { get { return m_Color.R; } set { m_Color = new Color(m_Color.A, value, m_Color.G, m_Color.B); } }

        /// <summary>
        /// Green value of the selected color.
        /// </summary>
        public int G { get { return m_Color.G; } set { m_Color = new Color(m_Color.A, m_Color.R, value, m_Color.B); } }

        /// <summary>
        /// Blue value of the selected color.
        /// </summary>
        public int B { get { return m_Color.B; } set { m_Color = new Color(m_Color.A, m_Color.R, m_Color.G, value); } }

        /// <summary>
        /// Alpha value of the selected color.
        /// </summary>
        public int A { get { return m_Color.A; } set { m_Color = new Color(value, m_Color.R, m_Color.G, m_Color.B); } }

        /// <summary>
        /// Invoked when the selected color has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPicker"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorPicker(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = true;

            CreateControls();
            SelectedColor = new Color(255, 50, 60, 70);
        }

		private void CreateControls()
		{
			VerticalLayout colorControlLayout = new VerticalLayout(this);
			colorControlLayout.Dock = Dock.Fill;

			CreateColorControl(colorControlLayout, "Red");
			CreateColorControl(colorControlLayout, "Green");
			CreateColorControl(colorControlLayout, "Blue");
			CreateColorControl(colorControlLayout, "Alpha");

			GroupBox finalGroup = new GroupBox(this);
			finalGroup.Dock = Dock.Right;
			finalGroup.Text = "Result";
			finalGroup.Name = "ResultGroupBox";

			DockLayout finalLayout = new DockLayout(finalGroup);

			ColorDisplay disp = new ColorDisplay(finalLayout);
			disp.Dock = Dock.Fill;
			disp.Name = "Result";
			disp.Width = Util.Ignore;
			disp.Height = Util.Ignore;
		}

		private void CreateColorControl(ControlBase parent, string name)
        {
            GroupBox colorGroup = new GroupBox(parent);
            colorGroup.Text = name;
            colorGroup.Name = name + "groupbox";

			DockLayout layout = new DockLayout(colorGroup);

            ColorDisplay disp = new ColorDisplay(layout);
			disp.Height = Util.Ignore;
			disp.Dock = Dock.Left;
            disp.Name = name;

            TextBoxNumeric numeric = new TextBoxNumeric(layout);
			numeric.Dock = Dock.Right;
			numeric.FitToText = "000";
			numeric.Name = name + "Box";
            numeric.SelectAllOnFocus = true;
            numeric.TextChanged += NumericTyped;

            HorizontalSlider slider = new HorizontalSlider(layout);
			slider.Dock = Dock.Fill;
			slider.VerticalAlignment = VerticalAlignment.Center;
            slider.SetRange(0, 255);
            slider.Name = name + "Slider";
            slider.ValueChanged += SlidersMoved;
        }

		private void NumericTyped(ControlBase control, EventArgs args)
        {
            TextBoxNumeric box = control as TextBoxNumeric;
            if (null == box)
                return;

            if (box.Text == string.Empty)
                return;

            int textValue = (int)box.Value;
            if (textValue < 0) textValue = 0;
            if (textValue > 255) textValue = 255;

            if (box.Name.Contains("Red"))
                R = textValue;

            if (box.Name.Contains("Green"))
                G = textValue;

            if (box.Name.Contains("Blue"))
                B = textValue;

            if (box.Name.Contains("Alpha"))
                A = textValue;

            UpdateControls();
        }

        private void UpdateColorControls(string name, Color col, int sliderVal)
        {
            ColorDisplay disp = FindChildByName(name, true) as ColorDisplay;
            disp.Color = col;

            HorizontalSlider slider = FindChildByName(name + "Slider", true) as HorizontalSlider;
            slider.Value = sliderVal;

            TextBoxNumeric box = FindChildByName(name + "Box", true) as TextBoxNumeric;
            box.Value = sliderVal;
        }

        private void UpdateControls()
        {	//This is a little weird, but whatever for now
            UpdateColorControls("Red", new Color(255, SelectedColor.R, 0, 0), SelectedColor.R);
            UpdateColorControls("Green", new Color(255, 0, SelectedColor.G, 0), SelectedColor.G);
            UpdateColorControls("Blue", new Color(255, 0, 0, SelectedColor.B), SelectedColor.B);
            UpdateColorControls("Alpha", new Color(SelectedColor.A, 255, 255, 255), SelectedColor.A);

            ColorDisplay disp = FindChildByName("Result", true) as ColorDisplay;
            disp.Color = SelectedColor;

            if (ColorChanged != null)
                ColorChanged.Invoke(this, EventArgs.Empty);
        }

        private void SlidersMoved(ControlBase control, EventArgs args)
        {
            /*
            HorizontalSlider* redSlider		= gwen_cast<HorizontalSlider>(	FindChildByName( "RedSlider",   true ) );
            HorizontalSlider* greenSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "GreenSlider", true ) );
            HorizontalSlider* blueSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "BlueSlider",  true ) );
            HorizontalSlider* alphaSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "AlphaSlider", true ) );
            */

            HorizontalSlider slider = control as HorizontalSlider;
            if (slider != null)
                SetColorByName(GetColorFromName(slider.Name), (int)slider.Value);

            UpdateControls();
            //SetColor( Gwen::Color( redSlider->GetValue(), greenSlider->GetValue(), blueSlider->GetValue(), alphaSlider->GetValue() ) );
        }

        private int GetColorByName(string colorName)
        {
            if (colorName == "Red")
                return SelectedColor.R;
            if (colorName == "Green")
                return SelectedColor.G;
            if (colorName == "Blue")
                return SelectedColor.B;
            if (colorName == "Alpha")
                return SelectedColor.A;
            return 0;
        }

        private static string GetColorFromName(string name)
        {
            if (name.Contains("Red"))
                return "Red";
            if (name.Contains("Green"))
                return "Green";
            if (name.Contains("Blue"))
                return "Blue";
            if (name.Contains("Alpha"))
                return "Alpha";
            return String.Empty;
        }

        private void SetColorByName(string colorName, int colorValue)
        {
            if (colorName == "Red")
                R = colorValue;
            else if (colorName == "Green")
                G = colorValue;
            else if (colorName == "Blue")
                B = colorValue;
            else if (colorName == "Alpha")
                A = colorValue;
        }

        /// <summary>
        /// Determines whether the Alpha control is visible.
        /// </summary>
        public bool AlphaVisible
        {
            get
            {
                GroupBox gb = FindChildByName("Alphagroupbox", true) as GroupBox;
                return !gb.IsHidden;
            }
            set
            {
                GroupBox gb = FindChildByName("Alphagroupbox", true) as GroupBox;
                gb.IsHidden = !value;
                //Invalidate();
            }
        }
    }
}
