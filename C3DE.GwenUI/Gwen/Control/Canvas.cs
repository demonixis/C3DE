using System;
using System.Collections.Generic;
using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;

namespace Gwen.Control
{
	/// <summary>
	/// Canvas control. It should be the root parent for all other controls.
	/// </summary>
	public class Canvas : ControlBase
	{
		private bool m_NeedsRedraw;
		private float m_Scale;

		private Color m_BackgroundColor;

		// [omeg] these are not created by us, so no disposing
		internal ControlBase FirstTab;
		internal ControlBase NextTab;

		private readonly List<IDisposable> m_DisposeQueue; // dictionary for faster access?

		private readonly HashSet<ControlBase> m_MeasureQueue = new HashSet<ControlBase>();

		/// <summary>
		/// Scale for rendering.
		/// </summary>
		public override float Scale
		{
			get { return m_Scale; }
			set
			{
				if (m_Scale == value)
					return;

				m_Scale = value;

				if (Skin != null && Skin.Renderer != null)
					Skin.Renderer.Scale = m_Scale;

				OnScaleChanged();
				Redraw();
				Invalidate();
			}
		}

		/// <summary>
		/// Background color.
		/// </summary>
		public Color BackgroundColor { get { return m_BackgroundColor; } set { m_BackgroundColor = value; } }

		/// <summary>
		/// In most situations you will be rendering the canvas every frame. 
		/// But in some situations you will only want to render when there have been changes. 
		/// You can do this by checking NeedsRedraw.
		/// </summary>
		public bool NeedsRedraw { get { return m_NeedsRedraw; } set { m_NeedsRedraw = value; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Canvas"/> class.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		public Canvas(Skin.SkinBase skin)
		{
			Dock = Dock.Fill;
			SetBounds(0, 0, 10000, 10000);
			SetSkin(skin);
			Scale = 1.0f;
			BackgroundColor = Color.White;
			ShouldDrawBackground = false;

			m_DisposeQueue = new List<IDisposable>();

			FontCache.CreateCache(skin.Renderer);
		}

		public override void Dispose()
		{
			ProcessDelayedDeletes();

			// Dispose all cached fonts.
			FontCache.FreeCache();

			base.Dispose();
		}

		/// <summary>
		/// Re-renders the control, invalidates cached texture.
		/// </summary>
		public override void Redraw()
		{
			NeedsRedraw = true;
			base.Redraw();
		}

		// Children call parent.GetCanvas() until they get to 
		// this top level function.
		public override Canvas GetCanvas()
		{
			return this;
		}

		/// <summary>
		/// Renders the canvas. Call in your rendering loop.
		/// </summary>
		public void RenderCanvas()
		{
			DoThink();

			Skin.SkinBase skin = Skin;
			Renderer.RendererBase render = skin.Renderer;

			render.Begin();

			render.ClipRegion = Bounds;
			render.RenderOffset = Point.Zero;

			if (ShouldDrawBackground)
			{
				render.DrawColor = m_BackgroundColor;
				render.DrawFilledRect(RenderBounds);
			}

			DoRender(skin);

			DragAndDrop.RenderOverlay(this, skin);

			Gwen.ToolTip.RenderToolTip(skin);

			render.EndClip();

			render.End();
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			base.Render(skin);
			m_NeedsRedraw = false;
		}

		/// <summary>
		/// Handler invoked when control's bounds change.
		/// </summary>
		/// <param name="oldBounds">Old bounds.</param>
		protected override void OnBoundsChanged(Rectangle oldBounds)
		{
			base.OnBoundsChanged(oldBounds);
			Invalidate();
		}

		/// <summary>
		/// Processes input and layout. Also purges delayed delete queue.
		/// </summary>
		private void DoThink()
		{
			if (IsHidden || IsCollapsed)
				return;

			Animation.GlobalThink();

			// Reset tabbing
			NextTab = null;
			FirstTab = null;
			
			ProcessDelayedDeletes();

			// Check has focus etc..
			RecurseControls();

			// If we didn't have a next tab, cycle to the start.
			if (NextTab == null)
				NextTab = FirstTab;

			InputHandler.OnCanvasThink(this);

			// Update timers
			Timer.Tick();

			// Is total layout needed
			if (this.NeedsLayout)
			{
				DoLayout();
			}

			// Check if individual controls need layout
			if (m_MeasureQueue.Count > 0)
			{
				foreach (ControlBase element in m_MeasureQueue)
				{
					element.DoLayout();
				}

				m_MeasureQueue.Clear();
			}
		}

		/// <summary>
		/// Adds given control to the delete queue and detaches it from canvas. Don't call from Dispose, it modifies child list.
		/// </summary>
		/// <param name="control">Control to delete.</param>
		public void AddDelayedDelete(ControlBase control)
		{
			if (!m_DisposeQueue.Contains(control))
			{
				m_DisposeQueue.Add(control);
				RemoveChild(control, false);
			}
#if DEBUG
			else
				throw new InvalidOperationException("Control deleted twice");
#endif
		}

		private void ProcessDelayedDeletes()
		{
			//if (m_DisposeQueue.Count > 0)
			//    System.Diagnostics.Debug.Print("Canvas.ProcessDelayedDeletes: {0} items", m_DisposeQueue.Count);
			foreach (IDisposable control in m_DisposeQueue)
			{
				control.Dispose();
			}
			m_DisposeQueue.Clear();
		}

		public void AddToMeasure(ControlBase element)
		{
			m_MeasureQueue.Add(element);
		}

		/// <summary>
		/// Handles mouse movement events. Called from Input subsystems.
		/// </summary>
		/// <returns>True if handled.</returns>
		public bool Input_MouseMoved(int x, int y, int dx, int dy)
		{
			if (IsHidden || IsCollapsed)
				return false;

			// Todo: Handle scaling here..
			//float fScale = 1.0f / Scale();

			return InputHandler.OnMouseMoved(this, x, y, dx, dy);
		}

		/// <summary>
		/// Handles mouse button events. Called from Input subsystems.
		/// </summary>
		/// <returns>True if handled.</returns>
		public bool Input_MouseButton(int button, bool down)
		{
			if (IsHidden || IsCollapsed) return false;

			return InputHandler.OnMouseClicked(this, button, down);
		}

		/// <summary>
		/// Handles keyboard events. Called from Input subsystems.
		/// </summary>
		/// <returns>True if handled.</returns>
		public bool Input_Key(Key key, bool down)
		{
			if (IsHidden || IsCollapsed) return false;
			if (key <= Key.Invalid) return false;
			if (key >= Key.Count) return false;

			return InputHandler.OnKeyEvent(this, key, down);
		}

		/// <summary>
		/// Handles keyboard events. Called from Input subsystems.
		/// </summary>
		/// <returns>True if handled.</returns>
		public bool Input_Character(char chr)
		{
			if (IsHidden || IsCollapsed) return false;
			if (char.IsControl(chr)) return false;

			//Handle Accelerators
			if (InputHandler.HandleAccelerator(this, chr))
				return true;

			//Handle characters
			if (InputHandler.KeyboardFocus == null) return false;
			if (InputHandler.KeyboardFocus.GetCanvas() != this) return false;
			if (!InputHandler.KeyboardFocus.IsVisible) return false;
			if (InputHandler.IsControlDown) return false;

			return InputHandler.KeyboardFocus.InputChar(chr);
		}

		/// <summary>
		/// Handles the mouse wheel events. Called from Input subsystems.
		/// </summary>
		/// <returns>True if handled.</returns>
		public bool Input_MouseWheel(int val)
		{
			if (IsHidden || IsCollapsed) return false;
			if (InputHandler.HoveredControl == null) return false;
			if (InputHandler.HoveredControl == this) return false;
			if (InputHandler.HoveredControl.GetCanvas() != this) return false;

			return InputHandler.HoveredControl.InputMouseWheeled(val);
		}
	}
}
