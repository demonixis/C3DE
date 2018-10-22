using System;
using Gwen.Input;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	[Xml.XmlControl]
	public class MultilineTextBox : ScrollControl
	{
		private MultilineText m_Text;

		private Point m_CursorPos;
		private Point m_CursorEnd;

		private bool m_SelectAll;

		protected Rectangle m_CaretBounds;

		private float m_LastInputTime;

		private Point StartPoint
		{
			get
			{
				if (CursorPosition.Y == m_CursorEnd.Y)
				{
					return CursorPosition.X < CursorEnd.X ? CursorPosition : CursorEnd;
				}
				else {
					return CursorPosition.Y < CursorEnd.Y ? CursorPosition : CursorEnd;
				}
			}
		}

		private Point EndPoint
		{
			get
			{
				if (CursorPosition.Y == m_CursorEnd.Y)
				{
					return CursorPosition.X > CursorEnd.X ? CursorPosition : CursorEnd;
				}
				else {
					return CursorPosition.Y > CursorEnd.Y ? CursorPosition : CursorEnd;
				}
			}
		}

		/// <summary>
		/// Indicates whether the text has active selection.
		/// </summary>
		public bool HasSelection { get { return m_CursorPos != m_CursorEnd; } }

		/// <summary>
		/// Invoked when the text has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> TextChanged;

		/// <summary>
		/// Get a point representing where the cursor physically appears on the screen.
		/// Y is line number, X is character position on that line.
		/// </summary>
		public Point CursorPosition
		{
			get
			{
				int y = Util.Clamp(m_CursorPos.Y, 0, m_Text.TotalLines - 1);
				int x = Util.Clamp(m_CursorPos.X, 0, m_Text[y].Length); // X may be beyond the last character, but we will want to draw it at the end of line.

				return new Point(x, y);
			}
			set
			{
				m_CursorPos.X = value.X;
				m_CursorPos.Y = value.Y;
				RefreshCursorBounds();
			}
		}

		/// <summary>
		/// Get a point representing where the endpoint of text selection.
		/// Y is line number, X is character position on that line.
		/// </summary>
		public Point CursorEnd
		{
			get
			{
				int y = Util.Clamp(m_CursorEnd.Y, 0, m_Text.TotalLines - 1);
				int x = Util.Clamp(m_CursorEnd.X, 0, m_Text[y].Length); // X may be beyond the last character, but we will want to draw it at the end of line.

				return new Point(x, y);
			}
			set
			{
				m_CursorEnd.X = value.X;
				m_CursorEnd.Y = value.Y;
				RefreshCursorBounds();
			}
		}

		/// <summary>
		/// Indicates whether the control will accept Tab characters as input.
		/// </summary>
		[Xml.XmlProperty]
		public bool AcceptTabs { get; set; }

		/// <summary>
		/// Returns the number of lines that are in the Multiline Text Box.
		/// </summary>
		public int TotalLines
		{
			get
			{
				return m_Text.TotalLines;
			}
		}

		private int LineHeight
		{
			get
			{
				return m_Text.LineHeight;
			}
		}

		/// <summary>
		/// Gets and sets the text to display to the user. Each line is seperated by
		/// an Environment.NetLine character.
		/// </summary>
		[Xml.XmlProperty]
		public string Text
		{
			get
			{
				return m_Text.Text;
			}
			set
			{
				SetText(value);
			}
		}

		[Xml.XmlProperty]
		public Font Font { get { return m_Text.Font; } set { m_Text.Font = value; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="TextBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public MultilineTextBox(ControlBase parent) : base(parent)
		{
			Padding = Padding.Three;

			EnableScroll(true, true);
			AutoHideBars = true;

			MouseInputEnabled = true;
			KeyboardInputEnabled = true;
			KeyboardNeeded = true;
			IsTabable = false;
			AcceptTabs = true;

			m_Text = new MultilineText(this);
			m_Text.BoundsChanged += ScrollChanged;

			m_CursorPos = new Point(0, 0);
			m_CursorEnd = new Point(0, 0);
			m_SelectAll = false;

			Font = Skin.DefaultFont;

			AddAccelerator("Ctrl + C", OnCopy);
			AddAccelerator("Ctrl + X", OnCut);
			AddAccelerator("Ctrl + V", OnPaste);
			AddAccelerator("Ctrl + A", OnSelectAll);

			SetText(String.Empty);
		}

		/// <summary>
		/// Sets the label text.
		/// </summary>
		/// <param name="text">Text to set.</param>
		/// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
		public void SetText(string text, bool doEvents = true)
		{
			m_Text.SetText(text);

			m_CursorPos = Point.Zero;
			m_CursorEnd = m_CursorPos;

			UpdateText();
			RefreshCursorBounds();

			if (doEvents)
				OnTextChanged();
		}

		/// <summary>
		/// Inserts text at current cursor position, erasing selection if any.
		/// </summary>
		/// <param name="text">Text to insert.</param>
		public void InsertText(string text)
		{
			if (HasSelection)
			{
				EraseSelection();
			}

			m_CursorPos = m_Text.InsertText(text, CursorPosition);
			m_CursorEnd = m_CursorPos;

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();
		}

		/// <summary>
		/// Remove all text.
		/// </summary>
		public void Clear()
		{
			m_Text.Clear();

			m_CursorPos = Point.Zero;
			m_CursorEnd = m_CursorPos;

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();
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
			if (m_SelectAll)
			{
				OnSelectAll(this, EventArgs.Empty);
				//m_SelectAll = false;
				return;
			}

			Point coords = GetClosestCharacter(x, y);

			if (down)
			{
				CursorPosition = coords;

				if (!Input.InputHandler.IsShiftDown)
					CursorEnd = coords;

				InputHandler.MouseFocus = this;
			}
			else
			{
				if (InputHandler.MouseFocus == this)
				{
					CursorPosition = coords;
					InputHandler.MouseFocus = null;
				}
			}

			RefreshCursorBounds();
		}

		/// <summary>
		/// Handler invoked on mouse double click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		protected override void OnMouseDoubleClickedLeft(int x, int y)
		{
			//base.OnMouseDoubleClickedLeft(x, y);
			OnSelectAll(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handler invoked on mouse moved event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="dx">X change.</param>
		/// <param name="dy">Y change.</param>
		protected override void OnMouseMoved(int x, int y, int dx, int dy)
		{
			base.OnMouseMoved(x, y, dx, dy);
			if (InputHandler.MouseFocus != this) return;

			Point c = GetClosestCharacter(x, y);

			CursorPosition = c;

			RefreshCursorBounds();
		}

		/// <summary>
		/// Handler for character input event.
		/// </summary>
		/// <param name="chr">Character typed.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnChar(char chr)
		{
			//base.OnChar(chr);
			if (chr == '\t' && !AcceptTabs) return false;

			InsertText(chr.ToString());
			return true;
		}

		/// <summary>
		/// Handler for Paste event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void OnPaste(ControlBase from, EventArgs args)
		{
			base.OnPaste(from, args);
			InsertText(Platform.Platform.GetClipboardText());
		}

		/// <summary>
		/// Handler for Copy event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void OnCopy(ControlBase from, EventArgs args)
		{
			if (!HasSelection) return;
			base.OnCopy(from, args);

			Platform.Platform.SetClipboardText(GetSelection());
		}

		/// <summary>
		/// Handler for Cut event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void OnCut(ControlBase from, EventArgs args)
		{
			if (!HasSelection) return;
			base.OnCut(from, args);

			Platform.Platform.SetClipboardText(GetSelection());
			EraseSelection();
		}


		/// <summary>
		/// Handler for Select All event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void OnSelectAll(ControlBase from, EventArgs args)
		{
			//base.OnSelectAll(from);
			m_CursorEnd = new Point(0, 0);
			m_CursorPos = new Point(m_Text[m_Text.TotalLines - 1].Length, m_Text.TotalLines);

			RefreshCursorBounds();
		}

		/// <summary>
		/// Handler for Return keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyReturn(bool down)
		{
			if (down) return true;

			//Split current string, putting the rhs on a new line
			string currentLine = m_Text[m_CursorPos.Y];
			string lhs = currentLine.Substring(0, CursorPosition.X);
			string rhs = currentLine.Substring(CursorPosition.X);

			m_Text[m_CursorPos.Y] = lhs;
			m_Text.InsertLine(m_CursorPos.Y + 1, rhs);

			OnKeyDown(true);
			OnKeyHome(true);

			if (m_CursorPos.Y == TotalLines - 1)
			{
				ScrollToBottom();
			}

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Backspace keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyBackspace(bool down)
		{
			if (!down) return true;

			if (HasSelection)
			{
				EraseSelection();
				return true;
			}

			if (m_CursorPos.X == 0)
			{
				if (m_CursorPos.Y == 0)
				{
					return true; //Nothing left to delete
				}
				else
				{
					string lhs = m_Text[m_CursorPos.Y - 1];
					string rhs = m_Text[m_CursorPos.Y];
					m_Text.RemoveLine(m_CursorPos.Y);
					OnKeyUp(true);
					OnKeyEnd(true);
					m_Text[m_CursorPos.Y] = lhs + rhs;
				}
			}
			else
			{
				string currentLine = m_Text[m_CursorPos.Y];
				string lhs = currentLine.Substring(0, CursorPosition.X - 1);
				string rhs = currentLine.Substring(CursorPosition.X);
				m_Text[m_CursorPos.Y] = lhs + rhs;
				OnKeyLeft(true);
			}

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Delete keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyDelete(bool down)
		{
			if (!down) return true;

			if (HasSelection)
			{
				EraseSelection();
				return true;
			}

			if (m_CursorPos.X == m_Text[m_CursorPos.Y].Length)
			{
				if (m_CursorPos.Y == m_Text.TotalLines - 1)
				{
					return true; //Nothing left to delete
				}
				else
				{
					string lhs = m_Text[m_CursorPos.Y];
					string rhs = m_Text[m_CursorPos.Y + 1];
					m_Text.RemoveLine(m_CursorPos.Y + 1);
					OnKeyEnd(true);
					m_Text[m_CursorPos.Y] = lhs + rhs;
				}
			}
			else
			{
				string currentLine = m_Text[m_CursorPos.Y];
				string lhs = currentLine.Substring(0, CursorPosition.X);
				string rhs = currentLine.Substring(CursorPosition.X + 1);
				m_Text[m_CursorPos.Y] = lhs + rhs;
			}

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();

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
			if (!down) return true;

			if (m_CursorPos.Y > 0)
			{
				m_CursorPos.Y -= 1;
			}

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			RefreshCursorBounds();

			return true;
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
			if (!down) return true;

			if (m_CursorPos.Y < TotalLines - 1)
			{
				m_CursorPos.Y += 1;
			}

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Left Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyLeft(bool down)
		{
			if (!down) return true;

			if (m_CursorPos.X > 0)
			{
				m_CursorPos.X = Math.Min(m_CursorPos.X - 1, m_Text[m_CursorPos.Y].Length);
			}
			else
			{
				if (m_CursorPos.Y > 0)
				{
					OnKeyUp(down);
					OnKeyEnd(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Right Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyRight(bool down)
		{
			if (!down) return true;

			if (m_CursorPos.X < m_Text[m_CursorPos.Y].Length)
			{
				m_CursorPos.X = Math.Min(m_CursorPos.X + 1, m_Text[m_CursorPos.Y].Length);
			}
			else
			{
				if (m_CursorPos.Y < m_Text.TotalLines - 1)
				{
					OnKeyDown(down);
					OnKeyHome(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Home Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyHome(bool down)
		{
			if (!down) return true;

			m_CursorPos.X = 0;

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for End Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyEnd(bool down)
		{
			if (!down) return true;

			m_CursorPos.X = m_Text[m_CursorPos.Y].Length;

			if (!Input.InputHandler.IsShiftDown)
			{
				m_CursorEnd = m_CursorPos;
			}

			UpdateText();
			RefreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Tab Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyTab(bool down)
		{
			if (!AcceptTabs) return base.OnKeyTab(down);
			if (!down) return false;

			OnChar('\t');
			return true;
		}

		/// <summary>
		/// Returns currently selected text.
		/// </summary>
		/// <returns>Current selection.</returns>
		public string GetSelection()
		{
			if (!HasSelection) return String.Empty;

			string str = String.Empty;

			if (StartPoint.Y == EndPoint.Y)
			{
				int start = StartPoint.X;
				int end = EndPoint.X;

				str = m_Text[m_CursorPos.Y];
				str = str.Substring(start, end - start);
			}
			else
			{
				Point startPoint = StartPoint;
				Point endPoint = EndPoint;

				str = m_Text[startPoint.Y].Substring(startPoint.X) + Environment.NewLine; //Copy start
				for (int i = 1; i < endPoint.Y - startPoint.Y; i++)
				{
					str += m_Text[startPoint.Y + i] + Environment.NewLine; //Copy middle
				}
				str += m_Text[endPoint.Y].Substring(0, endPoint.X); //Copy end
			}

			return str;
		}

		/// <summary>
		/// Deletes selected text.
		/// </summary>
		public void EraseSelection()
		{
			if (StartPoint.Y == EndPoint.Y)
			{
				int start = StartPoint.X;
				int end = EndPoint.X;

				m_Text[StartPoint.Y] = m_Text[StartPoint.Y].Remove(start, end - start);
			}
			else
			{
				Point startPoint = StartPoint;
				Point endPoint = EndPoint;

				/* Remove Start */
				if (startPoint.X < m_Text[startPoint.Y].Length)
				{
					m_Text[startPoint.Y] = m_Text[startPoint.Y].Remove(startPoint.X);
				}

				/* Remove Middle */
				for (int i = 1; i < endPoint.Y - startPoint.Y; i++)
				{
					m_Text.RemoveLine(startPoint.Y + 1);
				}

				/* Remove End */
				if (endPoint.X < m_Text[startPoint.Y + 1].Length)
				{
					m_Text[startPoint.Y] += m_Text[startPoint.Y + 1].Substring(endPoint.X);
				}

				m_Text.RemoveLine(startPoint.Y + 1);
			}

			// Move the cursor to the start of the selection, 
			// since the end is probably outside of the string now.
			m_CursorPos = StartPoint;
			m_CursorEnd = StartPoint;

			UpdateText();
			OnTextChanged();
			RefreshCursorBounds();
		}

		/// <summary>
		/// Refreshes the cursor location and selected area when the inner panel scrolls
		/// </summary>
		/// <param name="control">The inner panel the text is embedded in</param>
		private void ScrollChanged(ControlBase control, EventArgs args)
		{
			RefreshCursorBounds(false);
		}

		/// <summary>
		/// Handler for text changed event.
		/// </summary>
		private void OnTextChanged()
		{
			if (TextChanged != null)
				TextChanged.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Invalidates the control.
		/// </summary>
		/// <remarks>
		/// Causes layout, repaint, invalidates cached texture.
		/// </remarks>
		private void UpdateText()
		{
			Invalidate();
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			base.Render(skin);

			if (ShouldDrawBackground)
				skin.DrawTextBox(this);

			if (!HasFocus) return;

			Rectangle oldClipRegion = skin.Renderer.ClipRegion;

			skin.Renderer.SetClipRegion(this.Container.Bounds);

			int verticalSize = this.LineHeight;

			// Draw selection.. if selected..
			if (m_CursorPos != m_CursorEnd)
			{
				if (StartPoint.Y == EndPoint.Y)
				{
					Point pA = GetCharacterPosition(StartPoint);
					Point pB = GetCharacterPosition(EndPoint);

					Rectangle selectionBounds = new Rectangle();
					selectionBounds.X = Math.Min(pA.X, pB.X);
					selectionBounds.Y = pA.Y;
					selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
					selectionBounds.Height = verticalSize;

					skin.Renderer.DrawColor = Skin.Colors.TextBox.Background_Selected;
					skin.Renderer.DrawFilledRect(selectionBounds);
				}
				else
				{
					/* Start */
					Point pA = GetCharacterPosition(StartPoint);
					Point pB = GetCharacterPosition(new Point(m_Text[StartPoint.Y].Length, StartPoint.Y));

					Rectangle selectionBounds = new Rectangle();
					selectionBounds.X = Math.Min(pA.X, pB.X);
					selectionBounds.Y = pA.Y;
					selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
					selectionBounds.Height = verticalSize;

					skin.Renderer.DrawColor = Skin.Colors.TextBox.Background_Selected;
					skin.Renderer.DrawFilledRect(selectionBounds);

					/* Middle */
					for (int i = 1; i < EndPoint.Y - StartPoint.Y; i++)
					{
						pA = GetCharacterPosition(new Point(0, StartPoint.Y + i));
						pB = GetCharacterPosition(new Point(m_Text[StartPoint.Y + i].Length, StartPoint.Y + i));

						selectionBounds = new Rectangle();
						selectionBounds.X = Math.Min(pA.X, pB.X);
						selectionBounds.Y = pA.Y;
						selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
						selectionBounds.Height = verticalSize;

						skin.Renderer.DrawColor = Skin.Colors.TextBox.Background_Selected;
						skin.Renderer.DrawFilledRect(selectionBounds);
					}

					/* End */
					pA = GetCharacterPosition(new Point(0, EndPoint.Y));
					pB = GetCharacterPosition(EndPoint);

					selectionBounds = new Rectangle();
					selectionBounds.X = Math.Min(pA.X, pB.X);
					selectionBounds.Y = pA.Y;
					selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
					selectionBounds.Height = verticalSize;

					skin.Renderer.DrawColor = Skin.Colors.TextBox.Background_Selected;
					skin.Renderer.DrawFilledRect(selectionBounds);
				}
			}

			// Draw caret
			float time = Platform.Platform.GetTimeInSeconds() - m_LastInputTime;

			if ((time % 1.0f) <= 0.5f)
			{
				skin.Renderer.DrawColor = Skin.Colors.TextBox.Caret;
				skin.Renderer.DrawFilledRect(m_CaretBounds);
			}

			skin.Renderer.ClipRegion = oldClipRegion;
		}

		private Point GetCharacterPosition(Point position)
		{
			Point p = m_Text.GetCharacterPosition(position);

			return new Point(p.X + m_Text.ActualLeft + Padding.Left, p.Y + m_Text.ActualTop + Padding.Top);
		}

		private Point GetClosestCharacter(int px, int py)
		{
			Point p = m_Text.CanvasPosToLocal(new Point(px, py));

			return m_Text.GetClosestCharacter(p);
		}

		protected void RefreshCursorBounds(bool makeCaretVisible = true)
		{
			m_LastInputTime = Platform.Platform.GetTimeInSeconds();

			if (makeCaretVisible)
				MakeCaretVisible();

			Point pA = GetCharacterPosition(CursorPosition);

			m_CaretBounds.X = pA.X;
			m_CaretBounds.Y = pA.Y;

			m_CaretBounds.Width = 1;
			m_CaretBounds.Height = this.LineHeight;

			Redraw();
		}

		protected virtual void MakeCaretVisible()
		{
			Size viewSize = ViewableContentSize;
			Point caretPos = GetCharacterPosition(CursorPosition);

			caretPos.X -= Padding.Left + m_Text.ActualLeft;
			caretPos.Y -= Padding.Top + m_Text.ActualTop;

			EnsureVisible(new Rectangle(caretPos.X, caretPos.Y, 5, LineHeight), new Size(viewSize.Width / 5, 0));
		}
	}
}
