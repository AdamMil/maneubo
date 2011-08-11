using System.Drawing;
using System.Windows.Forms;

namespace Maneubo
{
  delegate void MouseDragEventHandler(object sender, MouseDragEventArgs e);

  #region MouseDragEventArgs
  sealed class MouseDragEventArgs : MouseEventArgs
  {
    public MouseDragEventArgs(MouseButtons button, Point originalPoint, int x, int y, int offsetX, int offsetY) : base(button, 1, x, y, 0)
    {
      start  = originalPoint;
      offset = new Size(offsetX, offsetY);
    }

    public Point Start
    {
      get { return start; }
    }

    public Size Offset
    {
      get { return offset; }
    }

    Point start;
    Size offset;
  }
  #endregion

  #region MouseCanvas
  class MouseCanvas : Control
  {
    public MouseCanvas()
    {
      SetStyle(ControlStyles.Selectable, true);
      SetStyle(ControlStyles.ContainerControl | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick | ControlStyles.UserMouse,
               false);
    }

    public event MouseEventHandler MouseDragStart;
    public event MouseDragEventHandler MouseDrag;
    public event MouseDragEventHandler MouseDragEnd;

    public void CancelMouseDrag()
    {
      if(dragButton != MouseButtons.None)
      {
        mouseDownPos[ButtonToIndex(dragButton)] = new Point(-1, -1); // mark the drag button as released
        dragButton = MouseButtons.None; // clear the dragging flag
        Capture = false; // stop capturing the mouse
      }
    }

    protected virtual void OnMouseDragStart(MouseEventArgs e)
    {
      if(MouseDragStart != null) MouseDragStart(this, e);
    }

    protected virtual void OnMouseDrag(MouseDragEventArgs e)
    {
      if(MouseDrag != null) MouseDrag(this, e);
    }

    protected virtual void OnMouseDragEnd(MouseDragEventArgs e)
    {
      if(MouseDragEnd != null) MouseDragEnd(this, e);
    }

    /* use low-level mouse events to implement higher-level click and drag events */
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);

      int button = ButtonToIndex(e.Button);
      if(button == -1) return; // ignore unsupported buttons
      // when a mouse button is pressed, mark the location. this serves as both an indication that the button is pressed
      // and stores the beginning of the drag, if the user drags the mouse
      mouseDownPos[button] = e.Location;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      if(dragButton != MouseButtons.None) // if we're currently dragging, fire a drag event
      {
        int xd = e.X-lastDragPos.X, yd = e.Y-lastDragPos.Y;
        if(xd == 0 && yd == 0) return;

        OnMouseDrag(new MouseDragEventArgs(dragButton, mouseDownPos[ButtonToIndex(dragButton)], e.X, e.Y, xd, yd));
        // update the last drag point so we can send a delta to OnMouseDrag()
        lastDragPos = e.Location;
      }
      else // otherwise, see if we should start dragging.
      {
        int button = ButtonToIndex(e.Button);
        if(button == -1 || !IsMouseDown(button)) return; // ignore unsupported buttons

        int xd = e.X-mouseDownPos[button].X, yd = e.Y-mouseDownPos[button].Y;
        int dist = xd*xd + yd*yd; // the squared distance
        if(dist >= 16) // if the mouse is moved four pixels or more, start a drag event
        {
          dragButton = e.Button;
          lastDragPos = e.Location;

          // issue a drag start using the stored location of where the mouse was originally pressed
          OnMouseDragStart(new MouseEventArgs(e.Button, e.Clicks, mouseDownPos[button].X, mouseDownPos[button].Y, e.Delta));

          if(dragButton != MouseButtons.None) // if the drag wasn't immediately cancelled
          {
            Capture = true; // capture the mouse so we can be sure to receive the end of the drag
            // then issue a drag event because the mouse has since moved. always specify the original drag button.
            OnMouseDrag(new MouseDragEventArgs(dragButton, mouseDownPos[ButtonToIndex(dragButton)], e.X, e.Y, xd, yd));
          }
        }
      }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);

      int button = ButtonToIndex(e.Button);
      if(button == -1) return; // ignore unsupported buttons

      if(dragButton == e.Button) // if we're currently dragging, end the drag
      {
        OnMouseDragEnd(new MouseDragEventArgs(dragButton, mouseDownPos[ButtonToIndex(dragButton)], e.X, e.Y, 0, 0)); // specify the original drag button
        dragButton = MouseButtons.None; // clear our drag button flag
        Capture = false; // stop capturing the mouse so other things can use it
      }
      else if(IsMouseDown(button)) // otherwise we're not currently dragging. was the button pressed over the control?
      {
        OnMouseClick(e); // yes, so now that it's been released, signal a click event.
      }

      mouseDownPos[button] = new Point(-1, -1); // in any case, mark the button as released.
    }

    bool IsMouseDown(int index) { return mouseDownPos[index].X >= 0; }

    Point[] mouseDownPos = new Point[3] { new Point(-1, -1), new Point(-1, -1), new Point(-1, -1) };
    Point lastDragPos;
    MouseButtons dragButton = MouseButtons.None;

    static int ButtonToIndex(MouseButtons button)
    {
      return button == MouseButtons.Left ? 0 : button == MouseButtons.Middle ? 1 : button == MouseButtons.Right ? 2 : -1;
    }
  }
  #endregion
}