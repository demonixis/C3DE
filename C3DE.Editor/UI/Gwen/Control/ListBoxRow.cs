using System;

namespace Gwen.Control
{
    /// <summary>
    /// List box row (selectable).
    /// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
    public class ListBoxRow : TableRow
    {
        private bool m_Selected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxRow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ListBoxRow(ControlBase parent)
            : base(parent)
        {
			MouseInputEnabled = true;
            IsSelected = false;
        }

        /// <summary>
        /// Indicates whether the control is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return m_Selected; }
            set
            {
                m_Selected = value;             
                if (value)
                    SetTextColor(Skin.Colors.ListBox.Text_Selected);
                else
                    SetTextColor(Skin.Colors.ListBox.Text_Normal);
            }
        }

		/// <summary>
		/// Invoked when the row has been selected.
		/// </summary>
		public event GwenEventHandler<ItemSelectedEventArgs> Selected;

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawListBoxLine(this, IsSelected, EvenRow);
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
			base.OnMouseClickedLeft(x, y, down);
            if (down)
            {
                OnRowSelected();
            }
        }

		protected virtual void OnRowSelected()
		{
			if (Selected != null)
				Selected.Invoke(this, new ItemSelectedEventArgs(this));
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			ListBoxRow element = new ListBoxRow(parent);
			ListBox listBox = parent as ListBox;
			if (listBox != null)
				element.ColumnCount = listBox.ColumnCount;
			else
				throw new System.Xml.XmlException("Unknown parent for ListBox Row.");
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				int colIndex = 1;
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "Column")
					{
						if (parser.MoveToContent())
						{
							ControlBase column = parser.ParseElement(element);
							element.SetCellContents(colIndex++, column);
						}
						else
						{
							string colText = parser.GetAttribute("Text");
							element.SetCellText(colIndex++, colText != null ? colText : String.Empty);
						}
					}
				}
			}
			return element;
		}
	}
}
