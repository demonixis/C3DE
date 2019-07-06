using System;

namespace Gwen.Control.Internal
{
	public abstract class ComboBoxBase : ControlBase
	{
		private readonly Menu m_Menu;
		private MenuItem m_SelectedItem;

		/// <summary>
		/// Invoked when the selected item has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<ItemSelectedEventArgs> ItemSelected;

		/// <summary>
		/// Index of the selected radio button.
		/// </summary>
		public int SelectedIndex { get { return Children.IndexOf(m_SelectedItem); } set { SetSelection(value); } }

		/// <summary>
		/// Selected item.
		/// </summary>
		/// <remarks>Not just String property, because items also have internal names.</remarks>
		public MenuItem SelectedItem
		{
			get { return m_SelectedItem; }
			set
			{
				if (value != null && value.Parent == m_Menu)
				{
					m_SelectedItem = value;
					OnItemSelected(m_SelectedItem, new ItemSelectedEventArgs(value));
				}
			}
		}

		/// <summary>
		/// Indicates whether the combo menu is open.
		/// </summary>
		public bool IsOpen { get { return m_Menu != null && !m_Menu.IsCollapsed; } }

		public override bool IsDisabled { set { if (value && m_Menu != null) m_Menu.Collapse(); base.IsDisabled = value; } }

		internal override bool IsMenuComponent
		{
			get { return true; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ComboBoxBase"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ComboBoxBase(ControlBase parent)
            : base(parent)
        {
			m_Menu = new Menu(GetCanvas());
			m_Menu.IconMarginDisabled = true;
			m_Menu.IsTabable = false;
			m_Menu.HorizontalAlignment = HorizontalAlignment.Stretch;

			IsTabable = true;
			KeyboardInputEnabled = true;
		}

		/// <summary>
		/// Adds a new item.
		/// </summary>
		/// <param name="label">Item label (displayed).</param>
		/// <param name="name">Item name.</param>
		/// <returns>Newly created control.</returns>
		public virtual MenuItem AddItem(string label, string name = null, object UserData = null)
		{
			MenuItem item = m_Menu.AddItem(label, String.Empty);
			item.Name = name;
			item.Selected += OnItemSelected;
			item.UserData = UserData;

			if (m_SelectedItem == null)
				OnItemSelected(item, new ItemSelectedEventArgs(null));

			return item;
		}

		/// <summary>
		/// Adds an item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void AddItem(MenuItem item)
		{
			item.Parent = m_Menu;

			m_Menu.AddItem(item);
			item.Selected += OnItemSelected;

			if (m_SelectedItem == null)
				OnItemSelected(item, new ItemSelectedEventArgs(null));
		}

		public override void Disable()
		{
			base.Disable();
			GetCanvas().CloseMenus();
		}

		/// <summary>
		/// Removes all items.
		/// </summary>
		public virtual void RemoveAll()
		{
			if (m_Menu != null)
				m_Menu.RemoveAll();

			m_SelectedItem = null;
		}

		/// <summary>
		/// Internal handler for item selected event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnItemSelected(ControlBase control, EventArgs args)
		{
			if (!IsDisabled)
			{
				//Convert selected to a menu item
				MenuItem item = control as MenuItem;
				if (null == item) return;

				m_SelectedItem = item;
				m_Menu.IsCollapsed = true;

				if (ItemSelected != null)
					ItemSelected.Invoke(this, new ItemSelectedEventArgs(item));

				Focus();
			}
		}

		/// <summary>
		/// Opens the combo.
		/// </summary>
		public virtual void Open()
		{
			if (!IsDisabled)
			{
				GetCanvas().CloseMenus();

				if (null == m_Menu) return;

				Point p = LocalPosToCanvas(Point.Zero);

				m_Menu.Width = this.ActualWidth;

				int canvasHeight = GetCanvas().ActualHeight;
				if (p.Y > canvasHeight - 100)
				{
					// We need to do layout for the menu here to know the height of it.
					m_Menu.Arrange(new Rectangle(Point.Zero, m_Menu.Measure(Size.Infinity)));
					m_Menu.Position = new Point(p.X, p.Y - m_Menu.ActualHeight);
				}
				else
				{
					m_Menu.Position = new Point(p.X, p.Y + ActualHeight);
				}

				m_Menu.Show();
				m_Menu.BringToFront();
			}
		}

		/// <summary>
		/// Closes the combo.
		/// </summary>
		public virtual void Close()
		{
			if (m_Menu == null)
				return;

			m_Menu.Collapse();
		}

		/// <summary>
		/// Handler for Down Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyDown(bool down)
		{
			if (down)
			{
				var it = m_Menu.Children.IndexOf(m_SelectedItem);
				if (it + 1 < m_Menu.Children.Count)
					OnItemSelected(this, new ItemSelectedEventArgs(m_Menu.Children[it + 1]));
			}
			return true;
		}

		/// <summary>
		/// Handler for Up Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyUp(bool down)
		{
			if (down)
			{
				var it = m_Menu.Children.LastIndexOf(m_SelectedItem);
				if (it - 1 >= 0)
					OnItemSelected(this, new ItemSelectedEventArgs(m_Menu.Children[it - 1]));
			}
			return true;
		}

		/// <summary>
		/// Selects the specified option.
		/// </summary>
		/// <param name="index">Option to select.</param>
		public void SetSelection(int index)
		{
			if (index < 0 || index >= Children.Count)
				return;

			SelectedItem = Children[index] as MenuItem;
		}

		/// <summary>
		/// Selects the first menu item with the given text it finds. 
		/// If a menu item can not be found that matches input, nothing happens.
		/// </summary>
		/// <param name="text">The label to look for, this is what is shown to the user.</param>
		public void SelectByText(string text)
		{
			foreach (MenuItem item in m_Menu.Children)
			{
				if (item.Text == text)
				{
					SelectedItem = item;
					return;
				}
			}
		}

		/// <summary>
		/// Selects the first menu item with the given internal name it finds.
		/// If a menu item can not be found that matches input, nothing happens.
		/// </summary>
		/// <param name="name">The internal name to look for. To select by what is displayed to the user, use "SelectByText".</param>
		public void SelectByName(string name)
		{
			foreach (MenuItem item in m_Menu.Children)
			{
				if (item.Name == name)
				{
					SelectedItem = item;
					return;
				}
			}
		}

		/// <summary>
		/// Selects the first menu item with the given user data it finds.
		/// If a menu item can not be found that matches input, nothing happens.
		/// </summary>
		/// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
		/// If null is passed in, it will look for null/unset UserData.</param>
		public void SelectByUserData(object userdata)
		{
			foreach (MenuItem item in m_Menu.Children)
			{
				if (userdata == null)
				{
					if (item.UserData == null)
					{
						SelectedItem = item;
						return;
					}
				}
				else if (userdata.Equals(item.UserData))
				{
					SelectedItem = item;
					return;
				}
			}
		}
	}
}
