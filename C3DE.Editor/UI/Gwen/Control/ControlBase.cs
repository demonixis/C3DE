using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;

namespace Gwen.Control
{
	/// <summary>
	/// Base control class.
	/// </summary>
	public abstract class ControlBase : IDisposable
	{
		/// <summary>
		/// Delegate used for all control event handlers.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="arguments" >Additional arguments. May be empty (EventArgs.Empty).</param>
		public delegate void GwenEventHandler<in T>(ControlBase sender, T arguments) where T : System.EventArgs;

		private bool m_Disposed;

		private ControlBase m_Parent;

		/// <summary>
		/// This is the panel's actual parent - most likely the logical 
		/// parent's InnerPanel (if it has one). You should rarely need this.
		/// </summary>
		private ControlBase m_ActualParent;

		private ControlBase m_ToolTip;

		private Skin.SkinBase m_Skin;

		private Rectangle m_Bounds;
		private Rectangle m_RenderBounds;
		private Rectangle m_InnerBounds;

		private Rectangle m_DesiredBounds;

		private Rectangle m_AnchorBounds;
		private Anchor m_Anchor;

		private Size m_MeasuredSize;

		private Size m_MinimumSize = Size.One;
		private Size m_MaximumSize = Size.Infinity;

		protected Padding m_Padding;
		private Margin m_Margin;

		private string m_Name;

		private Cursor m_Cursor;

		private bool m_CacheTextureDirty;
		private bool m_CacheToTexture;

		private Package m_DragAndDrop_Package;

		private Xml.Component m_Component;

		private object m_UserData;

		/// <summary>
		/// Real list of children.
		/// </summary>
		private readonly List<ControlBase> m_Children;

		/// <summary>
		/// Invoked when mouse pointer enters the control.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> HoverEnter;

		/// <summary>
		/// Invoked when mouse pointer leaves the control.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> HoverLeave;

		/// <summary>
		/// Invoked when control's bounds have been changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> BoundsChanged;

		/// <summary>
		/// Invoked when the control has been left-clicked.
		/// </summary>
		[Xml.XmlEvent]
		public virtual event GwenEventHandler<ClickedEventArgs> Clicked;

		/// <summary>
		/// Invoked when the control has been double-left-clicked.
		/// </summary>
		[Xml.XmlEvent]
		public virtual event GwenEventHandler<ClickedEventArgs> DoubleClicked;

		/// <summary>
		/// Invoked when the control has been right-clicked.
		/// </summary>
		[Xml.XmlEvent]
		public virtual event GwenEventHandler<ClickedEventArgs> RightClicked;

		/// <summary>
		/// Invoked when the control has been double-right-clicked.
		/// </summary>
		[Xml.XmlEvent]
		public virtual event GwenEventHandler<ClickedEventArgs> DoubleRightClicked;

		/// <summary>
		/// Returns true if any on click events are set.
		/// </summary>
		internal bool ClickEventAssigned
		{
			get
			{
				return Clicked != null || RightClicked != null || DoubleClicked != null || DoubleRightClicked != null;
			}
		}

		/// <summary>
		/// Accelerator map.
		/// </summary>
		private readonly Dictionary<string, GwenEventHandler<EventArgs>> m_Accelerators;

		/// <summary>
		/// Logical list of children.
		/// </summary>
		public virtual List<ControlBase> Children
		{
			get
			{
				return m_Children;
			}
		}

		/// <summary>
		/// The logical parent. It's usually what you expect, the control you've parented it to.
		/// </summary>
		public ControlBase Parent
		{
			get { return m_Parent; }
			set
			{
				if (m_Parent == value)
					return;

				if (m_Parent != null)
				{
					m_Parent.RemoveChild(this, false);
				}

				m_Parent = value;
				m_ActualParent = null;

				if (m_Parent != null)
				{
					m_Parent.AddChild(this);
				}
			}
		}

		/// <summary>
		/// Dock position.
		/// </summary>
		[Xml.XmlProperty]
		public Dock Dock
		{
			get { return (Dock)GetInternalFlag(InternalFlags.Dock_Mask); }
			set
			{
				if (CheckAndChangeInternalFlag(InternalFlags.Dock_Mask, (InternalFlags)value))
					Invalidate();
			}
		}

		/// <summary>
		/// Is layout needed.
		/// </summary>
		protected bool NeedsLayout { get { return IsSetInternalFlag(InternalFlags.NeedsLayout); } set { SetInternalFlag(InternalFlags.NeedsLayout, value); } }

		/// <summary>
		/// Is layout done at least once for the control.
		/// </summary>
		protected bool LayoutDone { get { return IsSetInternalFlag(InternalFlags.LayoutDone); } set { SetInternalFlag(InternalFlags.LayoutDone, value); } }

		/// <summary>
		/// Current skin.
		/// </summary>
		public Skin.SkinBase Skin
		{
			get
			{
				if (m_Skin != null)
					return m_Skin;
				if (m_Parent != null)
					return m_Parent.Skin;

				throw new InvalidOperationException("GetSkin: null");
			}
		}

		/// <summary>
		/// Current tooltip.
		/// </summary>
		public ControlBase ToolTip
		{
			get { return m_ToolTip; }
			set
			{
				m_ToolTip = value;
				if (m_ToolTip != null)
				{
					m_ToolTip.Collapse(true, false);
				}
			}
		}

		/// <summary>
		/// Label typed tool tip text.
		/// </summary>
		[Xml.XmlProperty]
		public string ToolTipText
		{
			get
			{
				if (m_ToolTip != null && m_ToolTip is Label)
					return ((Label)m_ToolTip).Text;
				else
					return String.Empty;
			}
			set
			{
				SetToolTipText(value);
			}
		}

		/// <summary>
		/// Indicates whether this control is a menu component.
		/// </summary>
		internal virtual bool IsMenuComponent
		{
			get
			{
				if (m_Parent == null)
					return false;
				return m_Parent.IsMenuComponent;
			}
		}

		/// <summary>
		/// Determines whether the control should be clipped to its bounds while rendering.
		/// </summary>
		protected virtual bool ShouldClip { get { return true; } }

		/// <summary>
		/// Minimum size that the control needs to draw itself correctly. Valid after DoMeasure call. This includes margins.
		/// </summary>
		public Size MeasuredSize { get { return m_MeasuredSize; } }

		public virtual float Scale
		{
			get
			{
				if (m_Parent != null)
					return m_Parent.Scale;
				else
					return 1.0f;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int BaseUnit
		{
			get
			{
				return Util.Ceil(Skin.BaseUnit * Scale);
			}
		}

		/// <summary>
		/// Current padding - inner spacing. Padding is not valid for all controls.
		/// </summary>
		[Xml.XmlProperty]
		public virtual Padding Padding
		{
			get { return m_Padding; }
			set
			{
				if (m_Padding == value)
					return;

				m_Padding = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Current margin - outer spacing.
		/// </summary>
		[Xml.XmlProperty]
		public Margin Margin
		{
			get { return m_Margin; }
			set
			{
				if (m_Margin == value)
					return;

				m_Margin = value;
				InvalidateParent();
			}
		}

		/// <summary>
		/// Vertical alignment of the control if the control is smaller than the available space.
		/// </summary>
		[Xml.XmlProperty]
		public VerticalAlignment VerticalAlignment
		{
			get { return (VerticalAlignment)GetInternalFlag(InternalFlags.AlignV_Mask); }
			set
			{
				if (CheckAndChangeInternalFlag(InternalFlags.AlignV_Mask, (InternalFlags)value))
					Invalidate();
			}
		}

		/// <summary>
		/// Horizontal alignment of the control if the control is smaller than the available space.
		/// </summary>
		[Xml.XmlProperty]
		public HorizontalAlignment HorizontalAlignment
		{
			get { return (HorizontalAlignment)GetInternalFlag(InternalFlags.AlignH_Mask); }
			set
			{
				if (CheckAndChangeInternalFlag(InternalFlags.AlignH_Mask, (InternalFlags)value))
					Invalidate();
			}
		}

		/// <summary>
		/// Indicates whether the control is on top of its parent's children.
		/// </summary>
		public virtual bool IsOnTop { get { return this == Parent.m_Children.First(); } } // todo: validate

		/// <summary>
		/// Component if this control is the base of the user defined control group.
		/// </summary>
		public Xml.Component Component { get { return m_Component; } set { m_Component = value; } }

		/// <summary>
		/// User data associated with the control.
		/// </summary>
		[Xml.XmlProperty]
		public object UserData { get { return m_UserData; } set { m_UserData = value; } }

		/// <summary>
		/// Indicates whether the control is hovered by mouse pointer.
		/// </summary>
		public virtual bool IsHovered { get { return InputHandler.HoveredControl == this; } }

		/// <summary>
		/// Indicates whether the control has focus.
		/// </summary>
		public bool HasFocus { get { return InputHandler.KeyboardFocus == this; } }

		/// <summary>
		/// Indicates whether the control is disabled.
		/// </summary>
		[Xml.XmlProperty]
		public virtual bool IsDisabled { get { return IsSetInternalFlag(InternalFlags.Disabled); } set { SetInternalFlag(InternalFlags.Disabled, value); } }

		/// <summary>
		/// Indicates whether the control is hidden.
		/// </summary>
		[Xml.XmlProperty]
		public virtual bool IsHidden { get { return IsSetInternalFlag(InternalFlags.Hidden); } set { if (CheckAndChangeInternalFlag(InternalFlags.Hidden, value)) Redraw(); } }

		/// <summary>
		/// Indicates whether the control is hidden.
		/// </summary>
		[Xml.XmlProperty]
		public virtual bool IsCollapsed { get { return IsSetInternalFlag(InternalFlags.Collapsed); } set { if (CheckAndChangeInternalFlag(InternalFlags.Collapsed, value)) InvalidateParent(); } }

		/// <summary>
		/// Determines whether the control's position should be restricted to parent's bounds.
		/// </summary>
		public bool RestrictToParent { get { return IsSetInternalFlag(InternalFlags.RestrictToParent); } set { SetInternalFlag(InternalFlags.RestrictToParent, value); } }

		/// <summary>
		/// Determines whether the control receives mouse input events.
		/// </summary>
		public bool MouseInputEnabled { get { return IsSetInternalFlag(InternalFlags.MouseInputEnabled); } set { SetInternalFlag(InternalFlags.MouseInputEnabled, value); } }

		/// <summary>
		/// Determines whether the control receives keyboard input events.
		/// </summary>
		public bool KeyboardInputEnabled { get { return IsSetInternalFlag(InternalFlags.KeyboardInputEnabled); } set { SetInternalFlag(InternalFlags.KeyboardInputEnabled, value); } }

		/// <summary>
		/// Determines whether the control receives keyboard character events.
		/// </summary>
		public bool KeyboardNeeded { get { return IsSetInternalFlag(InternalFlags.KeyboardNeeded); } set { SetInternalFlag(InternalFlags.KeyboardNeeded, value); } }

		/// <summary>
		/// Gets or sets the mouse cursor when the cursor is hovering the control.
		/// </summary>
		public Cursor Cursor { get { return m_Cursor; } set { m_Cursor = value; } }

		/// <summary>
		/// Indicates whether the control is tabable (can be focused by pressing Tab).
		/// </summary>
		public bool IsTabable { get { return IsSetInternalFlag(InternalFlags.Tabable); } set { SetInternalFlag(InternalFlags.Tabable, value); } }

		/// <summary>
		/// Flag for internal use to indicate if the control needs to measure itself.
		/// </summary>
		/// <remarks>Not used in the base class. This is only for control implementers to optimize measurement pass.</remarks>
		protected bool IsDirty { get { return IsSetInternalFlag(InternalFlags.Dirty); } set { SetInternalFlag(InternalFlags.Dirty, value); } }

		/// <summary>
		/// Indicates whether control's background should be drawn during rendering.
		/// </summary>
		public bool ShouldDrawBackground { get { return IsSetInternalFlag(InternalFlags.DrawBackground); } set { SetInternalFlag(InternalFlags.DrawBackground, value); } }

		/// <summary>
		/// Indicates whether the renderer should cache drawing to a texture to improve performance (at the cost of memory).
		/// </summary>
		public bool ShouldCacheToTexture { get { return m_CacheToTexture; } set { m_CacheToTexture = value; /*Children.ForEach(x => x.ShouldCacheToTexture=value);*/ } }

		/// <summary>
		/// Gets or sets the control's internal name.
		/// </summary>
		[Xml.XmlProperty]
		public string Name { get { return m_Name; } set { m_Name = value; } }

		/// <summary>
		/// Control's size and position relative to the parent.
		/// </summary>
		public Rectangle Bounds { get { return m_Bounds; } }

		/// <summary>
		/// Bounds for the renderer.
		/// </summary>
		public Rectangle RenderBounds { get { return m_RenderBounds; } }

		/// <summary>
		/// Bounds adjusted by padding.
		/// </summary>
		public Rectangle InnerBounds { get { return m_InnerBounds; } }

		/// <summary>
		/// Size restriction.
		/// </summary>
		[Xml.XmlProperty]
		public Size MinimumSize { get { return m_MinimumSize; } set { m_MinimumSize = value; InvalidateParent(); } }

		/// <summary>
		/// Size restriction.
		/// </summary>
		[Xml.XmlProperty]
		public Size MaximumSize { get { return m_MaximumSize; } set { m_MaximumSize = value; InvalidateParent(); } }

		/// <summary>
		/// Determines whether hover should be drawn during rendering.
		/// </summary>
		protected bool ShouldDrawHover { get { return InputHandler.MouseFocus == this || InputHandler.MouseFocus == null; } }

		protected virtual bool AccelOnlyFocus { get { return false; } }
		protected virtual bool NeedsInputChars { get { return false; } }

		/// <summary>
		/// Indicates whether the control and its parents are visible.
		/// </summary>
		public bool IsVisible
		{
			get
			{
				if (IsHidden)
					return false;

				if (IsCollapsed)
					return false;

				if (Parent != null)
					return Parent.IsVisible;

				return true;
			}
		}

		/// <summary>
		/// Location of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualLeft { get { return m_Bounds.X; } }
		/// <summary>
		/// Location of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualTop { get { return m_Bounds.Y; } }
		/// <summary>
		/// Width of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualWidth { get { return m_Bounds.Width; } }
		/// <summary>
		/// Height of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualHeight { get { return m_Bounds.Height; } }

		/// <summary>
		/// Location of the control. Valid after DoArrange call.
		/// </summary>
		public Point ActualPosition { get { return m_Bounds.Location; } }
		/// <summary>
		/// Size of the control. Valid after DoArrange call.
		/// </summary>
		public Size ActualSize { get { return m_Bounds.Size; } }

		/// <summary>
		/// Location of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualRight { get { return m_Bounds.Right; } }
		/// <summary>
		/// Location of the control. Valid after DoArrange call.
		/// </summary>
		public int ActualBottom { get { return m_Bounds.Bottom; } }

		/// <summary>
		/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
		/// </summary>
		[Xml.XmlProperty]
		public virtual int Left { get { return m_DesiredBounds.X; } set { if (m_DesiredBounds.X == value) return; m_DesiredBounds.X = value; InvalidateParent(); } }
		/// <summary>
		/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
		/// </summary>
		[Xml.XmlProperty]
		public virtual int Top { get { return m_DesiredBounds.Y; } set { if (m_DesiredBounds.Y == value) return; m_DesiredBounds.Y = value; InvalidateParent(); } }
		/// <summary>
		/// Desired size of the control. Set this value only if HorizontalAlignment is not Stretch. By default this value is ignored.
		/// </summary>
		[Xml.XmlProperty]
		public virtual int Width { get { return m_DesiredBounds.Width; } set { if (m_DesiredBounds.Width == value) return; m_DesiredBounds.Width = value; /*if (m_HorizontalAlignment == HorizontalAlignment.Stretch) m_HorizontalAlignment = HorizontalAlignment.Left;*/ InvalidateParent(); } }
		/// <summary>
		/// Desired size of the control. Set this value only if VerticalAlignment is not Stretch. By default this value is ignored.
		/// </summary>
		[Xml.XmlProperty]
		public virtual int Height { get { return m_DesiredBounds.Height; } set { if (m_DesiredBounds.Height == value) return; m_DesiredBounds.Height = value; /*if (m_VerticalAlignment == VerticalAlignment.Stretch) m_VerticalAlignment = VerticalAlignment.Top;*/ InvalidateParent(); } }

		/// <summary>
		/// Desired location of the control. Used only on default layout (DockLayout) if Dock property is None.
		/// </summary>
		[Xml.XmlProperty]
		public virtual Point Position { get { return m_DesiredBounds.Location; } set { if (m_DesiredBounds.Location == value) return; m_DesiredBounds.Location = value; InvalidateParent(); } }
		/// <summary>
		/// Desired size of the control. Set this only if both of alignments are not Stretch. By default this value is ignored.
		/// </summary>
		[Xml.XmlProperty]
		public virtual Size Size { get { return m_DesiredBounds.Size; } set { if (m_DesiredBounds.Size == value) return; m_DesiredBounds.Size = value; InvalidateParent(); } }

		/// <summary>
		/// Desired location and size of the control. Set this only if both of alignments are not Stretch. Used only on default layout (DockLayout) if Dock property is None. By default size is ignored.
		/// </summary>
		[Xml.XmlProperty]
		public virtual Rectangle DesiredBounds { get { return m_DesiredBounds; } set { if (m_DesiredBounds == value) return; m_DesiredBounds = value; InvalidateParent(); } }

		/// <summary>
		/// Default location and size of the control insize the container. Used only on AnchorLayout.
		/// </summary>
		[Xml.XmlProperty]
		public Rectangle AnchorBounds { get { return m_AnchorBounds; } set { if (m_AnchorBounds == value) return; m_AnchorBounds = value; Invalidate(); } }
		/// <summary>
		/// How the control is moved and/or stretched if the container size changes. Used only on AnchorLayout.
		/// </summary>
		[Xml.XmlProperty]
		public Anchor Anchor { get { return m_Anchor; } set { if (m_Anchor == value) return; m_Anchor = value; Invalidate(); } }

		/// <summary>
		/// Enable this if the parent of the control doesn't need to know if a new layout is needed.
		/// </summary>
		protected bool IsVirtualControl { get { return IsSetInternalFlag(InternalFlags.VirtualControl); } set { SetInternalFlag(InternalFlags.VirtualControl, value); } }

		/// <summary>
		/// Determines whether margin, padding and bounds outlines for the control will be drawn. Applied recursively to all children.
		/// </summary>
		public bool DrawDebugOutlines
		{
			get { return IsSetInternalFlag(InternalFlags.DrawDebugOutlines); }
			set
			{
				if (!CheckAndChangeInternalFlag(InternalFlags.DrawDebugOutlines, value))
					return;
				foreach (ControlBase child in Children)
				{
					child.DrawDebugOutlines = value;
				}
			}
		}

		public Color PaddingOutlineColor { get; set; }
		public Color MarginOutlineColor { get; set; }
		public Color BoundsOutlineColor { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ControlBase"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ControlBase(ControlBase parent = null)
		{
			m_InternalFlags = 0; // All flags set to false by default

			m_Children = new List<ControlBase>();
			m_Accelerators = new Dictionary<string, GwenEventHandler<EventArgs>>();

			m_Bounds = new Rectangle(Point.Zero, Size.Infinity);
			m_Padding = Padding.Zero;
			m_Margin = Margin.Zero;

			m_Anchor = Anchor.LeftTop;

			m_DesiredBounds = new Rectangle(0, 0, Util.Ignore, Util.Ignore);

			m_AnchorBounds = new Rectangle(0, 0, 0, 0);

			SetInternalFlag(InternalFlags.AlignHStretch | InternalFlags.AlignVStretch | InternalFlags.DockNone | InternalFlags.DrawBackground | InternalFlags.Dirty, true);

			Parent = parent;

			Invalidate();
			Cursor = Cursor.Normal;
			m_ToolTip = null;
			m_CacheTextureDirty = true;
			m_CacheToTexture = false;

			BoundsOutlineColor = Color.Red;
			MarginOutlineColor = Color.Green;
			PaddingOutlineColor = Color.Blue;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			if (m_Disposed)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine(String.Format("Control {{{0}}} disposed twice.", this));
#endif
				return;
			}

			if (InputHandler.HoveredControl == this)
				InputHandler.HoveredControl = null;
			if (InputHandler.KeyboardFocus == this)
				InputHandler.KeyboardFocus = null;
			if (InputHandler.MouseFocus == this)
				InputHandler.MouseFocus = null;

			DragAndDrop.ControlDeleted(this);
			Gwen.ToolTip.ControlDeleted(this);
			Animation.Cancel(this);

			foreach (ControlBase child in m_Children)
				child.Dispose();

			if (m_ToolTip != null)
				m_ToolTip.Dispose();

			m_Children.Clear();

			m_Disposed = true;
			GC.SuppressFinalize(this);
		}

#if DEBUG
		~ControlBase()
		{
			throw new InvalidOperationException(String.Format("IDisposable object {{{0}}} finalized.", this));
		}
#endif

		/// <summary>
		/// Detaches the control from canvas and adds to the deletion queue (processed in Canvas.DoThink).
		/// </summary>
		public void DelayedDelete()
		{
			GetCanvas().AddDelayedDelete(this);
		}

		public override string ToString()
		{
			string type = GetType().ToString();
			string name = String.IsNullOrWhiteSpace(m_Name) ? "" : " Name: " + m_Name;
			if (this is MenuItem)
				return type + name + " [MenuItem: " + (this as MenuItem).Text + "]";
			if (this is Label)
				return type + name + " [Label: " + (this as Label).Text + "]";
			if (this is Control.Internal.Text)
				return type + name + " [Text: " + (this as Control.Internal.Text).String + "]";
			return type + name;
		}

		/// <summary>
		/// Gets the canvas (root parent) of the control.
		/// </summary>
		/// <returns></returns>
		public virtual Canvas GetCanvas()
		{
			ControlBase canvas = m_Parent;
			if (canvas == null)
				return null;

			return canvas.GetCanvas();
		}

		/// <summary>
		/// Enables the control.
		/// </summary>
		public void Enable()
		{
			IsDisabled = false;
		}

		/// <summary>
		/// Disables the control.
		/// </summary>
		public virtual void Disable()
		{
			IsDisabled = true;
		}

		/// <summary>
		/// Default accelerator handler.
		/// </summary>
		/// <param name="control">Event source.</param>
		private void DefaultAcceleratorHandler(ControlBase control, EventArgs args)
		{
			OnAccelerator();
		}

		/// <summary>
		/// Default accelerator handler.
		/// </summary>
		protected virtual void OnAccelerator()
		{

		}

		/// <summary>
		/// Hides the control. Hidden controls participate in the layout process. If you don't want to layout, use Collapse.
		/// </summary>
		public virtual void Hide()
		{
			IsHidden = true;
		}

		/// <summary>
		/// Collapse or show the control. Collapsed controls don't participate in the layout process and are hidden.
		/// </summary>
		/// <param name="collapsed">Collapse or show.</param>
		/// <param name="measure">Is layout triggered.</param>
		public virtual void Collapse(bool collapsed = true, bool measure = true)
		{
			if (!measure)
				SetInternalFlag(InternalFlags.Collapsed, collapsed);
			else
				IsCollapsed = collapsed;
		}

		/// <summary>
		/// Shows the control.
		/// </summary>
		public virtual void Show()
		{
			IsCollapsed = false;
			IsHidden = false;
		}

		/// <summary>
		/// Creates a tooltip for the control.
		/// </summary>
		/// <param name="text">Tooltip text.</param>
		public virtual void SetToolTipText(string text)
		{
			Label tooltip = new Label(this);
			tooltip.Parent = null; // ToolTip doesn't have a parent
			tooltip.m_Skin = Skin; // and that's why we need to set skin here.
			tooltip.Text = text;
			tooltip.TextColorOverride = Skin.Colors.TooltipText;
			tooltip.Padding = new Padding(5, 3, 5, 3);

			MouseInputEnabled = true;
			ToolTip = tooltip;
		}

		/// <summary>
		/// Trigger the layout process.
		/// </summary>
		public virtual void Invalidate()
		{
			if (!this.IsVirtualControl || !this.LayoutDone)
			{
				NeedsLayout = true;

				if (m_Parent != null)
					if (!m_Parent.NeedsLayout)
						m_Parent.Invalidate();
			}
			else
			{
				Canvas canvas = GetCanvas();
				if (canvas != null)
					canvas.AddToMeasure(this);
			}
		}

		/// <summary>
		/// Trigger parent layout process. Use this instead of Invalidate() if you know that
		/// the parent is affected some way by the change.
		/// </summary>
		public virtual void InvalidateParent()
		{
			if (m_Parent != null)
				m_Parent.Invalidate();
		}

		/// <summary>
		/// Sends the control to the bottom of paren't visibility stack.
		/// </summary>
		public virtual void SendToBack()
		{
			if (m_ActualParent == null)
				return;
			if (m_ActualParent.m_Children.Count == 0)
				return;
			if (m_ActualParent.m_Children.First() == this)
				return;

			m_ActualParent.m_Children.Remove(this);
			m_ActualParent.m_Children.Insert(0, this);
		}

		/// <summary>
		/// Brings the control to the top of paren't visibility stack.
		/// </summary>
		public virtual void BringToFront()
		{
			if (m_ActualParent == null)
				return;
			if (m_ActualParent.m_Children.Last() == this)
				return;

			m_ActualParent.m_Children.Remove(this);
			m_ActualParent.m_Children.Add(this);
			Redraw();
		}

		public virtual void BringNextToControl(ControlBase child, bool behind)
		{
			if (null == m_ActualParent)
				return;

			int index = m_ActualParent.m_Children.IndexOf(this);
			int newIndex = m_ActualParent.m_Children.IndexOf(child);

			if (index == -1 || newIndex == -1)
				return;

			if (newIndex == 0 && !behind)
			{
				SendToBack();
				return;
			}
			else if (newIndex == m_ActualParent.m_Children.Count - 1 && behind)
			{
				BringToFront();
				return;
			}

			m_ActualParent.m_Children.Remove(this);
			if (newIndex > index)
				newIndex--;

			if (behind)
				newIndex++;

			m_ActualParent.m_Children.Insert(newIndex, this);
		}

		public virtual void MoveChildToIndex(int index)
		{
			if (m_ActualParent == null)
				return;

			int oldIndex = m_ActualParent.m_Children.IndexOf(this);

			if (oldIndex == index)
				return;

			if (oldIndex < index)
				index--;

			m_ActualParent.m_Children.Remove(this);
			m_ActualParent.m_Children.Insert(index, this);
		}

		/// <summary>
		/// Finds a child by name.
		/// </summary>
		/// <param name="name">Child name.</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found control or null.</returns>
		public virtual ControlBase FindChildByName(string name, bool recursive = false)
		{
			ControlBase b = this.Children.Where(x => x.m_Name == name).FirstOrDefault();
			if (b != null)
				return b;

			if (recursive)
			{
				foreach (ControlBase child in this.Children)
				{
					b = child.FindChildByName(name, true);
					if (b != null)
						return b;
				}
			}
			return null;
		}

		/// <summary>
		/// Attaches specified control as a child of this one.
		/// </summary>
		/// <param name="child">Control to be added as a child.</param>
		public virtual void AddChild(ControlBase child)
		{
			m_Children.Add(child);
			child.m_ActualParent = this;

			OnChildAdded(child);
		}

		/// <summary>
		/// Detaches specified control from this one.
		/// </summary>
		/// <param name="child">Child to be removed.</param>
		/// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
		public virtual void RemoveChild(ControlBase child, bool dispose)
		{
			m_Children.Remove(child);
			OnChildRemoved(child);

			if (dispose)
				child.DelayedDelete();
		}

		/// <summary>
		/// Removes all children (and disposes them).
		/// </summary>
		public virtual void DeleteAllChildren()
		{
			// todo: probably shouldn't invalidate after each removal
			while (m_Children.Count > 0)
				RemoveChild(m_Children[0], true);
		}

		/// <summary>
		/// Handler invoked when a child is added.
		/// </summary>
		/// <param name="child">Child added.</param>
		protected virtual void OnChildAdded(ControlBase child)
		{
		}

		/// <summary>
		/// Handler invoked when a child is removed.
		/// </summary>
		/// <param name="child">Child removed.</param>
		protected virtual void OnChildRemoved(ControlBase child)
		{
		}

		/// <summary>
		/// Moves the control to a specific point, clamping on paren't bounds if RestrictToParent is set.
		/// This function will override control location set by layout or user.
		/// </summary>
		/// <param name="x">Target x coordinate.</param>
		/// <param name="y">Target y coordinate.</param>
		public virtual void MoveTo(int x, int y)
		{
			if (RestrictToParent && (Parent != null))
			{
				ControlBase parent = Parent;
				if (x < Padding.Left)
					x = Padding.Left;
				else if (x + ActualWidth > parent.ActualWidth - Padding.Right)
					x = parent.ActualWidth - ActualWidth - Padding.Right;
				if (y < Padding.Top)
					y = Padding.Top;
				else if (y + ActualHeight > parent.ActualHeight - Padding.Bottom)
					y = parent.ActualHeight - ActualHeight - Padding.Bottom;
			}

			SetBounds(x, y, ActualWidth, ActualHeight);

			m_DesiredBounds.X = m_Bounds.X;
			m_DesiredBounds.Y = m_Bounds.Y;
		}

		/// <summary>
		/// Sets the control position.
		/// </summary>
		/// <param name="x">Target x coordinate.</param>
		/// <param name="y">Target y coordinate.</param>
		/// <remarks>Bounds are reset after the next layout pass.</remarks>
		public virtual void SetPosition(float x, float y)
		{
			SetPosition((int)x, (int)y);
		}

		/// <summary>
		/// Sets the control position.
		/// </summary>
		/// <param name="x">Target x coordinate.</param>
		/// <param name="y">Target y coordinate.</param>
		/// <remarks>Bounds are reset after the next layout pass.</remarks>
		public virtual void SetPosition(int x, int y)
		{
			SetBounds(x, y, ActualWidth, ActualHeight);
		}

		/// <summary>
		/// Sets the control size.
		/// </summary>
		/// <param name="width">New width.</param>
		/// <param name="height">New height.</param>
		/// <returns>True if bounds changed.</returns>
		/// <remarks>Bounds are reset after the next layout pass.</remarks>
		public virtual bool SetSize(int width, int height)
		{
			return SetBounds(ActualLeft, ActualTop, width, height);
		}

		/// <summary>
		/// Sets the control bounds.
		/// </summary>
		/// <param name="bounds">New bounds.</param>
		/// <returns>True if bounds changed.</returns>
		/// <remarks>Bounds are reset after the next layout pass.</remarks>
		public virtual bool SetBounds(Rectangle bounds)
		{
			return SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}

		/// <summary>
		/// Sets the control bounds.
		/// </summary>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <returns>
		/// True if bounds changed.
		/// </returns>
		/// <remarks>Bounds are reset after the next layout pass.</remarks>
		public virtual bool SetBounds(int x, int y, int width, int height)
		{
			if (m_Bounds.X == x &&
				m_Bounds.Y == y &&
				m_Bounds.Width == width &&
				m_Bounds.Height == height)
				return false;

			Rectangle oldBounds = Bounds;

			m_Bounds.X = x;
			m_Bounds.Y = y;

			m_Bounds.Width = width;
			m_Bounds.Height = height;

			OnBoundsChanged(oldBounds);

			Redraw();
			UpdateRenderBounds();

			if (BoundsChanged != null)
				BoundsChanged.Invoke(this, EventArgs.Empty);

			return true;
		}

		/// <summary>
		/// Handler invoked when control's bounds change.
		/// </summary>
		/// <param name="oldBounds">Old bounds.</param>
		protected virtual void OnBoundsChanged(Rectangle oldBounds)
		{
		}

		/// <summary>
		/// Handler invoked when control's scale changes.
		/// </summary>
		protected virtual void OnScaleChanged()
		{
			foreach (ControlBase child in m_Children)
			{
				child.OnScaleChanged();
			}
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected virtual void Render(Skin.SkinBase skin)
		{
		}

		/// <summary>
		/// Renders the control to a cache using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		/// <param name="master">Root parent.</param>
		protected virtual void DoCacheRender(Skin.SkinBase skin, ControlBase master)
		{
			Renderer.RendererBase render = skin.Renderer;
			Renderer.ICacheToTexture cache = render.CTT;

			if (cache == null)
				return;

			Point oldRenderOffset = render.RenderOffset;
			Rectangle oldRegion = render.ClipRegion;

			if (this != master)
			{
				render.AddRenderOffset(Bounds);
				render.AddClipRegion(Bounds);
			}
			else
			{
				render.RenderOffset = Point.Zero;
				render.ClipRegion = new Rectangle(0, 0, ActualWidth, ActualHeight);
			}

			if (m_CacheTextureDirty && render.ClipRegionVisible)
			{
				render.StartClip();

				if (ShouldCacheToTexture)
					cache.SetupCacheTexture(this);

				//Render myself first
				//var old = render.ClipRegion;
				//render.ClipRegion = Bounds;
				//var old = render.RenderOffset;
				//render.RenderOffset = new Point(Bounds.X, Bounds.Y);
				Render(skin);
				//render.RenderOffset = old;
				//render.ClipRegion = old;

				if (m_Children.Count > 0)
				{
					//Now render my kids
					foreach (ControlBase child in m_Children)
					{
						if (child.IsHidden || child.IsCollapsed)
							continue;
						child.DoCacheRender(skin, master);
					}
				}

				if (ShouldCacheToTexture)
				{
					cache.FinishCacheTexture(this);
					m_CacheTextureDirty = false;
				}
			}

			render.ClipRegion = oldRegion;
			render.StartClip();
			render.RenderOffset = oldRenderOffset;

			if (ShouldCacheToTexture)
				cache.DrawCachedControlTexture(this);
		}

		/// <summary>
		/// Rendering logic implementation.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		internal virtual void DoRender(Skin.SkinBase skin)
		{
			// If this control has a different skin, 
			// then so does its children.
			if (m_Skin != null)
				skin = m_Skin;

			// Do think
			Think();

			Renderer.RendererBase render = skin.Renderer;

			if (render.CTT != null && ShouldCacheToTexture)
			{
				DoCacheRender(skin, this);
				return;
			}

			RenderRecursive(skin, Bounds);

			if (DrawDebugOutlines)
				skin.DrawDebugOutlines(this);
		}

		/// <summary>
		/// Recursive rendering logic.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		/// <param name="clipRect">Clipping rectangle.</param>
		protected virtual void RenderRecursive(Skin.SkinBase skin, Rectangle clipRect)
		{
			Renderer.RendererBase render = skin.Renderer;
			Point oldRenderOffset = render.RenderOffset;

			render.AddRenderOffset(clipRect);

			RenderUnder(skin);

			Rectangle oldRegion = render.ClipRegion;

			if (ShouldClip)
			{
				render.AddClipRegion(clipRect);

				if (!render.ClipRegionVisible)
				{
					render.RenderOffset = oldRenderOffset;
					render.ClipRegion = oldRegion;
					return;
				}

				render.StartClip();
			}

			//Render myself first
			Render(skin);

			if (m_Children.Count > 0)
			{
				//Now render my kids
				foreach (ControlBase child in m_Children)
				{
					if (child.IsHidden || child.IsCollapsed)
						continue;
					child.DoRender(skin);
				}
			}

			render.ClipRegion = oldRegion;
			render.StartClip();
			RenderOver(skin);

			RenderFocus(skin);

			render.RenderOffset = oldRenderOffset;
		}

		/// <summary>
		/// Sets the control's skin.
		/// </summary>
		/// <param name="skin">New skin.</param>
		/// <param name="doChildren">Deterines whether to change children skin.</param>
		public virtual void SetSkin(Skin.SkinBase skin, bool doChildren = false)
		{
			if (m_Skin == skin)
				return;
			m_Skin = skin;
			//Invalidate();
			Redraw();
			OnSkinChanged(skin);

			if (doChildren)
			{
				foreach (ControlBase child in m_Children)
				{
					child.SetSkin(skin, true);
				}
			}
		}

		/// <summary>
		/// Handler invoked when control's skin changes.
		/// </summary>
		/// <param name="newSkin">New skin.</param>
		protected virtual void OnSkinChanged(Skin.SkinBase newSkin)
		{

		}

		/// <summary>
		/// Handler invoked on mouse wheel event.
		/// </summary>
		/// <param name="delta">Scroll delta.</param>
		protected virtual bool OnMouseWheeled(int delta)
		{
			if (m_ActualParent != null)
				return m_ActualParent.OnMouseWheeled(delta);

			return false;
		}

		/// <summary>
		/// Invokes mouse wheeled event (used by input system).
		/// </summary>
		internal bool InputMouseWheeled(int delta)
		{
			return OnMouseWheeled(delta);
		}

		/// <summary>
		/// Handler invoked on mouse moved event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="dx">X change.</param>
		/// <param name="dy">Y change.</param>
		protected virtual void OnMouseMoved(int x, int y, int dx, int dy)
		{

		}

		/// <summary>
		/// Invokes mouse moved event (used by input system).
		/// </summary>
		internal void InputMouseMoved(int x, int y, int dx, int dy)
		{
			OnMouseMoved(x, y, dx, dy);
		}

		/// <summary>
		/// Handler invoked on mouse click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="down">If set to <c>true</c> mouse button is down.</param>
		protected virtual void OnMouseClickedLeft(int x, int y, bool down)
		{
			if (down && Clicked != null)
				Clicked(this, new ClickedEventArgs(x, y, down));
		}

		/// <summary>
		/// Invokes left mouse click event (used by input system).
		/// </summary>
		internal void InputMouseClickedLeft(int x, int y, bool down)
		{
			OnMouseClickedLeft(x, y, down);
		}

		/// <summary>
		/// Handler invoked on mouse click (right) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="down">If set to <c>true</c> mouse button is down.</param>
		protected virtual void OnMouseClickedRight(int x, int y, bool down)
		{
			if (down && RightClicked != null)
				RightClicked(this, new ClickedEventArgs(x, y, down));
		}

		/// <summary>
		/// Invokes right mouse click event (used by input system).
		/// </summary>
		internal void InputMouseClickedRight(int x, int y, bool down)
		{
			OnMouseClickedRight(x, y, down);
		}

		/// <summary>
		/// Handler invoked on mouse double click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		protected virtual void OnMouseDoubleClickedLeft(int x, int y)
		{
			// [omeg] should this be called?
			// [halfofastaple] Maybe. Technically, a double click is still technically a single click. However, this shouldn't be called here, and
			//					Should be called by the event handler.
			OnMouseClickedLeft(x, y, true);

			if (DoubleClicked != null)
				DoubleClicked(this, new ClickedEventArgs(x, y, true));
		}

		/// <summary>
		/// Invokes left double mouse click event (used by input system).
		/// </summary>
		internal void InputMouseDoubleClickedLeft(int x, int y)
		{
			OnMouseDoubleClickedLeft(x, y);
		}

		/// <summary>
		/// Handler invoked on mouse double click (right) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		protected virtual void OnMouseDoubleClickedRight(int x, int y)
		{
			// [halfofastaple] See: OnMouseDoubleClicked for discussion on triggering single clicks in a double click event
			OnMouseClickedRight(x, y, true);

			if (DoubleRightClicked != null)
				DoubleRightClicked(this, new ClickedEventArgs(x, y, true));
		}

		/// <summary>
		/// Invokes right double mouse click event (used by input system).
		/// </summary>
		internal void InputMouseDoubleClickedRight(int x, int y)
		{
			OnMouseDoubleClickedRight(x, y);
		}

		/// <summary>
		/// Handler invoked on mouse cursor entering control's bounds.
		/// </summary>
		protected virtual void OnMouseEntered()
		{
			if (HoverEnter != null)
				HoverEnter.Invoke(this, EventArgs.Empty);

			if (ToolTip != null)
				Gwen.ToolTip.Enable(this);
			else if (Parent != null && Parent.ToolTip != null)
				Gwen.ToolTip.Enable(Parent);

			Redraw();
		}

		/// <summary>
		/// Invokes mouse enter event (used by input system).
		/// </summary>
		internal void InputMouseEntered()
		{
			OnMouseEntered();
		}

		/// <summary>
		/// Handler invoked on mouse cursor leaving control's bounds.
		/// </summary>
		protected virtual void OnMouseLeft()
		{
			if (HoverLeave != null)
				HoverLeave.Invoke(this, EventArgs.Empty);

			if (ToolTip != null)
				Gwen.ToolTip.Disable(this);

			Redraw();
		}

		/// <summary>
		/// Invokes mouse leave event (used by input system).
		/// </summary>
		internal void InputMouseLeft()
		{
			OnMouseLeft();
		}

		/// <summary>
		/// Focuses the control.
		/// </summary>
		public virtual void Focus()
		{
			if (InputHandler.KeyboardFocus == this)
				return;

			if (InputHandler.KeyboardFocus != null)
				InputHandler.KeyboardFocus.OnLostKeyboardFocus();

			InputHandler.KeyboardFocus = this;
			OnKeyboardFocus();
			Redraw();
		}

		/// <summary>
		/// Unfocuses the control.
		/// </summary>
		public virtual void Blur()
		{
			if (InputHandler.KeyboardFocus != this)
				return;

			InputHandler.KeyboardFocus = null;
			OnLostKeyboardFocus();
			Redraw();
		}

		/// <summary>
		/// Control has been clicked - invoked by input system. Windows use it to propagate activation.
		/// </summary>
		public virtual void Touch()
		{
			if (Parent != null)
				Parent.OnChildTouched(this);
		}

		protected virtual void OnChildTouched(ControlBase control)
		{
			Touch();
		}

		/// <summary>
		/// Gets a child by its coordinates.
		/// </summary>
		/// <param name="x">Child X.</param>
		/// <param name="y">Child Y.</param>
		/// <returns>Control or null if not found.</returns>
		public virtual ControlBase GetControlAt(int x, int y)
		{
			if (IsHidden || IsCollapsed)
				return null;

			if (x < 0 || y < 0 || x >= ActualWidth || y >= ActualHeight)
				return null;

			// todo: convert to linq FindLast
			var rev = ((IList<ControlBase>)m_Children).Reverse(); // IList.Reverse creates new list, List.Reverse works in place.. go figure
			foreach (ControlBase child in rev)
			{
				ControlBase found = child.GetControlAt(x - child.ActualLeft, y - child.ActualTop);
				if (found != null)
					return found;
			}

			if (!MouseInputEnabled)
				return null;

			return this;
		}

		/// <summary>
		/// Override this method if you need to customize the layout process.
		/// </summary>
		/// <param name="availableSize">Available size for the control. The control doesn't need to use all the space that is available.</param>
		/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
		/// <remarks>There is no need to call the base method if you don't need a dock layout implementation.</remarks>
		protected virtual Size OnMeasure(Size availableSize)
		{
			int parentWidth = m_Padding.Left + m_Padding.Right;
			int parentHeight = m_Padding.Top + m_Padding.Bottom;
			int childrenWidth = m_Padding.Left + m_Padding.Right;
			int childrenHeight = m_Padding.Top + m_Padding.Bottom;

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock == Dock.None || dock == Dock.Fill)
					continue;

				Size childSize = new Size(Math.Max(0, availableSize.Width - childrenWidth), Math.Max(0, availableSize.Height - childrenHeight));

				childSize = child.Measure(childSize);

				switch (child.Dock)
				{
					case Dock.Left:
					case Dock.Right:
						parentHeight = Math.Max(parentHeight, childrenHeight + childSize.Height);
						childrenWidth += childSize.Width;
						break;
					case Dock.Top:
					case Dock.Bottom:
						parentWidth = Math.Max(parentWidth, childrenWidth + childSize.Width);
						childrenHeight += childSize.Height;
						break;
				}
			}

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock != Dock.Fill)
					continue;

				Size childSize = new Size(Math.Max(0, availableSize.Width - childrenWidth), Math.Max(0, availableSize.Height - childrenHeight));

				childSize = child.Measure(childSize);

				parentWidth = Math.Max(parentWidth, childrenWidth + childSize.Width);
				parentHeight = Math.Max(parentHeight, childrenHeight + childSize.Height);
			}

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock != Dock.None)
					continue;

				Size childSize = child.Measure(availableSize);

				parentWidth = Math.Max(parentWidth, child.Left + childSize.Width);
				parentHeight = Math.Max(parentHeight, child.Top + childSize.Height);
			}

			parentWidth = Math.Max(parentWidth, childrenWidth);
			parentHeight = Math.Max(parentHeight, childrenHeight);

			return new Size(parentWidth, parentHeight);
		}

		/// <summary>
		/// Call this method for all child controls.
		/// </summary>
		/// <param name="availableWidth">Width that is available for the control.</param>
		/// <param name="availableHeight">Height that is available for the control.</param>
		/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
		public Size Measure(int availableWidth, int availableHeight)
		{
			return Measure(new Size(availableWidth, availableHeight));
		}

		/// <summary>
		/// Call this method for all child controls.
		/// </summary>
		/// <param name="availableSize">Size that is available for the control.</param>
		/// <returns>Minimum size that the control needs to draw itself correctly.</returns>
		public Size Measure(Size availableSize)
		{
			availableSize -= m_Margin;

			if (!Util.IsIgnore(m_DesiredBounds.Width))
				availableSize.Width = Math.Min(availableSize.Width, m_DesiredBounds.Width);
			if (!Util.IsIgnore(m_DesiredBounds.Height))
				availableSize.Height = Math.Min(availableSize.Height, m_DesiredBounds.Height);

			availableSize.Width = Util.Clamp(availableSize.Width, m_MinimumSize.Width, m_MaximumSize.Width);
			availableSize.Height = Util.Clamp(availableSize.Height, m_MinimumSize.Height, m_MaximumSize.Height);

			Size size = OnMeasure(availableSize);
			if (Util.IsInfinity(size.Width) || Util.IsInfinity(size.Height))
				throw new InvalidOperationException("Measured size cannot be infinity.");

			if (!Util.IsIgnore(m_DesiredBounds.Width))
				size.Width = m_DesiredBounds.Width;
			if (!Util.IsIgnore(m_DesiredBounds.Height))
				size.Height = m_DesiredBounds.Height;

			size.Width = Util.Clamp(size.Width, m_MinimumSize.Width, m_MaximumSize.Width);
			size.Height = Util.Clamp(size.Height, m_MinimumSize.Height, m_MaximumSize.Height);

			if (size.Width > availableSize.Width)
				size.Width = availableSize.Width;
			if (size.Height > availableSize.Height)
				size.Height = availableSize.Height;

			size += m_Margin;

			m_MeasuredSize = size;

			return m_MeasuredSize;
		}

		/// <summary>
		/// Override this method if you need to customize the layout process. Usually, if you override Measure, you also need to override Arrange.
		/// </summary>
		/// <param name="finalSize">Space that the control should fill.</param>
		/// <returns>Space that the control filled.</returns>
		/// <remarks>There is no need to call the base method if you don't need a dock layout implementation.</remarks>
		protected virtual Size OnArrange(Size finalSize)
		{
			int childrenLeft = m_Padding.Left;
			int childrenTop = m_Padding.Top;
			int childrenRight = m_Padding.Right;
			int childrenBottom = m_Padding.Bottom;

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock == Dock.None || dock == Dock.Fill)
					continue;

				Size childSize = child.MeasuredSize;
				Rectangle bounds = new Rectangle(childrenLeft, childrenTop, Math.Max(0, finalSize.Width - (childrenLeft + childrenRight)), Math.Max(0, finalSize.Height - (childrenTop + childrenBottom)));

				switch (dock)
				{
					case Dock.Left:
						childrenLeft += childSize.Width;
						bounds.Width = childSize.Width;
						break;
					case Dock.Right:
						childrenRight += childSize.Width;
						bounds.X = Math.Max(0, finalSize.Width - childrenRight);
						bounds.Width = childSize.Width;
						break;
					case Dock.Top:
						childrenTop += childSize.Height;
						bounds.Height = childSize.Height;
						break;
					case Dock.Bottom:
						childrenBottom += childSize.Height;
						bounds.Y = Math.Max(0, finalSize.Height - childrenBottom);
						bounds.Height = childSize.Height;
						break;
				}

				child.Arrange(bounds);
			}

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock != Dock.Fill)
					continue;

				Rectangle bounds = new Rectangle(childrenLeft, childrenTop, Math.Max(0, finalSize.Width - (childrenLeft + childrenRight)), Math.Max(0, finalSize.Height - (childrenTop + childrenBottom)));

				m_InnerBounds = bounds;

				child.Arrange(bounds);
			}

			foreach (ControlBase child in m_Children)
			{
				if (child.IsCollapsed)
					continue;

				Dock dock = child.Dock;

				if (dock != Dock.None)
					continue;

				Size childSize = child.MeasuredSize;
				Rectangle bounds = new Rectangle(child.Left, child.Top, finalSize.Width - child.Left, finalSize.Height - child.Top);

				child.Arrange(bounds);
			}

			return finalSize;
		}

		/// <summary>
		/// Call this method for all child controls.
		/// </summary>
		/// <param name="x">Final horizontal location. This includes margins.</param>
		/// <param name="y">Final vertical location. This includes margins.</param>
		/// <param name="width">Final width of the control. This includes margins.</param>
		/// <param name="height">Final height of the control. This includes margins.</param>
		public void Arrange(int x, int y, int width, int height)
		{
			Arrange(new Rectangle(x, y, width, height));
		}

		/// <summary>
		/// Call this method for all child controls.
		/// </summary>
		/// <param name="finalRect">Final location and size of the control. This includes margins.</param>
		public void Arrange(Rectangle finalRect)
		{
			Size finalSize = finalRect.Size;

			HorizontalAlignment halign = HorizontalAlignment;
			VerticalAlignment valign = VerticalAlignment;

			if (halign != HorizontalAlignment.Stretch)
				finalSize.Width = m_MeasuredSize.Width;
			if (valign != VerticalAlignment.Stretch)
				finalSize.Height = m_MeasuredSize.Height;

			finalSize -= m_Margin;

			if (!Util.IsIgnore(m_DesiredBounds.Width))
				finalSize.Width = Math.Min(finalRect.Width, m_DesiredBounds.Width);
			if (!Util.IsIgnore(m_DesiredBounds.Height))
				finalSize.Height = Math.Min(finalRect.Height, m_DesiredBounds.Height);

			Size arrangedSize = OnArrange(finalSize);

			if (!Util.IsIgnore(m_DesiredBounds.Width))
				arrangedSize.Width = m_DesiredBounds.Width;
			else if (halign == HorizontalAlignment.Stretch)
				arrangedSize.Width = finalSize.Width;

			if (!Util.IsIgnore(m_DesiredBounds.Height))
				arrangedSize.Height = m_DesiredBounds.Height;
			else if (valign == VerticalAlignment.Stretch)
				arrangedSize.Height = finalSize.Height;

			arrangedSize.Width = Util.Clamp(arrangedSize.Width, m_MinimumSize.Width, m_MaximumSize.Width);
			arrangedSize.Height = Util.Clamp(arrangedSize.Height, m_MinimumSize.Height, m_MaximumSize.Height);

			if (arrangedSize.Width > finalSize.Width)
				arrangedSize.Width = finalSize.Width;
			if (arrangedSize.Height > finalSize.Height)
				arrangedSize.Height = finalSize.Height;

			Size areaSize = finalRect.Size;
			areaSize -= m_Margin;

			Point offset = Point.Zero;
			if (halign == HorizontalAlignment.Center)
				offset.X = (areaSize.Width - arrangedSize.Width) / 2;
			else if (halign == HorizontalAlignment.Right)
				offset.X = areaSize.Width - arrangedSize.Width;
			if (valign == VerticalAlignment.Center)
				offset.Y = (areaSize.Height - arrangedSize.Height) / 2;
			else if (valign == VerticalAlignment.Bottom)
				offset.Y = areaSize.Height - arrangedSize.Height;

			SetBounds(finalRect.Left + m_Margin.Left + offset.X, finalRect.Top + m_Margin.Top + offset.Y, arrangedSize.Width, arrangedSize.Height);

			NeedsLayout = false;
			LayoutDone = true;
		}

		/// <summary>
		/// Invoke the layout process for the control and it's children.
		/// </summary>
		public virtual void DoLayout()
		{
			OnMeasure(m_Bounds.Size);
			OnArrange(m_Bounds.Size);

			NeedsLayout = false;
			LayoutDone = true;
		}

		/// <summary>
		/// Recursively check tabs, focus etc.
		/// </summary>
		protected virtual void RecurseControls()
		{
			if (IsHidden || IsCollapsed)
				return;

			foreach (ControlBase child in Children)
			{
				child.RecurseControls();
			}

			if (IsTabable)
			{
				if (GetCanvas().FirstTab == null)
					GetCanvas().FirstTab = this;
				if (GetCanvas().NextTab == null)
					GetCanvas().NextTab = this;
			}

			if (InputHandler.KeyboardFocus == this)
			{
				GetCanvas().NextTab = null;
			}
		}

		/// <summary>
		/// Checks if the given control is a child of this instance.
		/// </summary>
		/// <param name="child">Control to examine.</param>
		/// <returns>True if the control is our child.</returns>
		public bool IsChild(ControlBase child)
		{
			return m_Children.Contains(child);
		}

		/// <summary>
		/// Converts local coordinates to canvas coordinates.
		/// </summary>
		/// <param name="pnt">Local coordinates.</param>
		/// <returns>Canvas coordinates.</returns>
		public virtual Point LocalPosToCanvas(Point pnt)
		{
			if (m_ActualParent != null)
			{
				int x = pnt.X + ActualLeft;
				int y = pnt.Y + ActualTop;

				return m_ActualParent.LocalPosToCanvas(new Point(x, y));
			}

			return pnt;
		}

		/// <summary>
		/// Converts canvas coordinates to local coordinates.
		/// </summary>
		/// <param name="pnt">Canvas coordinates.</param>
		/// <returns>Local coordinates.</returns>
		public virtual Point CanvasPosToLocal(Point pnt)
		{
			if (m_ActualParent != null)
			{
				int x = pnt.X - ActualLeft;
				int y = pnt.Y - ActualTop;

				return m_ActualParent.CanvasPosToLocal(new Point(x, y));
			}

			return pnt;
		}

		/// <summary>
		/// Closes all menus recursively.
		/// </summary>
		public virtual void CloseMenus()
		{
			// todo: not very efficient with the copying and recursive closing, maybe store currently open menus somewhere (canvas)?
			var childrenCopy = m_Children.ToArray();
			foreach (ControlBase child in childrenCopy)
			{
				child.CloseMenus();
			}
		}

		/// <summary>
		/// Copies Bounds to RenderBounds.
		/// </summary>
		protected virtual void UpdateRenderBounds()
		{
			m_RenderBounds.X = 0;
			m_RenderBounds.Y = 0;

			m_RenderBounds.Width = m_Bounds.Width;
			m_RenderBounds.Height = m_Bounds.Height;
		}

		/// <summary>
		/// Sets mouse cursor to current cursor.
		/// </summary>
		public virtual void UpdateCursor()
		{
			Platform.Platform.SetCursor(m_Cursor);
		}

		// giver
		public virtual Package DragAndDrop_GetPackage(int x, int y)
		{
			return m_DragAndDrop_Package;
		}

		// giver
		public virtual bool DragAndDrop_Draggable()
		{
			if (m_DragAndDrop_Package == null)
				return false;

			return m_DragAndDrop_Package.IsDraggable;
		}

		// giver
		public virtual void DragAndDrop_SetPackage(bool draggable, string name = "", object userData = null)
		{
			if (m_DragAndDrop_Package == null)
			{
				m_DragAndDrop_Package = new Package();
				m_DragAndDrop_Package.IsDraggable = draggable;
				m_DragAndDrop_Package.Name = name;
				m_DragAndDrop_Package.UserData = userData;
			}
		}

		// giver
		public virtual bool DragAndDrop_ShouldStartDrag()
		{
			return true;
		}

		// giver
		public virtual void DragAndDrop_StartDragging(Package package, int x, int y)
		{
			package.HoldOffset = CanvasPosToLocal(new Point(x, y));
			package.DrawControl = this;
		}

		// giver
		public virtual void DragAndDrop_EndDragging(bool success, int x, int y)
		{
		}

		// receiver
		public virtual bool DragAndDrop_HandleDrop(Package p, int x, int y)
		{
			DragAndDrop.SourceControl.Parent = this;
			return true;
		}

		// receiver
		public virtual void DragAndDrop_HoverEnter(Package p, int x, int y)
		{

		}

		// receiver
		public virtual void DragAndDrop_HoverLeave(Package p)
		{

		}

		// receiver
		public virtual void DragAndDrop_Hover(Package p, int x, int y)
		{

		}

		// receiver
		public virtual bool DragAndDrop_CanAcceptPackage(Package p)
		{
			return false;
		}

		/// <summary>
		/// Handles keyboard accelerator.
		/// </summary>
		/// <param name="accelerator">Accelerator text.</param>
		/// <returns>True if handled.</returns>
		internal virtual bool HandleAccelerator(string accelerator)
		{
			if (InputHandler.KeyboardFocus == this || !AccelOnlyFocus)
			{
				if (m_Accelerators.ContainsKey(accelerator))
				{
					m_Accelerators[accelerator].Invoke(this, EventArgs.Empty);
					return true;
				}
			}

			return m_Children.Any(child => child.HandleAccelerator(accelerator));
		}

		/// <summary>
		/// Adds keyboard accelerator.
		/// </summary>
		/// <param name="accelerator">Accelerator text.</param>
		/// <param name="handler">Handler.</param>
		public void AddAccelerator(string accelerator, GwenEventHandler<EventArgs> handler)
		{
			accelerator = accelerator.Trim().ToUpperInvariant();
			m_Accelerators[accelerator] = handler;
		}

		/// <summary>
		/// Adds keyboard accelerator with a default handler.
		/// </summary>
		/// <param name="accelerator">Accelerator text.</param>
		public void AddAccelerator(string accelerator)
		{
			m_Accelerators[accelerator] = DefaultAcceleratorHandler;
		}

		/// <summary>
		/// Removes keyboard accelerator.
		/// </summary>
		/// <param name="accelerator">Accelerator text.</param>
		public void RemoveAccelerator(string accelerator)
		{
			m_Accelerators.Remove(accelerator);
		}

		/// <summary>
		/// Re-renders the control, invalidates cached texture.
		/// </summary>
		public virtual void Redraw()
		{
			UpdateColors();
			m_CacheTextureDirty = true;
			if (m_Parent != null)
				m_Parent.Redraw();
		}

		/// <summary>
		/// Updates control colors.
		/// </summary>
		/// <remarks>
		/// Used in composite controls like lists to differentiate row colors etc.
		/// </remarks>
		public virtual void UpdateColors()
		{

		}

		/// <summary>
		/// Handler for keyboard events.
		/// </summary>
		/// <param name="key">Key pressed.</param>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyPressed(Key key, bool down = true)
		{
			bool handled = false;
			switch (key)
			{
				case Key.Tab: handled = OnKeyTab(down); break;
				case Key.Space: handled = OnKeySpace(down); break;
				case Key.Home: handled = OnKeyHome(down); break;
				case Key.End: handled = OnKeyEnd(down); break;
				case Key.Return: handled = OnKeyReturn(down); break;
				case Key.Backspace: handled = OnKeyBackspace(down); break;
				case Key.Delete: handled = OnKeyDelete(down); break;
				case Key.Right: handled = OnKeyRight(down); break;
				case Key.Left: handled = OnKeyLeft(down); break;
				case Key.Up: handled = OnKeyUp(down); break;
				case Key.Down: handled = OnKeyDown(down); break;
				case Key.Escape: handled = OnKeyEscape(down); break;
				default: break;
			}

			if (!handled && Parent != null)
				Parent.OnKeyPressed(key, down);

			return handled;
		}

		/// <summary>
		/// Invokes key press event (used by input system).
		/// </summary>
		internal bool InputKeyPressed(Key key, bool down = true)
		{
			return OnKeyPressed(key, down);
		}

		/// <summary>
		/// Handler for keyboard events.
		/// </summary>
		/// <param name="key">Key pressed.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyReleaseed(Key key)
		{
			return OnKeyPressed(key, false);
		}

		/// <summary>
		/// Handler for Tab keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyTab(bool down)
		{
			if (!down)
				return true;

			if (GetCanvas().NextTab != null)
			{
				GetCanvas().NextTab.Focus();
				Redraw();
			}

			return true;
		}

		/// <summary>
		/// Handler for Space keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeySpace(bool down) { return false; }

		/// <summary>
		/// Handler for Return keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyReturn(bool down) { return false; }

		/// <summary>
		/// Handler for Backspace keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyBackspace(bool down) { return false; }

		/// <summary>
		/// Handler for Delete keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyDelete(bool down) { return false; }

		/// <summary>
		/// Handler for Right Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyRight(bool down) { return false; }

		/// <summary>
		/// Handler for Left Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyLeft(bool down) { return false; }

		/// <summary>
		/// Handler for Home keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyHome(bool down) { return false; }

		/// <summary>
		/// Handler for End keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyEnd(bool down) { return false; }

		/// <summary>
		/// Handler for Up Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyUp(bool down) { return false; }

		/// <summary>
		/// Handler for Down Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyDown(bool down) { return false; }

		/// <summary>
		/// Handler for Escape keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnKeyEscape(bool down) { return false; }

		/// <summary>
		/// Handler for Paste event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected virtual void OnPaste(ControlBase from, EventArgs args)
		{
		}

		/// <summary>
		/// Handler for Copy event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected virtual void OnCopy(ControlBase from, EventArgs args)
		{
		}

		/// <summary>
		/// Handler for Cut event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected virtual void OnCut(ControlBase from, EventArgs args)
		{
		}

		/// <summary>
		/// Handler for Select All event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected virtual void OnSelectAll(ControlBase from, EventArgs args)
		{
		}

		internal void InputCopy(ControlBase from)
		{
			OnCopy(from, EventArgs.Empty);
		}

		internal void InputPaste(ControlBase from)
		{
			OnPaste(from, EventArgs.Empty);
		}

		internal void InputCut(ControlBase from)
		{
			OnCut(from, EventArgs.Empty);
		}

		internal void InputSelectAll(ControlBase from)
		{
			OnSelectAll(from, EventArgs.Empty);
		}

		/// <summary>
		/// Renders the focus overlay.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected virtual void RenderFocus(Skin.SkinBase skin)
		{
			if (InputHandler.KeyboardFocus != this)
				return;
			if (!IsTabable)
				return;

			skin.DrawKeyboardHighlight(this, RenderBounds, 3);
		}

		/// <summary>
		/// Renders under the actual control (shadows etc).
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected virtual void RenderUnder(Skin.SkinBase skin)
		{

		}

		/// <summary>
		/// Renders over the actual control (overlays).
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected virtual void RenderOver(Skin.SkinBase skin)
		{

		}

		/// <summary>
		/// Called during rendering.
		/// </summary>
		public virtual void Think()
		{

		}

		/// <summary>
		/// Handler for gaining keyboard focus.
		/// </summary>
		protected virtual void OnKeyboardFocus()
		{

		}

		/// <summary>
		/// Handler for losing keyboard focus.
		/// </summary>
		protected virtual void OnLostKeyboardFocus()
		{

		}

		/// <summary>
		/// Handler for character input event.
		/// </summary>
		/// <param name="chr">Character typed.</param>
		/// <returns>True if handled.</returns>
		protected virtual bool OnChar(Char chr)
		{
			return false;
		}

		internal bool InputChar(Char chr)
		{
			return OnChar(chr);
		}

#if false
		public virtual void Anim_WidthIn(float length, float delay = 0.0f, float ease = 1.0f)
		{
			Animation.Add(this, new Anim.Size.Width(0, ActualWidth, length, false, delay, ease));
			ActualWidth = 0;
		}

		public virtual void Anim_HeightIn(float length, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Height(0, ActualHeight, length, false, delay, ease));
			ActualHeight = 0;
		}

		public virtual void Anim_WidthOut(float length, bool hide, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Width(ActualWidth, 0, length, hide, delay, ease));
		}

		public virtual void Anim_HeightOut(float length, bool hide, float delay, float ease)
		{
			Animation.Add(this, new Anim.Size.Height(ActualHeight, 0, length, hide, delay, ease));
		}
#endif

		private InternalFlags m_InternalFlags;

		private bool IsSetInternalFlag(InternalFlags flag)
		{
			return (m_InternalFlags & flag) != 0;
		}

		private InternalFlags GetInternalFlag(InternalFlags mask)
		{
			return m_InternalFlags & mask;
		}

		private void SetInternalFlag(InternalFlags flag, bool value)
		{
			if (value)
				m_InternalFlags |= flag;
			else
				m_InternalFlags &= ~flag;
		}

		private bool CheckAndChangeInternalFlag(InternalFlags flag, bool value)
		{
			bool oldValue = (m_InternalFlags & flag) != 0;
			if (oldValue == value)
				return false;

			if (value)
				m_InternalFlags |= flag;
			else
				m_InternalFlags &= ~flag;

			return true;
		}

		private void SetInternalFlag(InternalFlags mask, InternalFlags flag)
		{
			m_InternalFlags = (m_InternalFlags & ~mask) | flag;
		}

		private bool CheckAndChangeInternalFlag(InternalFlags mask, InternalFlags flag)
		{
			if ((m_InternalFlags & mask) == flag)
				return false;

			m_InternalFlags = (m_InternalFlags & ~mask) | flag;

			return true;
		}

		[Flags]
		internal enum InternalFlags
		{
			// AlignH
			AlignHLeft			= 1 << 0,
			AlignHCenter		= 1 << 1,
			AlignHRight			= 1 << 2,
			AlignHStretch		= 1 << 3,
			AlignH_Mask			= AlignHLeft | AlignHCenter | AlignHRight | AlignHStretch,

			// AlignV
			AlignVTop			= 1 << 4,
			AlignVCenter		= 1 << 5,
			AlignVBottom		= 1 << 6,
			AlignVStretch		= 1 << 7,
			AlignV_Mask			= AlignVTop | AlignVCenter | AlignVBottom | AlignVStretch,

			// Dock
			DockNone			= 1 << 8,
			DockLeft			= 1 << 9,
			DockTop				= 1 << 10,
			DockRight			= 1 << 11,
			DockBottom			= 1 << 12,
			DockFill			= 1 << 13,
			Dock_Mask			= DockNone | DockLeft | DockTop | DockRight | DockBottom | DockFill,

			// Flags
			VirtualControl		= 1 << 14,
			NeedsLayout			= 1 << 15,
			LayoutDone			= 1 << 16,
			Disabled			= 1 << 17,
			Hidden				= 1 << 18,
			Collapsed			= 1 << 19,
			DrawDebugOutlines	= 1 << 20,
			RestrictToParent	= 1 << 21,
			MouseInputEnabled	= 1 << 22,
			KeyboardInputEnabled= 1 << 23,
			DrawBackground		= 1 << 24,
			Tabable				= 1 << 25,
			KeyboardNeeded		= 1 << 26,
			Dirty				= 1 << 27,
		}
	}
}
