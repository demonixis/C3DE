using System;

namespace Gwen.Control
{
	public interface ITableRowFactory
	{
		TableRow Create();
		TableRow Create(string text, string name = "", object userData = null);
		TableRow Create(object item);
	}
}
