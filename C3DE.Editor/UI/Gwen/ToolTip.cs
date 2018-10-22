using Gwen.Control;

namespace Gwen
{
	/// <summary>
	/// Tooltip handling.
	/// </summary>
	public static class ToolTip
	{
		private static ControlBase g_ToolTip;

		/// <summary>
		/// Enables tooltip display for the specified control.
		/// </summary>
		/// <param name="control">Target control.</param>
		public static void Enable(ControlBase control)
		{
			if (null == control.ToolTip)
				return;

			ControlBase toolTip = control.ToolTip;
			g_ToolTip = control;
			toolTip.Measure(Size.Infinity);
			toolTip.Arrange(new Rectangle(Point.Zero, toolTip.MeasuredSize));
		}

		/// <summary>
		/// Disables tooltip display for the specified control.
		/// </summary>
		/// <param name="control">Target control.</param>
		public static void Disable(ControlBase control)
		{
			if (g_ToolTip == control)
			{
				g_ToolTip = null;
			}
		}

		/// <summary>
		/// Disables tooltip display for the specified control.
		/// </summary>
		/// <param name="control">Target control.</param>
		public static void ControlDeleted(ControlBase control)
		{
			Disable(control);
		}

		/// <summary>
		/// Renders the currently visible tooltip.
		/// </summary>
		/// <param name="skin"></param>
		public static void RenderToolTip(Skin.SkinBase skin)
		{
			if (null == g_ToolTip) return;

			Renderer.RendererBase render = skin.Renderer;

			Point oldRenderOffset = render.RenderOffset;
			Point mousePos = Input.InputHandler.MousePosition;
			Rectangle bounds = g_ToolTip.ToolTip.Bounds;

			Rectangle offset = Util.FloatRect(mousePos.X - bounds.Width / 2, mousePos.Y - bounds.Height - 10, bounds.Width, bounds.Height);
			offset = Util.ClampRectToRect(offset, g_ToolTip.GetCanvas().Bounds);

			//Calculate offset on screen bounds
			render.AddRenderOffset(offset);
			render.EndClip();

			skin.DrawToolTip(g_ToolTip.ToolTip);
			g_ToolTip.ToolTip.DoRender(skin);

			render.RenderOffset = oldRenderOffset;
		}
	}
}
