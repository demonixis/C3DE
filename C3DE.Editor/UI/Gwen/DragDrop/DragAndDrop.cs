using System;
using Gwen.Control;
using Gwen.Input;

namespace Gwen.DragDrop
{
    /// <summary>
    /// Drag and drop handling.
    /// </summary>
    public static class DragAndDrop
    {
        public static Package CurrentPackage;
        public static ControlBase HoveredControl;
        public static ControlBase SourceControl;

        private static ControlBase m_LastPressedControl;
        private static ControlBase m_NewHoveredControl;
        private static Point m_LastPressedPos;
        private static int m_MouseX;
        private static int m_MouseY;

        private static bool OnDrop(int x, int y)
        {
            bool success = false;

            if (HoveredControl != null)
            {
                HoveredControl.DragAndDrop_HoverLeave(CurrentPackage);
                success = HoveredControl.DragAndDrop_HandleDrop(CurrentPackage, x, y);
            }

            // Report back to the source control, to tell it if we've been successful.
            SourceControl.DragAndDrop_EndDragging(success, x, y);

            CurrentPackage = null;
            SourceControl = null;
			HoveredControl = null;

			return true;
        }

        private static bool ShouldStartDraggingControl( int x, int y )
        {
            // We're not holding a control down..
            if (m_LastPressedControl == null) 
                return false;

            // Not been dragged far enough
            int length = Math.Abs(x - m_LastPressedPos.X) + Math.Abs(y - m_LastPressedPos.Y);
            if (length < 5) 
                return false;

            // Create the dragging package

            CurrentPackage = m_LastPressedControl.DragAndDrop_GetPackage(m_LastPressedPos.X, m_LastPressedPos.Y);

            // We didn't create a package!
            if (CurrentPackage == null)
            {
                m_LastPressedControl = null;
                SourceControl = null;
				HoveredControl = null;
				return false;
            }

            // Now we're dragging something!
            SourceControl = m_LastPressedControl;
            InputHandler.MouseFocus = null;
            m_LastPressedControl = null;
            CurrentPackage.DrawControl = null;

            // Some controls will want to decide whether they should be dragged at that moment.
            // This function is for them (it defaults to true)
            if (!SourceControl.DragAndDrop_ShouldStartDrag())
            {
                SourceControl = null;
                CurrentPackage = null;
				HoveredControl = null;
				return false;
            }

            SourceControl.DragAndDrop_StartDragging(CurrentPackage, m_LastPressedPos.X, m_LastPressedPos.Y);

            return true;
        }

        private static void UpdateHoveredControl(ControlBase control, int x, int y)
        {
            //
            // We use this global variable to represent our hovered control
            // That way, if the new hovered control gets deleted in one of the
            // Hover callbacks, we won't be left with a hanging pointer.
            // This isn't ideal - but it's minimal.
            //
            m_NewHoveredControl = control;

			// Check to see if the new potential control can accept this type of package.
			// If not, ignore it and show an error cursor.
			while (m_NewHoveredControl != null && !m_NewHoveredControl.DragAndDrop_CanAcceptPackage(CurrentPackage))
			{
				// We can't drop on this control, so lets try to drop
				// onto its parent..
				m_NewHoveredControl = m_NewHoveredControl.Parent;

				// Its parents are dead. We can't drop it here.
				// Show the NO WAY cursor.
				if (m_NewHoveredControl == null)
				{
					Platform.Platform.SetCursor(Cursor.No);
				}
			}

			// Nothing to change..
			if (HoveredControl == m_NewHoveredControl)
                return;

			// We changed - tell the old hovered control that it's no longer hovered.
			if (HoveredControl != null)
                HoveredControl.DragAndDrop_HoverLeave(CurrentPackage);

            // If we're hovering where the control came from, just forget it.
            // By changing it to null here we're not going to show any error cursors
            // it will just do nothing if you drop it.
            if (m_NewHoveredControl == SourceControl)
                m_NewHoveredControl = null;

            // Become out new hovered control
            HoveredControl = m_NewHoveredControl;

            // If we exist, tell us that we've started hovering.
            if (HoveredControl != null)
            {
                HoveredControl.DragAndDrop_HoverEnter(CurrentPackage, x, y);
            }

            m_NewHoveredControl = null;
        }

        public static bool Start(ControlBase control, Package package)
        {
            if (CurrentPackage != null)
            {
                return false;
            }

            CurrentPackage = package;
            SourceControl = control;
            return true;
        }

        public static bool OnMouseButton(ControlBase hoveredControl, int x, int y, bool down)
        {
            if (!down)
            {
                m_LastPressedControl = null;

                // Not carrying anything, allow normal actions
                if (CurrentPackage == null)
                    return false;

                // We were carrying something, drop it.
                OnDrop(x, y);
                return true;
            }

            if (hoveredControl == null) 
                return false;
            if (!hoveredControl.DragAndDrop_Draggable()) 
                return false;

            // Store the last clicked on control. Don't do anything yet, 
            // we'll check it in OnMouseMoved, and if it moves further than
            // x pixels with the mouse down, we'll start to drag.
            m_LastPressedPos = new Point(x, y);
            m_LastPressedControl = hoveredControl;

            return false;
        }

        public static void OnMouseMoved(ControlBase hoveredControl, int x, int y)
        {
            // Always keep these up to date, they're used to draw the dragged control.
            m_MouseX = x;
            m_MouseY = y;

            // If we're not carrying anything, then check to see if we should
            // pick up from a control that we're holding down. If not, then forget it.
            if (CurrentPackage == null && !ShouldStartDraggingControl(x, y))
                return;

            // Swap to this new hovered control and notify them of the change.
            UpdateHoveredControl(hoveredControl, x, y);

            if (HoveredControl == null)
                return;

            // Update the hovered control every mouse move, so it can show where
            // the dropped control will land etc..
            HoveredControl.DragAndDrop_Hover(CurrentPackage, x, y);

			// Override the cursor - since it might have been set my underlying controls
			// Ideally this would show the 'being dragged' control. TODO
			Platform.Platform.SetCursor(Cursor.Normal);

            hoveredControl.Redraw();
        }

        public static void RenderOverlay(Canvas canvas, Skin.SkinBase skin)
        {
            if (CurrentPackage == null) 
                return;
            if (CurrentPackage.DrawControl == null) 
                return;

            Point old = skin.Renderer.RenderOffset;

            skin.Renderer.AddRenderOffset(new Rectangle(
                m_MouseX - SourceControl.ActualLeft - CurrentPackage.HoldOffset.X,
                m_MouseY - SourceControl.ActualTop - CurrentPackage.HoldOffset.Y, 0, 0));
            CurrentPackage.DrawControl.DoRender(skin);

            skin.Renderer.RenderOffset = old;
        }

        public static void ControlDeleted(ControlBase control)
        {
            if (SourceControl == control)
            {
                SourceControl = null;
                CurrentPackage = null;
                HoveredControl = null;
                m_LastPressedControl = null;
            }

            if (m_LastPressedControl == control)
                m_LastPressedControl = null;

            if (HoveredControl == control)
                HoveredControl = null;

            if (m_NewHoveredControl == control)
                m_NewHoveredControl = null;
        }
    }
}
