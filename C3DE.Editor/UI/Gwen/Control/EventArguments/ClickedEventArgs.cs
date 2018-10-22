using System;

namespace Gwen.Control
{
	public class ClickedEventArgs : EventArgs
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public bool MouseDown { get; private set; }

		internal ClickedEventArgs(int x, int y, bool down)
		{
			this.X = x;
			this.Y = y;
			this.MouseDown = down;
		}
	}
}
