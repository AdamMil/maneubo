using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AdamMil.Collections;
using AdamMil.Mathematics.Geometry;
using AdamMil.Utilities;
using BoardPoint = AdamMil.Mathematics.Geometry.Point2;
using BoardRect  = AdamMil.Mathematics.Geometry.Rectangle;
using SysPoint   = System.Drawing.Point;
using SysRect    = System.Drawing.Rectangle;

namespace Maneubo
{
  #region Shape
  abstract class Shape
  {
    protected Shape()
    {
      Children = new ChildCollection(this);
    }

    #region ChildCollection
    public sealed class ChildCollection : ValidatedCollection<Shape>
    {
      public ChildCollection(Shape parent)
      {
        this.parent = parent;
      }

      protected override void ClearItems()
      {
        foreach(Shape shape in this)
        {
          shape.Parent = null;
          shape.SetBoard(null);
        }
        base.ClearItems();
      }

      protected override void InsertItem(int index, Shape item)
      {
        base.InsertItem(index, item);
        item.Parent = parent;
        item.SetBoard(parent.Board);
      }

      protected override void RemoveItem(int index, Shape item)
      {
        base.RemoveItem(index, item);
        item.Parent = null;
        item.SetBoard(null);
      }

      protected override void SetItem(int index, Shape item)
      {
        Shape oldShape = this[index];
        if(oldShape != item)
        {
          base.SetItem(index, item);
          oldShape.Parent = null;
          oldShape.SetBoard(null);
          item.Parent = parent;
          item.SetBoard(parent.Board);
        }
      }

      protected override void ValidateItem(Shape item, int index)
      {
        if(item.Board != null) throw new ArgumentException("This shape already belongs to a maneuvering board.");
      }

      internal void Swap(int a, int b)
      {
        Shape temp = Items[a];
        Items[a] = Items[b];
        Items[b] = temp;
      }

      readonly Shape parent;
    }
    #endregion

    public ManeuveringBoard Board { get; private set; }
    public ChildCollection Children { get; private set; }
    public string Name { get; set; }
    public Shape Parent { get; private set; }
    public abstract BoardPoint Position { get; set; }

    public IEnumerable<Shape> EnumerateAncestors()
    {
      for(Shape ancestor=Parent; ancestor != null; ancestor = ancestor.Parent) yield return ancestor;
    }

    public IEnumerable<Shape> EnumerateDescendants()
    {
      foreach(Shape child in Children)
      {
        yield return child;
        foreach(Shape descendant in child.EnumerateDescendants()) yield return descendant;
      }
    }

    public abstract double GetSelectionDistance(SysPoint point);

    public bool IsAncestorOf(Shape otherShape)
    {
      return IsAncestorOf(otherShape, true);
    }

    public bool IsAncestorOf(Shape otherShape, bool includeSelf)
    {
      if(otherShape == null) throw new ArgumentNullException();
      if(includeSelf && otherShape == this) return true;
      for(Shape ancestor=otherShape.Parent; ancestor != null; ancestor = ancestor.Parent)
      {
        if(ancestor == this) return true;
      }
      return false;
    }

    public abstract void Render(Graphics graphics);

    protected Brush Brush
    {
      get
      {
        return this == Board.ReferenceShape ? Board.referenceBrush : this == Board.SelectedShape ? Board.selectedBrush : Board.normalBrush;
      }
    }

    protected Pen Pen
    {
      get { return this == Board.ReferenceShape ? Board.referencePen : this == Board.SelectedShape ? Board.selectedPen : Board.normalPen; }
    }

    protected void CenterText(Graphics graphics, float x, float y, string text)
    {
      if(!string.IsNullOrEmpty(text))
      {
        x -= graphics.MeasureString(text, Board.Font, new SizeF(1000, 1000), StringFormat.GenericTypographic).Width * 0.5f;
        graphics.DrawString(text, Board.Font, Brush, x, y, StringFormat.GenericTypographic);
      }
    }

    protected void RenderName(Graphics graphics, float x, float y)
    {
      CenterText(graphics, x, y, Name);
    }

    internal void SetBoard(ManeuveringBoard board)
    {
      Board = board;
      foreach(Shape descendant in EnumerateDescendants()) descendant.Board = board;
    }
  }
  #endregion

  #region CircleShape
  sealed class CircleShape : Shape
  {
    public override BoardPoint Position { get; set; }

    public double Radius
    {
      get { return _radius; }
      set
      {
        if(value < 0) throw new ArgumentOutOfRangeException();
        _radius = value;
      }
    }

    public override double GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      // get the minimum from either the center point or the circle itself, whichever is smaller
      double distanceFromCenter = boardPoint.DistanceTo(Position), distanceFromEdge = Math.Abs(distanceFromCenter - Radius);
      double threshold = distanceFromCenter < distanceFromEdge ? 5 : 4;
      double distance = Math.Min(distanceFromCenter, distanceFromEdge) * Board.ZoomFactor; // get the smaller distance in pixels
      return distance <= threshold ? distance : double.NaN; // the selection is only valid if it's within a small distance
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor;
      float centerX = (float)Position.X * scale, centerY = -(float)Position.Y * scale;
      graphics.DrawCircle(Pens.Black, centerX, centerY, (float)Radius * scale);

      const float CrossSize = 5;
      graphics.DrawLine(Pen, new PointF(centerX, centerY-CrossSize/2), new PointF(centerX, centerY+CrossSize/2));
      graphics.DrawLine(Pen, new PointF(centerX-CrossSize/2, centerY), new PointF(centerX+CrossSize/2, centerY));

      RenderName(graphics, centerX, centerY+CrossSize/2+2);
    }

    double _radius;
  }
  #endregion

  #region LineShape
  sealed class LineShape : Shape
  {
    public BoardPoint Start { get; set; }
    public BoardPoint End { get; set; }

    public double Length
    {
      get { return Vector.Length; }
    }

    public override BoardPoint Position
    {
      get { return Start + Vector*0.5; }
      set
      {
        Vector2 half = Vector*0.5;
        Start = value - half;
        End   = value + half;
      }
    }

    public Vector2 Vector
    {
      get { return End - Start; }
    }

    public override double GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      double distanceFromEnd = Math.Min(boardPoint.DistanceTo(Start), boardPoint.DistanceTo(End));
      double distanceFromSegment = new Line2(Start, End).SegmentDistanceTo(boardPoint);
      double threshold = distanceFromEnd < distanceFromSegment ? 5 : 4; // allow 5 pixels from an endpoint and 4 from the line itself
      double distance = Math.Min(distanceFromEnd, distanceFromSegment) * Board.ZoomFactor;
      return distance <= threshold ? distance : double.NaN;
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor;
      float startX = (float)Start.X * scale, startY = -(float)Start.Y * scale, endX = (float)End.X * scale, endY = -(float)End.Y * scale;
      graphics.DrawLine(Pen, startX, startY, endX, endY);

      graphics.FillRectangle(Brush, startX-2, startY-2, 4, 4);
      graphics.FillRectangle(Brush, endX-2, endY-2, 4, 4);
    }
  }
  #endregion

  #region UnitShape
  sealed class UnitShape : Shape
  {
    /// <summary>Gets or sets the unit's direction, in radians. This direction may be relative or true, depending on the value of
    /// <see cref="IsMotionRelative"/>. Note that this is a nautical angle and not a Cartesian angle.
    /// </summary>
    public double Direction
    {
      get { return _direction; }
      set
      {
        while(value < 0) value += Math.PI*2;
        while(value >= Math.PI*2) value -= Math.PI*2;
        _direction = value;
      }
    }

    /// <summary>Gets or sets whether the unit's motion (<see cref="Speed"/> and <see cref="Direction"/>) are relative to the
    /// <see cref="Parent"/> unit. If false, the unit's motion is absolute.
    /// </summary>
    public bool IsMotionRelative { get; set; }

    /// <summary>Gets or sets the unit's absolute position.</summary>
    public override BoardPoint Position { get; set; }

    /// <summary>Gets or sets the unit's speed, in meters per second. This speed may be relative or true, depending on the value of
    /// <see cref="IsmotionRelative"/>.
    /// </summary>
    public double Speed
    {
      get { return _speed; }
      set
      {
        if(value < 0) throw new ArgumentOutOfRangeException();
        _speed = value;
      }
    }

    public override double GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      double distance = Math.Max(Math.Abs(Position.X - boardPoint.X), Math.Abs(Position.Y - boardPoint.Y)) * Board.ZoomFactor;
      return distance <= 6 ? distance : double.NaN;
      /*BoardPoint boardPoint = Board.GetBoardPoint(point);
      double distanceFromCenter = boardPoint.DistanceTo(Position) * Board.ZoomFactor;
      double distanceFromVector;
      if(Speed > 0)
      {
        Vector motion = new Vector(0, Speed*360).Rotated(-Direction);
        // use two pixels before the endpoint as the hotspot, since that's closer to the center of mass of the arrow head
        motion.Normalize(motion.Length-2/Board.ZoomFactor);
        distanceFromVector = boardPoint.DistanceTo(Position + motion) * Board.ZoomFactor;
      }
      else
      {
        distanceFromVector = double.MaxValue;
      }*/
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor, x = (float)Position.X * scale, y = -(float)Position.Y * scale;

      // if the unit has a speed, draw the velocity vector. the vector will be drawn with a length equal to the distance traveled in
      // six minutes
      if(Speed > 0)
      {
        Vector2 motion = new Vector2(0, -Speed*scale*360).Rotated(Direction);
        graphics.DrawArrow(Pen, x, y, x+(float)motion.X, y+(float)motion.Y);
      }

      graphics.DrawRectangle(Pen, x-6, y-6, 12, 12);
      RenderName(graphics, x, y+8);
    }

    double _direction, _speed;
  }
  #endregion

  // TODO: about observations:
  // 1. there should be a way to have observations from a towed array connected to a unit
  // 2. it should be possible for the UnitShape to act as a first observation, so it should have a Time field

  #region ObservationType
  enum ObservationType
  {
    Point, BearingLine, Waypoint
  }
  #endregion

  #region Observation
  abstract class Observation : Shape
  {
    public UnitShape Observer { get; set; }
    public TimeSpan Time { get; set; }

    protected new Pen Pen
    {
      get { return this == Board.SelectedShape ? Board.selectedObservationPen : Board.observationPen; }
    }

    protected void RenderTime(Graphics graphics, PointF point)
    {
      RenderTime(graphics, point.X, point.Y);
    }

    protected void RenderTime(Graphics graphics, float x, float y)
    {
      CenterText(graphics, x, y, string.Format("{0}:{1:d2}:{2:d2}", (int)Time.TotalHours, Time.Minutes, Time.Seconds));
    }
  }
  #endregion

  #region BearingObservation
  sealed class BearingObservation : Observation
  {
    public double Bearing;

    public override BoardPoint Position
    {
      get { return Observer.Position + new Vector2(0, 1).Rotated(-Bearing); }
      set { Bearing = ManeuveringBoard.AngleBetween(Observer.Position, value); }
    }

    public override double GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      Line2 bearingLine = new Line2(Observer.Position, new Vector2(0, 1).Rotated(-Bearing));
      double distance = Math.Abs(bearingLine.DistanceTo(boardPoint)) * Board.ZoomFactor;

      // the distance measurement considers the entire infinite line, but we only want to consider the ray starting from the observer, so
      // we'll ignore the portion of the line on the other side of the observer point
      if(distance <= 4 && bearingLine.ClosestPointOnSegment(boardPoint) == bearingLine.Start &&
         boardPoint.DistanceTo(bearingLine.Start)*Board.ZoomFactor > 4)
      {
        return double.NaN;
      }
      else
      {
        return distance <= 4 ? distance : double.NaN;
      }
    }

    public override void Render(Graphics graphics)
    {
      Point2 observerPosition = new BoardPoint(Observer.Position.X*Board.ZoomFactor, -Observer.Position.Y*Board.ZoomFactor);
      Vector2 screenVector = new Vector2(0, -1).Rotated(Bearing);
      Line2 bearingLine = new Line2(observerPosition, screenVector);

      // since the bearing line is infinitely long, we'll test the intersection of the line against all four sides of the clipping
      // rectangle, and draw the line to the furthest intersection
      BoardRect clipRect = new BoardRect(graphics.VisibleClipBounds);
      BoardPoint endPoint = BoardPoint.Invalid;
      double maxDistance = 0;
      for(int i=0; i<4; i++)
      {
        Line2 edge = clipRect.GetEdge(i);
        LineIntersection intersection = bearingLine.GetIntersectionInfo(edge);
        if(intersection.OnSecond)
        {
          double distance = observerPosition.DistanceTo(intersection.Point);
          // find the closest point on the segment to ensure that we're only considering intersections in the forward direction of the
          // vector
          if(distance > maxDistance && bearingLine.ClosestPointOnSegment(intersection.Point) != bearingLine.Start)
          {
            endPoint    = intersection.Point;
            maxDistance = distance;
          }
        }
      }

      if(endPoint.IsValid)
      {
        graphics.DrawLine(Pen, observerPosition.ToPointF(), endPoint.ToPointF());
        RenderTime(graphics, (observerPosition + (endPoint - observerPosition)*0.5).ToPointF());
      }
    }
  }
  #endregion

  #region PointObservation
  sealed class PointObservation : Observation
  {
    public override BoardPoint Position { get; set; }

    public override double GetSelectionDistance(SysPoint point)
    {
      double distance = Board.GetBoardPoint(point).DistanceTo(Position) * Board.ZoomFactor;
      return distance <= 12 ? distance : double.NaN;
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor, x = (float)Position.X * scale, y = -(float)Position.Y * scale;

      // TODO: only draw a line back to the parent unit if the parent unit is functioning as a first observation
      int index = Parent.Children.IndexOf(this) - 1;
      PointObservation previousObservation = index >= 0 ? Parent.Children[index] as PointObservation : null;
      Shape previousShape = previousObservation != null && previousObservation.Observer == Observer ? previousObservation : Parent;
      float prevX = (float)previousShape.Position.X * scale, prevY = -(float)previousShape.Position.Y * scale;
      graphics.DrawArrow(Board.observationPen, prevX, prevY, x, y);

      graphics.DrawCircle(Pen, x, y, 6);
      RenderTime(graphics, x, y+8);
    }
  }
  #endregion

  #region LengthUnit
  enum LengthUnit
  {
    Meter, Kilometer, Foot, Yard, Kiloyard, Mile, NauticalMile
  }
  #endregion

  #region SpeedUnit
  enum SpeedUnit
  {
    MetersPerSecond, KilometersPerHour, MilesPerHour, Knots
  }
  #endregion

  #region UnitSystem
  enum UnitSystem
  {
    NauticalMetric, NauticalImperial, Metric, Imperial
  }
  #endregion

  #region ManeuveringBoard
  sealed class ManeuveringBoard : MouseCanvas
  {
    public ManeuveringBoard()
    {
      AddObservationTool = new AddObservationToolClass(this);
      AddUnitTool = new AddUnitToolClass(this);
      PointerTool = new PointerToolClass(this);

      BackColor = Color.FromArgb(0xad, 0xd8, 0xe6);
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight | ControlStyles.FixedWidth | ControlStyles.Opaque |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.ContainerControl, false);
      RootShapes = new ShapeCollection(this);
      SelectedTool = PointerTool;

      // GDI+ sometimes throws an OutOfMemoryException when drawing dashed lines. it's a known problem, but Microsoft doesn't care.
      // infuriating! >:-[  so we'll distinguish between observations and non-observations with line width instead of dash style.
      // TODO: investigate drawing with GDI rather than GDI+
      normalPen = (Pen)Pens.Black.Clone();
      normalPen.Width = 2;
      selectedPen = (Pen)Pens.Blue.Clone();
      selectedPen.Width = 2;
      referencePen = (Pen)Pens.Red.Clone();
      referencePen.Width = 2;
      observationPen = (Pen)Pens.Gray.Clone();
      selectedObservationPen = (Pen)selectedPen.Clone();

      normalBrush      = new SolidBrush(normalPen.Color);
      selectedBrush    = new SolidBrush(selectedPen.Color);
      referenceBrush   = new SolidBrush(referencePen.Color);
      observationBrush = new SolidBrush(observationPen.Color);
    }

    #region ShapeCollection
    public sealed class ShapeCollection : ValidatedCollection<Shape>
    {
      internal ShapeCollection(ManeuveringBoard board)
      {
        this.board = board;
      }

      protected override void ClearItems()
      {
        foreach(Shape shape in this) shape.SetBoard(null);
        base.ClearItems();
        board.Invalidate();
      }

      protected override void InsertItem(int index, Shape item)
      {
        base.InsertItem(index, item);
        item.SetBoard(board);
        board.Invalidate();
      }

      protected override void RemoveItem(int index, Shape item)
      {
        base.RemoveItem(index, item);
        item.SetBoard(null);
        board.Invalidate();
      }

      protected override void SetItem(int index, Shape item)
      {
        Shape oldShape = this[index];
        if(item != oldShape)
        {
          base.SetItem(index, item);
          oldShape.SetBoard(null);
          item.SetBoard(board);
          board.Invalidate();
        }
      }

      protected override void ValidateItem(Shape item, int index)
      {
        if(item.Board != null) throw new ArgumentException("The shape already belongs to a maneuvering board.");
      }

      readonly ManeuveringBoard board;
    }
    #endregion

    #region Tool
    public abstract class Tool
    {
      protected Tool(ManeuveringBoard board)
      {
        Board = board;
      }

      public virtual void Activate() { }
      public virtual void Deactivate() { }
      public virtual void KeyPress(KeyEventArgs e, bool down) { }
      public virtual bool MouseClick(MouseEventArgs e) { return false; }
      public virtual void MouseMove(MouseEventArgs e) { }
      public virtual bool MouseDragStart(MouseEventArgs e) { return false; }
      public virtual void MouseDrag(MouseDragEventArgs e) { }
      public virtual void MouseDragEnd(MouseDragEventArgs e) { }
      public virtual bool MouseWheel(MouseEventArgs e) { return false; }
      public virtual void RenderDecorations(Graphics graphics) { }

      protected ManeuveringBoard Board { get; private set; }
    }
    #endregion

    #region AddObservationToolClass
    public sealed class AddObservationToolClass : Tool
    {
      public AddObservationToolClass(ManeuveringBoard board) : base(board)
      {
        Type = ObservationType.Point;
      }

      public ObservationType Type { get; set; }

      public override void Activate()
      {
        Board.Invalidate();
      }

      public override void Deactivate()
      {
        Board.Invalidate();
      }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          Observation observation;
          if(Type == ObservationType.Point)
          {
            observation = new PointObservation() { Observer = Board.ReferenceShape, Position = Board.GetBoardPoint(e.Location) };
          }
          else if(Type == ObservationType.BearingLine)
          {
            double bearing = ManeuveringBoard.AngleBetween(Board.ReferenceShape.Position, Board.GetBoardPoint(e.Location));
            observation = new BearingObservation() { Observer = Board.ReferenceShape, Bearing = bearing };
          }
          else
          {
            throw new NotImplementedException();
          }

          // find where to insert the new observation. we want to put it after the selected item to start with, so the lines connecting
          // the observations is most likely to be correct, but anyway EditObservation() will sort them correctly so we don't have to be
          // too careful about order (e.g. we don't have to order by type)
          UnitShape observed = Board.SelectedShape as UnitShape;
          int index;
          if(observed != null) // insert it as the first observation if the unit is selected
          {
            Observation firstObservation =
                observed.Children.OfType<Observation>().Where(o => o.Observer == Board.ReferenceShape).FirstOrDefault();
            index = firstObservation != null ? observed.Children.IndexOf(firstObservation) : observed.Children.Count;
          }
          else // otherwise, insert it after the selected observation
          {
            observed = (UnitShape)Board.SelectedShape.Parent;
            index = observed.Children.IndexOf(Board.SelectedShape) + 1;
          }

          observed.Children.Insert(index, observation);
          if(Board.EditObservation(observation)) Board.SelectedShape = observation;
          else Board.DeleteShape(observation, false);
          return true;
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        InvalidateIfReference();
        if(Board.ReferenceShape != null)
        {
          BoardPoint cursor = Board.GetBoardPoint(Board.PointToClient(Cursor.Position));
          double angle = AngleBetween(Board.ReferenceShape.Position, cursor) * MathConst.RadiansToDegrees;
          Board.StatusText = angle.ToString("f2") + "°, " + Board.GetDistanceString((cursor-Board.ReferenceShape.Position).Length);
        }
      }

      public override void RenderDecorations(Graphics graphics)
      {
        if(Board.ReferenceShape != null)
        {
          // TODO: use a preallocated pen
          using(Pen pen = new Pen(Color.FromArgb(96, Color.Black), 1))
          {
            SysPoint clientPoint = Board.PointToClient(Cursor.Position);
            BoardPoint boardPoint = Board.GetBoardPoint(clientPoint);
            pen.DashStyle = DashStyle.Dash;
            graphics.DrawArrow(pen, Board.GetRenderPoint(Board.ReferenceShape.Position), new PointF(clientPoint.X, clientPoint.Y));
          }
        }
      }

      void InvalidateIfReference()
      {
        if(Board.ReferenceShape != null) Board.Invalidate();
      }
    }
    #endregion

    #region AddUnitToolClass
    public sealed class AddUnitToolClass : Tool
    {
      public AddUnitToolClass(ManeuveringBoard board) : base(board) { }

      public override void Activate()
      {
        InvalidateIfReference();
      }

      public override void Deactivate()
      {
        InvalidateIfReference();
      }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          HashSet<string> names = new HashSet<string>();
          foreach(Shape shape in Board.EnumerateShapes())
          {
            if(!string.IsNullOrEmpty(shape.Name)) names.Add(shape.Name);
          }

          string name = "M1";
          for(int suffix=2; names.Contains(name); suffix++) name = "M" + suffix.ToInvariantString();

          UnitShape unit = new UnitShape() { Name = name, Position = Board.GetBoardPoint(e.Location) };
          Board.RootShapes.Add(unit);
          Board.SelectedShape = unit;
          return true;
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        InvalidateIfReference();
        if(Board.ReferenceShape != null)
        {
          BoardPoint cursor = Board.GetBoardPoint(Board.PointToClient(Cursor.Position));
          double angle = AngleBetween(Board.ReferenceShape.Position, cursor) * MathConst.RadiansToDegrees;
          Board.StatusText = angle.ToString("f2") + "°, " + Board.GetDistanceString((cursor-Board.ReferenceShape.Position).Length);
        }
      }

      public override void RenderDecorations(Graphics graphics)
      {
        if(Board.ReferenceShape != null)
        {
          using(Pen pen = new Pen(Color.FromArgb(96, Color.Black), 1))
          {
            SysPoint clientPoint = Board.PointToClient(Cursor.Position);
            BoardPoint boardPoint = Board.GetBoardPoint(clientPoint);
            pen.DashStyle = DashStyle.Dash;
            graphics.DrawArrow(pen, Board.GetRenderPoint(Board.ReferenceShape.Position), new PointF(clientPoint.X, clientPoint.Y));
          }
        }
      }

      void InvalidateIfReference()
      {
        if(Board.ReferenceShape != null) Board.Invalidate();
      }
    }
    #endregion

    #region PointerToolClass
    public sealed class PointerToolClass : Tool
    {
      public PointerToolClass(ManeuveringBoard board) : base(board) { }
    }
    #endregion

    public event EventHandler SelectionChanged, StatusTextChanged, ToolChanged;

    public string StatusText
    {
      get { return _statusText; }
      private set
      {
        if(value != StatusText)
        {
          _statusText = value;
          OnStatusTextChanged();
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BoardPoint Center
    {
      get { return _center; }
      set
      {
        if(value != Center)
        {
          _center = value;
          Invalidate();
        }
      }
    }

    public UnitShape ReferenceShape
    {
      get { return _referenceShape; }
      set
      {
        if(value != ReferenceShape)
        {
          if(value != null && value.Board != this) throw new ArgumentException("The shape does not belong to this maneuvering board.");
          _referenceShape = value;
          Invalidate();
        }
      }
    }

    public ShapeCollection RootShapes { get; private set; }

    public Shape SelectedShape
    {
      get { return _selectedShape; }
      set
      {
        if(value != SelectedShape)
        {
          if(value != null && value.Board != this) throw new ArgumentException("The shape does not belong to this maneuvering board.");
          _selectedShape = value;
          OnSelectionChanged();
          Invalidate();
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Tool SelectedTool
    {
      get { return _currentTool; }
      set
      {
        if(value != SelectedTool)
        {
          if(value == null) throw new ArgumentNullException();
          if(SelectedTool != null) SelectedTool.Deactivate();
          Cursor = Cursors.Default;
          _currentTool = value;
          SelectedTool.Activate();
          OnToolChanged();
        }
      }
    }

    public UnitSystem UnitSystem
    {
      get { return _unitSystem; }
      set
      {
        if(value != UnitSystem)
        {
          _unitSystem = value;
          Invalidate();
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool WasChanged { get; set; }

    [Browsable(false)]
    public double ZoomFactor
    {
      get { return _zoom; }
      set
      {
        if(value != ZoomFactor)
        {
          if(value <= 0) throw new ArgumentOutOfRangeException();
          _zoom = value;
          Invalidate();
        }
      }
    }

    public IEnumerable<Shape> EnumerateShapes()
    {
      foreach(Shape shape in RootShapes)
      {
        yield return shape;
        foreach(Shape descendant in shape.EnumerateDescendants()) yield return descendant;
      }
    }

    public BoardPoint GetBoardPoint(SysPoint clientPoint)
    {
      return new BoardPoint((clientPoint.X-Width*0.5)/ZoomFactor + Center.X, ((Height*0.5-clientPoint.Y)/ZoomFactor + Center.Y));
    }

    public Vector2 GetBoardSize(Size size)
    {
      return new Vector2(size.Width/ZoomFactor, -size.Height/ZoomFactor);
    }

    public SysPoint GetClientPoint(BoardPoint point)
    {
      PointF renderPoint = GetRenderPoint(point);
      return new SysPoint((int)Math.Round(renderPoint.X), (int)Math.Round(renderPoint.Y));
    }

    public SysRect GetClientRect(BoardRect boardRect)
    {
      boardRect.Offset(-Center.X, -Center.Y); // make it relative to the control center rather than the board center

      // scale it by the zoom factor
      boardRect.X *= ZoomFactor;
      boardRect.Y *= ZoomFactor;
      boardRect.Width  *= ZoomFactor;
      boardRect.Height *= ZoomFactor;

      int x = (int)Math.Floor(boardRect.X), y = (int)Math.Floor(boardRect.Y);
      return new SysRect(x, y, (int)Math.Ceiling(boardRect.Right) - x, (int)Math.Ceiling(boardRect.Bottom) - y);
    }

    public PointF GetRenderPoint(BoardPoint point)
    {
      return new PointF((float)((point.X-Center.X)*ZoomFactor + Width*0.5), (float)(Height*0.5 - (point.Y-Center.Y)*ZoomFactor));
    }

    public Shape GetShapeUnderCursor(SysPoint point)
    {
      List<KeyValuePair<Shape, double>> shapes = new List<KeyValuePair<Shape, double>>();
      foreach(Shape shape in EnumerateShapes())
      {
        double distance = shape.GetSelectionDistance(point);
        if(!double.IsNaN(distance)) shapes.Add(new KeyValuePair<Shape, double>(shape, distance));
      }

      if(shapes.Count == 0)
      {
        return null;
      }
      else
      {
        shapes.Sort((a, b) =>
        {
          // put unit shapes before other types of shapes
          if(a.Key is UnitShape)
          {
            if(!(b.Key is UnitShape)) return -1;
          }
          else if(b.Key is UnitShape) return 1;

          // then sort by distance
          return a.Value.CompareTo(b.Value);
        });
        return shapes[0].Key;
      }
    }

    public readonly AddObservationToolClass AddObservationTool;
    public readonly AddUnitToolClass AddUnitTool;
    public readonly PointerToolClass PointerTool;

    public static double AngleBetween(BoardPoint from, BoardPoint to)
    {
      return SwapBearing(Math2D.AngleBetween(from, to));
    }

    public static double ConvertFromUnit(double length, LengthUnit fromUnit)
    {
      return length * conversionsToMeters[(int)fromUnit];
    }

    public static double ConvertFromUnit(double speed, SpeedUnit fromUnit)
    {
      return speed * conversionsToMPS[(int)fromUnit];
    }

    public static double ConvertToUnit(double meters, LengthUnit toUnit)
    {
      return meters / conversionsToMeters[(int)toUnit];
    }

    public static double ConvertToUnit(double metersPerSecond, SpeedUnit toUnit)
    {
      return metersPerSecond / conversionsToMPS[(int)toUnit];
    }

    public static LengthUnit GetAppropriateLengthUnit(double meters, UnitSystem system)
    {
      LengthUnit unit;
      switch(system)
      {
        case UnitSystem.Imperial:
          unit = meters >= conversionsToMeters[(int)LengthUnit.Mile] ?
              LengthUnit.Mile : meters >= conversionsToMeters[(int)LengthUnit.Yard] ? LengthUnit.Yard : LengthUnit.Foot;
          break;
        case UnitSystem.Metric:
          unit = meters >= 1000 ? LengthUnit.Kilometer : LengthUnit.Meter; break;
        case UnitSystem.NauticalImperial:
          unit = meters >= conversionsToMeters[(int)LengthUnit.NauticalMile]/10 ?
              LengthUnit.NauticalMile : meters >= conversionsToMeters[(int)LengthUnit.Yard] ? LengthUnit.Yard : LengthUnit.Foot;
          break;
        case UnitSystem.NauticalMetric:
          unit = meters >= conversionsToMeters[(int)LengthUnit.NauticalMile]/10 ? LengthUnit.NauticalMile : LengthUnit.Meter;
          break;
        default: unit = LengthUnit.Meter; break;
      }
      return unit;
    }

    public static SpeedUnit GetAppropriateSpeedUnit(double metersPerSecond, UnitSystem system)
    {
      SpeedUnit unit;
      switch(system)
      {
        case UnitSystem.Imperial: unit = SpeedUnit.MilesPerHour; break;
        case UnitSystem.Metric: unit = SpeedUnit.KilometersPerHour; break;
        case UnitSystem.NauticalImperial:
        case UnitSystem.NauticalMetric: unit = SpeedUnit.Knots; break;
        default: unit = SpeedUnit.MetersPerSecond; break;
      }
      return unit;
    }

    public static string GetDistanceString(double meters, LengthUnit unit)
    {
      if(meters < 0) throw new ArgumentOutOfRangeException();
      return GetRoundedString(ConvertToUnit(meters, unit)) + " " + lengthAbbreviations[(int)unit];
    }

    public static string GetDistanceString(double meters, UnitSystem system)
    {
      return GetDistanceString(meters, GetAppropriateLengthUnit(meters, system));
    }

    public static string GetSpeedString(double metersPerSecond, SpeedUnit unit)
    {
      if(metersPerSecond < 0) throw new ArgumentOutOfRangeException();
      return GetRoundedString(ConvertToUnit(metersPerSecond, unit)) + " " + speedAbbreviations[(int)unit];
    }

    public static string GetSpeedString(double metersPerSecond, UnitSystem system)
    {
      return GetSpeedString(metersPerSecond, GetAppropriateSpeedUnit(metersPerSecond, system));
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      Utility.Dispose(ref normalPen);
      Utility.Dispose(ref selectedPen);
      Utility.Dispose(ref referencePen);
      Utility.Dispose(ref observationPen);
      Utility.Dispose(ref selectedObservationPen);
      Utility.Dispose(ref normalBrush);
      Utility.Dispose(ref selectedBrush);
      Utility.Dispose(ref referenceBrush);
      Utility.Dispose(ref observationBrush);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      if(!e.Handled)
      {
        SelectedTool.KeyPress(e, true);
        // TODO: add default key handling here
      }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
      base.OnKeyUp(e);
      if(!e.Handled) SelectedTool.KeyPress(e, false);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);

      if(!SelectedTool.MouseClick(e))
      {
        Shape shape = GetShapeUnderCursor(e.Location);
        if(shape == null)
        {
          if(e.Button == MouseButtons.Left) SelectedShape = null;
        }
        else
        {
          if(e.Button == MouseButtons.Left) // left click sets selected shape
          {
            SelectedShape = shape;
          }
          else if(e.Button == MouseButtons.Middle) // middle click sets reference shape
          {
            UnitShape unit = shape as UnitShape;
            if(unit != null) ReferenceShape = unit;
          }
          else if(e.Button == MouseButtons.Right) // right click opens a context menu
          {
            SelectedShape = shape;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("&Edit shape data", null, (o, ea) => EditShape(shape));
            if(!(shape is Observation) && ReferenceShape != null && shape.Parent != ReferenceShape &&
               !shape.IsAncestorOf(ReferenceShape, true))
            {
              menu.Items.Add("Make &child of reference unit", null, (o, ea) => MakeChildOf(shape, ReferenceShape));
            }
            if(shape is UnitShape && shape != ReferenceShape)
            {
              menu.Items.Add("Make &reference unit", null, (o, ea) => ReferenceShape = (UnitShape)shape);
            }
            menu.Items.Add("-");
            menu.Items.Add("&Delete", null, (o, ea) => DeleteShape(shape, false));
            if(shape.Children.Any(c => !(c is Observation))) // if the shape has children that aren't observations...
            {
              menu.Items.Add("&Delete recursively", null, (o, ea) => DeleteShape(shape, true));
            }
            menu.Show(this, e.Location);
          }
        }
      }
    }

    protected override void OnMouseDragStart(MouseEventArgs e)
    {
      base.OnMouseDragStart(e);

      if(!SelectedTool.MouseDragStart(e)) // if the current tool didn't handle the drag...
      {
        if(e.Button == MouseButtons.Left) // left drag moves the item under the cursor
        {
          Shape shape = GetShapeUnderCursor(e.Location);
          if(shape != null)
          {
            SelectedShape = shape;
            dragStart = shape.Position;
            dragMode  = DragMode.MoveItem;
            return;
          }
        }
        else if(e.Button == MouseButtons.Right) // right drag scrolls the view
        {
          dragStart = Center;
          dragMode  = DragMode.Scroll;
          return;
        }

        // the drag was unhandled, so cancel it
        CancelMouseDrag();
      }
    }

    protected override void OnMouseDrag(MouseDragEventArgs e)
    {
      base.OnMouseDrag(e);

      if(dragMode == DragMode.MoveItem)
      {
        SelectedShape.Position = dragStart + GetBoardSize(new Size(e.X-e.Start.X, e.Y-e.Start.Y));
      }
      else if(dragMode == DragMode.Scroll)
      {
        Center = dragStart; // reset the camera so that the correct mouse movement can be calculated
        Vector2 offset = GetBoardSize(new Size(e.X-e.Start.X, e.Y-e.Start.Y));
        Center -= offset; // move the camera in the opposite direction of the mouse movement
      }
      else
      {
        SelectedTool.MouseDrag(e);
      }
    }

    protected override void OnMouseDragEnd(MouseDragEventArgs e)
    {
      base.OnMouseDragEnd(e);
      if(dragMode != DragMode.None) dragMode = DragMode.None;
      else SelectedTool.MouseDragEnd(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      SelectedTool.MouseMove(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
      base.OnMouseWheel(e);

      if(!SelectedTool.MouseWheel(e))
      {
        int delta = e.Delta / System.Windows.Forms.SystemInformation.MouseWheelScrollDelta;

        // we want to zoom around the mouse cursor so that the point under the cursor maps to the same location both before and after
        BoardPoint point = GetBoardPoint(e.Location);
        while(delta < 0 && ZoomFactor > 1.0/1024) { ZoomFactor *= 0.8; delta++; }
        while(delta > 0 && ZoomFactor < 16) { ZoomFactor *= 1.25; delta--; }
        Center += point - GetBoardPoint(e.Location);
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      using(Brush brush = new SolidBrush(BackColor)) e.Graphics.FillRectangle(brush, e.ClipRectangle);

      e.Graphics.PageUnit           = GraphicsUnit.Pixel;
      e.Graphics.SmoothingMode      = SmoothingMode.HighQuality;
      e.Graphics.CompositingMode    = CompositingMode.SourceOver;
      e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
      e.Graphics.InterpolationMode  = InterpolationMode.Bilinear;

      Matrix originalTransform = e.Graphics.Transform;
      e.Graphics.TranslateTransform(Width*0.5f-(float)(Center.X*ZoomFactor), Height*0.5f+(float)(Center.Y*ZoomFactor));

      foreach(Shape shape in EnumerateShapes()) shape.Render(e.Graphics);
      e.Graphics.Transform = originalTransform;
      SelectedTool.RenderDecorations(e.Graphics);

      // draw the scale bar
      {
        const int DesiredScaleBarWidth = 250; // the desired width of the scale bar in pixels. the actual bar can larger or smaller

        double scaleDistance = DesiredScaleBarWidth/ZoomFactor; // the distance that would be covered by the ideal scale bar in meters
        LengthUnit unit = GetAppropriateLengthUnit(scaleDistance, UnitSystem); // get the appropriate unit for that distance
        scaleDistance /= conversionsToMeters[(int)unit]; // and convert to that unit

        double magnitude = Math.Pow(10, Math.Ceiling(Math.Log10(scaleDistance))); // get the power of ten magnitude of the distance
        // we'll round the distance to the nearest quarter of the magnitude, so if the magnitude is 1000 m, we also get 250, 500, 750 m
        scaleDistance = Math.Round(scaleDistance / magnitude * 4) * 0.25 * magnitude;
        // if it was closest to zero, take the next lowest magnitude. it's possible this could change the most appropriate unit, but
        // we'll ignore that possibility for now
        if(scaleDistance == 0) scaleDistance = magnitude * 0.1;
        scaleDistance *= conversionsToMeters[(int)unit]; // convert back into meters

        // get the actual width of the scale bar and render it
        float scaleWidth = (float)(scaleDistance * ZoomFactor), x = 16, y = Height-Font.Height-16, xInc = scaleWidth/5;
        for(int i=0; i<5; x += xInc, i++) e.Graphics.FillRectangle((i & 1) == 0 ? Brushes.Black : Brushes.White, x, y, xInc, 4);
        // center the scale text horizontally under the right end of the scale bar
        string scaleText = GetDistanceString(scaleDistance, unit);
        e.Graphics.DrawString(scaleText, Font, Brushes.Black, x-e.Graphics.MeasureString(scaleText, Font).Width/2, y+8,
                              StringFormat.GenericTypographic);
      }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      Invalidate();
    }

    enum DragMode { None, MoveItem, Scroll }

    void DeleteObservationsBy(UnitShape unit)
    {
      foreach(UnitShape otherUnit in EnumerateShapes().OfType<UnitShape>())
      {
        for(int i=otherUnit.Children.Count-1; i >= 0; i--)
        {
          Observation observation = otherUnit.Children[i] as Observation;
          if(observation != null && observation.Observer == unit) otherUnit.Children.RemoveAt(i);
        }
      }
    }

    void DeleteShape(Shape shape, bool recursive)
    {
      if(shape == null) throw new ArgumentNullException();

      Shape parent = shape.Parent;
      CollectionBase<Shape> shapes = parent == null ? (CollectionBase<Shape>)RootShapes : parent.Children;

      int index = shapes.IndexOf(shape);
      shapes.RemoveAt(index);
      if(shape is UnitShape) DeleteObservationsBy((UnitShape)shape);

      if(recursive)
      {
        foreach(UnitShape unit in shape.EnumerateDescendants().OfType<UnitShape>()) DeleteObservationsBy(unit);
      }
      else
      {
        for(int i=shape.Children.Count-1; i >= 0; i--)
        {
          Shape child = shape.Children[i];
          if(!(child is Observation)) // if it's not an observation, then make it a sibling of the deleted item
          {
            shape.Children.RemoveAt(i);
            shapes.Add(child);
          }
        }
      }

      // if the selected shape or the reference shape were deleted, unselect them or select something else
      if(SelectedShape != null && SelectedShape.Board == null)
      {
        // if the shape was an observation, select either the next or previous observation in the chain or the parent unit
        Shape newSelection = null;
        if(parent != null)
        {
          Observation observation = shape as Observation;
          if(observation != null)
          {
            UnitShape observer = observation.Observer;
            if(index < parent.Children.Count)
            {
              observation = parent.Children[index] as Observation;
              if(observation != null && observation.Observer == observer && observation.GetType() == shape.GetType())
              {
                newSelection = observation;
              }
            }
            if(newSelection == null && index > 0)
            {
              observation = parent.Children[index-1] as Observation;
              if(observation != null && observation.Observer == observer && observation.GetType() == shape.GetType())
              {
                newSelection = observation;
              }
            }
            if(newSelection == null) newSelection = parent;
          }
        }

        SelectedShape = newSelection;
      }

      if(ReferenceShape != null && ReferenceShape.Board == null) ReferenceShape = null;
    }

    bool EditObservation(Observation observation)
    {
      ObservationDataForm form = new ObservationDataForm(observation, UnitSystem);
      if(form.ShowDialog() == DialogResult.OK)
      {
        if(observation is BearingObservation) ((BearingObservation)observation).Bearing = form.Bearing;
        else observation.Position = form.Position;
        observation.Time = form.Time;

        // we'll sort the observations based on observer, type, and time, to allow observations to find the adjacent observations in a
        // chain just by taking the observations at adjacent indices
        
        // make note of the indexes of all the items
        Shape.ChildCollection shapes = observation.Parent.Children;
        int[] indexes = new int[shapes.Count];
        for(int i=0; i<indexes.Length; i++) indexes[i] = i;

        // sort the indexes
        Dictionary<UnitShape,int> observerNumbers = new Dictionary<UnitShape,int>();
        Array.Sort(indexes, (ai, bi) =>
        {
          // first sort observations after non-observations
          Observation a = shapes[ai] as Observation, b = shapes[bi] as Observation;
          if(a == null) return b == null ? ai - bi : -1; // maintain the order between non-observations
          else if(b == null) return 1;

          // then sort observations by type
          Type aType = a.GetType(), bType = b.GetType();
          if(aType != bType) return a.Name.CompareTo(b.Name);

          // then sort by observer
          if(a.Observer != b.Observer)
          {
            if(!observerNumbers.ContainsKey(a.Observer)) observerNumbers.Add(a.Observer, observerNumbers.Count);
            if(!observerNumbers.ContainsKey(b.Observer)) observerNumbers.Add(b.Observer, observerNumbers.Count);
            return observerNumbers[a.Observer] - observerNumbers[b.Observer];
          }

          // then sort by time
          return a.Time.CompareTo(b.Time);
        });

        int[] backIndexes = new int[indexes.Length];
        for(int i=0; i<backIndexes.Length; i++) backIndexes[indexes[i]] = i;

        for(int i=0; i<indexes.Length; i++)
        {
          if(indexes[i] != i)
          {
            shapes.Swap(i, indexes[i]);
            indexes[backIndexes[i]] = indexes[i];
            backIndexes[indexes[i]] = backIndexes[i];
          }
        }

        Invalidate();
        return true;
      }
      else
      {
        return false;
      }
    }

    void EditShape(Shape shape)
    {
      if(shape == null) throw new ArgumentNullException();
      Observation observation = shape as Observation;
      if(observation != null)
      {
        EditObservation(observation);
      }
      else
      {
        ShapeDataForm form = new ShapeDataForm(shape, UnitSystem);
        if(form.ShowDialog() == DialogResult.OK)
        {
          shape.Name = form.ShapeName;
          UnitShape unit = shape as UnitShape;
          if(unit != null)
          {
            unit.Direction        = form.Direction;
            unit.Speed            = form.Speed;
            unit.IsMotionRelative = form.IsMotionRelative;
          }
          else
          {
            LineShape line = shape as LineShape;
            if(line != null)
            {
              line.End = line.Start + new Vector2(0, form.ShapeSize).Rotated(-form.Direction);
            }
            else
            {
              CircleShape circle = shape as CircleShape;
              if(circle != null) circle.Radius = form.ShapeSize;
              else throw new NotImplementedException();
            }
          }

          Invalidate();
        }
      }
    }

    string GetDistanceString(double meters)
    {
      return GetDistanceString(meters, UnitSystem);
    }

    void MakeChildOf(Shape child, Shape parent)
    {
      if(child.Parent != parent)
      {
        if(child.Parent != null) child.Parent.Children.Remove(child);
        else RootShapes.Remove(child);
        parent.Children.Add(child);
      }
    }

    void OnSelectionChanged()
    {
      // deselect incompatible tools
      if(SelectedShape == null && SelectedTool == AddObservationTool) SelectedTool = PointerTool;

      if(SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
    }

    void OnStatusTextChanged()
    {
      if(StatusTextChanged != null) StatusTextChanged(this, EventArgs.Empty);
    }

    void OnToolChanged()
    {
      if(ToolChanged != null) ToolChanged(this, EventArgs.Empty);
    }

    internal Brush normalBrush, selectedBrush, referenceBrush, observationBrush;
    internal Pen normalPen, selectedPen, referencePen, observationPen, selectedObservationPen;
    BoardPoint _center, dragStart;
    double _zoom = 1;
    Tool _currentTool;
    UnitShape _referenceShape;
    Shape _selectedShape;
    string _statusText;
    UnitSystem _unitSystem;
    DragMode dragMode;

    static string GetRoundedString(double value)
    {
      return GetRoundedString(value, 2);
    }

    static string GetRoundedString(double value, int decimals)
    {
      // if the absolute value is between zero and one, show enough decimal places to get the leading nonzero digit plus up to 'decimals'
      // additional digits, so if 'decimals' is 2 then 0.123 shows as 0.12 while 0.000123 shows as 0.00012
      double abs = Math.Abs(value);
      if(abs > 0 && abs < 1) decimals += -(int)Math.Floor(Math.Log10(abs)) - 1;
      return value.ToString("0." + new string('#', decimals));
    }

    /// <summary>Converts from a Cartesian bearing to a nautical angle and vice versa.</summary>
    static double SwapBearing(double radians)
    {
      // a Cartesian bearing is such that a right-pointing vector is at 0 degrees, and upward-pointing vector is at 90 degrees, etc. but we
      // want up to be 0, right to be 90, etc. to change the direction of rotation, we can subtract from 2pi. 
      // then, to offset it so that up is zero instead of right, we can add 90 degrees. so effectively, we can subtract from 2pi + pi/2.
      // we can also perform the same operation to convert back
      double angle = Math.PI*2.5 - radians;
      if(angle >= Math.PI*2) angle -= Math.PI*2; // then we need to normalize the angle, since it can be >= 360 degrees after that
      return angle;
    }

    // these correspond to the LengthUnit enum (Meter, Kilometer, Foot, Yard, Kiloyard, Mile, NauticalMile)
    static readonly double[] conversionsToMeters = new double[] { 1, 1000, 0.3048, 0.9144, 914.4, 1609.344, 1852 };
    static readonly string[] lengthAbbreviations = new string[] { "m", "km", "ft", "yd", "kyd", "mi", "nmi" };

    // these correspond to the SpeedUnit enum (MetersPerSecond, KilometersPerHour, MilesPerHour, Knots)
    static readonly double[] conversionsToMPS = new double[] { 1, 1000.0/3600, 1609.344/3600, 1852.0/3600 };
    static readonly string[] speedAbbreviations = new string[] { "m/s", "kph", "mph", "kn" };
  }
  #endregion
}