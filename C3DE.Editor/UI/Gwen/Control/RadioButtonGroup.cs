using System;
using System.Linq;
using Gwen.Control.Layout;

namespace Gwen.Control
{
	/// <summary>
	/// Radio button group.
	/// </summary>
	[Xml.XmlControl(ElementName = "RadioButton", CustomHandler = "XmlElementHandler")]
	public class RadioButtonGroup : VerticalLayout
	{
		private LabeledRadioButton m_Selected;
		
		/// <summary>
		/// Selected radio button.
		/// </summary>
		public LabeledRadioButton Selected { get { return m_Selected; } }

		/// <summary>
		/// Internal name of the selected radio button.
		/// </summary>
		public string SelectedName { get { return m_Selected.Name; } }

		/// <summary>
		/// Text of the selected radio button.
		/// </summary>
		public string SelectedLabel { get { return m_Selected.Text; } }

		/// <summary>
		/// Index of the selected radio button.
		/// </summary>
		public int SelectedIndex { get { return Children.IndexOf(m_Selected); } }

		/// <summary>
		/// Invoked when the selected option has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<ItemSelectedEventArgs> SelectionChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonGroup"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public RadioButtonGroup(ControlBase parent) : base(parent)
		{
			IsTabable = false;
			KeyboardInputEnabled = true;
		}

		/// <summary>
		/// Adds a new option.
		/// </summary>
		/// <param name="text">Option text.</param>
		/// <param name="optionName">Internal name.</param>
		/// <param name="userData">User data.</param>
		/// <returns>Newly created control.</returns>
		public virtual LabeledRadioButton AddOption(string text, string optionName = null, object userData = null)
		{
			LabeledRadioButton lrb = new LabeledRadioButton(this);
			lrb.Name = optionName;
			lrb.UserData = userData;
			lrb.Text = text;
			lrb.Checked += OnRadioClicked;
			lrb.Margin = new Margin(0, 0, 0, 1);
			lrb.KeyboardInputEnabled = false;
			lrb.IsTabable = true;

			return lrb;
		}

		/// <summary>
		/// Adds an option.
		/// </summary>
		/// <param name="lrb">Radio button.</param>
		public virtual void AddOption(LabeledRadioButton lrb)
		{
			lrb.Checked += OnRadioClicked;
			lrb.Margin = new Margin(0, 0, 0, 1);
			lrb.KeyboardInputEnabled = false;
			lrb.IsTabable = true;
		}

		/// <summary>
		/// Handler for the option change.
		/// </summary>
		/// <param name="fromPanel">Event source.</param>
		protected virtual void OnRadioClicked(ControlBase fromPanel, EventArgs args)
		{
			RadioButton @checked = fromPanel as RadioButton;
			foreach (LabeledRadioButton rb in Children.OfType<LabeledRadioButton>()) // todo: optimize
			{
				if (rb.RadioButton == @checked)
					m_Selected = rb;
				else
					rb.RadioButton.IsChecked = false;
			}

			OnChanged(m_Selected);
		}

		protected virtual void OnChanged(ControlBase NewTarget)
		{
			if (SelectionChanged != null)
				SelectionChanged.Invoke(this, new ItemSelectedEventArgs(NewTarget));
		}

		/// <summary>
		/// Selects the specified option.
		/// </summary>
		/// <param name="index">Option to select.</param>
		public void SetSelection(int index)
		{
			if (index < 0 || index >= Children.Count)
				return;

			(Children[index] as LabeledRadioButton).RadioButton.Press();
		}

		/// <summary>
		/// Selects the specified option.
		/// </summary>
		/// <param name="name">Option name to select.</param>
		public void SetSelectionByName(string name)
		{
			ControlBase child = FindChildByName(name, false);
			if (child != null)
				(child as LabeledRadioButton).RadioButton.Press();
		}

		/// <summary>
		/// Selects the specified option with the given user data it finds.
		/// </summary>
		/// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
		/// If null is passed in, it will look for null/unset UserData.</param>
		public void SelectByUserData(object userdata)
		{
			ControlBase option = this.Children.Where(x => x.UserData.Equals(userdata)).FirstOrDefault();
			if (option != null)
				(option as LabeledRadioButton).RadioButton.Press();
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			RadioButtonGroup element = new RadioButtonGroup(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "Option")
					{
						element.AddOption(parser.ParseElement<LabeledRadioButton>(element));
					}
				}
			}
			return element;
		}
	}
}
