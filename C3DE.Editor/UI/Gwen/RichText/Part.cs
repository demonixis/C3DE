using System;

namespace Gwen.RichText
{
	public abstract class Part
	{
		public abstract string[] Split(ref Font font);
	}
}
