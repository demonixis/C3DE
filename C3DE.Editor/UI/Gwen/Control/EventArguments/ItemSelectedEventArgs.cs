using System;

namespace Gwen.Control
{
	public class ItemSelectedEventArgs : EventArgs
	{
		public ControlBase SelectedItem { get; private set; }

		internal ItemSelectedEventArgs(ControlBase selecteditem)
		{
			this.SelectedItem = selecteditem;
		}
	}
}
