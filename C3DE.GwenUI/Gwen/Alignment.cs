using System;
using Gwen.Control;

namespace Gwen
{
	[Flags]
	public enum Alignment
	{
		Left = ControlBase.InternalFlags.AlignHLeft,
		CenterH = ControlBase.InternalFlags.AlignHCenter,
		Right = ControlBase.InternalFlags.AlignHRight,
		Top = ControlBase.InternalFlags.AlignVTop,
		CenterV = ControlBase.InternalFlags.AlignVCenter,
		Bottom = ControlBase.InternalFlags.AlignVBottom,
		Center = CenterV | CenterH
	}

	public enum HorizontalAlignment
	{
		Left = ControlBase.InternalFlags.AlignHLeft,
		Center = ControlBase.InternalFlags.AlignHCenter,
		Right = ControlBase.InternalFlags.AlignHRight,
		Stretch = ControlBase.InternalFlags.AlignHStretch
	}

	public enum VerticalAlignment
	{
		Top = ControlBase.InternalFlags.AlignVTop,
		Center = ControlBase.InternalFlags.AlignVCenter,
		Bottom = ControlBase.InternalFlags.AlignVBottom,
		Stretch = ControlBase.InternalFlags.AlignVStretch
	}
}
