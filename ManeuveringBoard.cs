using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AdamMil.Collections;
using AdamMil.Mathematics.Geometry;
using AdamMil.Mathematics.LinearEquations;
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

    public Vector2 Velocity
    {
      get { return new Vector2(0, Speed).Rotated(-Direction); }
    }

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

    public TMASolution TMASolution { get; set; }

    public double GetEffectiveSpeed()
    {
      return GetEffectiveSpeed(new TimeSpan());
    }

    public double GetEffectiveSpeed(TimeSpan time)
    {
      return GetEffectiveVelocity(time).Length;
    }

    public Vector2 GetEffectiveVelocity()
    {
      return GetEffectiveVelocity(new TimeSpan());
    }

    public Vector2 GetEffectiveVelocity(TimeSpan time)
    {
      Waypoint applicableWaypoint, previousWaypoint;
      applicableWaypoint = GetApplicableWaypoint(time, out previousWaypoint);

      if(applicableWaypoint == null)
      {
        return Velocity;
      }
      else
      {
        double previousTime = previousWaypoint == null ? 0 : previousWaypoint.Time.TotalSeconds;
        BoardPoint previousPosition = previousWaypoint == null ? Position : previousWaypoint.Position;
        return (applicableWaypoint.Position - previousPosition) / (applicableWaypoint.Time.TotalSeconds - previousTime);
      }
    }

    public BoardPoint GetPositionAt(TimeSpan time)
    {
      Waypoint applicableWaypoint, previousWaypoint;
      applicableWaypoint = GetApplicableWaypoint(time, out previousWaypoint);

      if(applicableWaypoint == null)
      {
        return Position + Velocity*time.TotalSeconds;
      }
      else
      {
        double previousTime = previousWaypoint == null ? 0 : previousWaypoint.Time.TotalSeconds;
        BoardPoint previousPosition = previousWaypoint == null ? Position : previousWaypoint.Position;
        return previousPosition + (applicableWaypoint.Position - previousPosition) *
               ((time.TotalSeconds - previousTime) / (applicableWaypoint.Time.TotalSeconds - previousTime));
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

      // don't draw the velocity vector if we're using the TMA tool for this shape, since they often coincide
      if(Board.SelectedTool != Board.TMATool || Board.SelectedShape != this)
      {
        // if the unit has a speed, draw the velocity vector. the vector will be drawn with a length equal to the distance traveled in
        // six minutes
        Vector2 velocity = GetEffectiveVelocity();
        if(velocity.LengthSqr > 0)
        {
          velocity = new Vector2(velocity.X, -velocity.Y) * scale * ManeuveringBoard.VectorTime;
          graphics.DrawArrow(Pen, x, y, x+(float)velocity.X, y+(float)velocity.Y);
        }
      }

      graphics.DrawRectangle(Pen, x-6, y-6, 12, 12);
      RenderName(graphics, x, y+8);
    }

    Waypoint GetApplicableWaypoint(TimeSpan time, out Waypoint previousWaypoint)
    {
      Waypoint applicableWaypoint = null, previous = null;
      foreach(Waypoint waypoint in Children.OfType<Waypoint>())
      {
        previous           = applicableWaypoint;
        applicableWaypoint = waypoint;
        if(waypoint.Time > time) break;
      }
      previousWaypoint = previous;
      return applicableWaypoint;
    }

    double _direction, _speed;
  }
  #endregion

  // TODO: about observations:
  // 1. there should be a way to have observations from a towed array connected to a unit
  // 2. it should be possible for the UnitShape to act as a first observation, so it should have a Time field

  #region PositionalDataType
  enum PositionalDataType
  {
    Point, BearingLine, Waypoint
  }
  #endregion

  #region PositionalDataShape
  abstract class PositionalDataShape : Shape
  {
    public TimeSpan Time { get; set; }

    protected void RenderTime(Graphics graphics, PointF point)
    {
      RenderTime(graphics, point.X, point.Y);
    }

    protected void RenderTime(Graphics graphics, float x, float y)
    {
      string timeStr = ((int)Time.TotalHours).ToString() + ":" + Time.Minutes.ToString("d2");
      if(Time.Seconds != 0) timeStr += ":" + Time.Seconds.ToString("d2");
      CenterText(graphics, x, y, timeStr);
    }
  }
  #endregion

  #region Observation
  abstract class Observation : PositionalDataShape
  {
    public UnitShape Observer { get; set; }

    protected new Pen Pen
    {
      get { return this == Board.SelectedShape ? Board.selectedObservationPen : Board.observationPen; }
    }
  }
  #endregion

  #region BearingObservation
  sealed class BearingObservation : Observation
  {
    public double Bearing;

    public override BoardPoint Position
    {
      get { return GetEffectiveObserverPosition() + new Vector2(0, 1).Rotated(-Bearing); }
      set { Bearing = ManeuveringBoard.AngleBetween(GetEffectiveObserverPosition(), value); }
    }

    public BoardPoint GetEffectiveObserverPosition()
    {
      return Observer.GetPositionAt(Time);
    }

    public Line2 GetBearingLine()
    {
      return new Line2(GetEffectiveObserverPosition(), new Vector2(0, 1).Rotated(-Bearing));
    }

    public override double GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      BoardPoint observerPosition = GetEffectiveObserverPosition();
      Line2 bearingLine = new Line2(observerPosition, new Vector2(0, 1).Rotated(-Bearing));
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
      BoardPoint observerPosition = GetEffectiveObserverPosition();
      observerPosition = new BoardPoint(observerPosition.X*Board.ZoomFactor, -observerPosition.Y*Board.ZoomFactor);
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
        graphics.DrawCircle(Pen, observerPosition.ToPointF(), 3);
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

  #region Waypoint
  sealed class Waypoint : PositionalDataShape
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

      int index = Parent.Children.IndexOf(this) - 1;
      Waypoint previousWaypoint = index >= 0 ? Parent.Children[index] as Waypoint : null;
      Shape previousShape = previousWaypoint != null ? previousWaypoint : Parent;
      float prevX = (float)previousShape.Position.X * scale, prevY = -(float)previousShape.Position.Y * scale;
      graphics.DrawArrow(Pen, prevX, prevY, x, y);

      graphics.DrawRectangle(Pen, x-6, y-6, 12, 12);
      graphics.DrawLine(Pen, x, y-6, x, y+6);
      graphics.DrawLine(Pen, x-6, y, x+6, y);
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

  #region TMASolution
  sealed class TMASolution
  {
    public BoardPoint Position;
    public Vector2 Velocity;
    public bool LockCourse, LockSpeed;
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
      SetupBackgroundTool = new SetupBackgroundToolClass(this);
      TMATool = new TMAToolClass(this);

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
      tmaPen = (Pen)Pens.DarkGreen.Clone();
      tmaPen.Width = 2;
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
      public virtual void OnSelectionChanged(Shape previousSelection) { }
      public virtual void RenderDecorations(Graphics graphics) { }

      protected ManeuveringBoard Board { get; private set; }
    }
    #endregion

    #region AddObservationToolClass
    public sealed class AddObservationToolClass : Tool
    {
      public AddObservationToolClass(ManeuveringBoard board) : base(board)
      {
        Type = PositionalDataType.Point;
      }

      public PositionalDataType Type
      {
        get { return _type; }
        set
        {
          if(value != Type)
          {
            _type = value;
            Board.Invalidate();
          }
        }
      }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          // clicking on a unit selects it. this way, the user doesn't have to change tools or right-click and dismiss the menu to change
          // the target for which we'll add an observation
          UnitShape unit = Board.GetShapeUnderCursor(e.Location) as UnitShape;
          if(unit != null && Board.SelectedShape != unit)
          {
            Board.SelectedShape = unit;
            return true;
          }

          unit = GetSelectedUnit();
          if(unit != null && (Type == PositionalDataType.Waypoint || unit != Board.ReferenceShape))
          {
            PositionalDataShape posData;
            if(Type == PositionalDataType.Waypoint)
            {
              posData = new Waypoint() { Position = Board.GetBoardPoint(e.Location) };
            }
            else if(Type == PositionalDataType.Point)
            {
              posData = new PointObservation() { Observer = Board.ReferenceShape, Position = Board.GetBoardPoint(e.Location) };
            }
            else if(Type == PositionalDataType.BearingLine)
            {
              double bearing = ManeuveringBoard.AngleBetween(Board.ReferenceShape.Position, Board.GetBoardPoint(e.Location));
              posData = new BearingObservation() { Observer = Board.ReferenceShape, Bearing = bearing };
            }
            else
            {
              throw new NotImplementedException();
            }

            // find where to insert the new observation. we want to put it after the selected item to start with, so the lines connecting
            // the observations is most likely to be correct, but anyway EditObservation() will sort them correctly so we don't have to be
            // too careful about order (e.g. we don't have to order by type)
            int index;
            if(Board.SelectedShape is UnitShape) // insert it as the first observation if the unit is selected
            {
              PositionalDataShape firstObservation =
                  unit.Children.OfType<PositionalDataShape>().Where(
                    s => !(s is Observation) || ((Observation)s).Observer == Board.ReferenceShape).FirstOrDefault();
              index = firstObservation != null ? unit.Children.IndexOf(firstObservation) : unit.Children.Count;
            }
            else // otherwise, insert it after the selected observation
            {
              unit = (UnitShape)Board.SelectedShape.Parent;
              index = unit.Children.IndexOf(Board.SelectedShape) + 1;
            }

            unit.Children.Insert(index, posData);
            if(Board.EditPositionalData(posData)) Board.SelectedShape = posData;
            else Board.DeleteShape(posData, false);
            return true;
          }
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        InvalidateIfApplicable();
      }

      public override void OnSelectionChanged(Shape previousSelection)
      {
        InvalidateIfApplicable();
      }

      public override void RenderDecorations(Graphics graphics)
      {
        if(IsApplicable)
        {
          // TODO: use a preallocated pen?
          using(Pen pen = new Pen(Color.FromArgb(96, Color.Black), 1))
          {
            SysPoint clientPoint = Board.PointToClient(Cursor.Position);
            BoardPoint boardPoint = Board.GetBoardPoint(clientPoint);
            pen.DashStyle = DashStyle.Dash;
            graphics.DrawArrow(pen, Board.GetRenderPoint(Board.ReferenceShape.Position), new PointF(clientPoint.X, clientPoint.Y));
          }
        }
      }

      bool IsApplicable
      {
        get
        {
          return Board.ReferenceShape != null &&
                 (Type == PositionalDataType.Waypoint || Board.ReferenceShape != Board.SelectedShape && GetSelectedUnit() != null);
        }
      }

      UnitShape GetSelectedUnit()
      {
        UnitShape unit = Board.SelectedShape as UnitShape;
        if(unit == null && Board.SelectedShape != null) unit = Board.SelectedShape.Parent as UnitShape;
        return unit;
      }

      void InvalidateIfApplicable()
      {
        if(IsApplicable) Board.Invalidate();
      }

      PositionalDataType _type;
    }
    #endregion

    #region AddUnitToolClass
    public sealed class AddUnitToolClass : Tool
    {
      public AddUnitToolClass(ManeuveringBoard board) : base(board) { }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          // clicking on a unit selects it. this way, the user doesn't have to change tools or right-click and dismiss the menu to change
          // selection
          UnitShape unit = Board.GetShapeUnderCursor(e.Location) as UnitShape;
          if(unit != null && Board.SelectedShape != unit)
          {
            Board.SelectedShape = unit;
            return true;
          }

          HashSet<string> names = new HashSet<string>();
          foreach(Shape shape in Board.EnumerateShapes())
          {
            if(!string.IsNullOrEmpty(shape.Name)) names.Add(shape.Name);
          }

          string name = "M1";
          for(int suffix=2; names.Contains(name); suffix++) name = "M" + suffix.ToInvariantString();

          unit = new UnitShape() { Name = name, Position = Board.GetBoardPoint(e.Location) };
          Board.RootShapes.Add(unit);
          Board.SelectedShape = unit;
          return true;
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        InvalidateIfReference();
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

    #region SetupBackgroundToolClass
    public sealed class SetupBackgroundToolClass : Tool
    {
      public SetupBackgroundToolClass(ManeuveringBoard board) : base(board) { }

      public override bool MouseDragStart(MouseEventArgs e)
      {
        base.MouseDragStart(e);

        // left drag scales the background image by letting the user draw a scale line and asking them how long it is in real units.
        // right drag moves the background image. middle drag scrolls the display
        dragStart = dragPoint = Board.GetBoardPoint(e.Location);
        if(e.Button == MouseButtons.Right) dragStart = Board.BackgroundImageCenter;
        else if(e.Button == MouseButtons.Middle) dragStart = Board.Center;
        else if(e.Button != MouseButtons.Left) return false;
        dragButton = e.Button;
        return true;
      }

      public override void MouseDrag(MouseDragEventArgs e)
      {
        if(dragButton == MouseButtons.Left)
        {
          dragPoint = Board.GetBoardPoint(e.Location);
          Board.Invalidate(); // repaint the scale line
        }
        else if(dragButton == MouseButtons.Right)
        {
          Board.BackgroundImageCenter = dragStart + (Board.GetBoardPoint(e.Location) - dragPoint);
        }
        else if(dragButton == MouseButtons.Middle)
        {
          Board.Center = dragStart; // reset the start position so we can correctly calculate the movement
          Board.Center = dragStart - (Board.GetBoardPoint(e.Location) - dragPoint);
        }
      }

      public override void MouseDragEnd(MouseDragEventArgs e)
      {
        if(dragButton == MouseButtons.Left)
        {
          double distance = dragStart.DistanceTo(dragPoint);
          BackgroundScaleForm form = new BackgroundScaleForm(distance*Board.ZoomFactor, distance, Board.UnitSystem);
          if(form.ShowDialog() == DialogResult.OK) Board.BackgroundImageScale *= form.Distance / distance;
          Board.Invalidate();
        }

        dragButton = MouseButtons.None;
      }

      public override void RenderDecorations(Graphics graphics)
      {
        if(dragButton == MouseButtons.Left && dragPoint != dragStart)
        {
          graphics.DrawLine(Pens.Red, Board.GetRenderPoint(dragStart), Board.GetRenderPoint(dragPoint));
        }
      }

      BoardPoint dragStart, dragPoint;
      MouseButtons dragButton;
    }
    #endregion

    #region TMAToolClass
    public sealed class TMAToolClass : Tool
    {
      public TMAToolClass(ManeuveringBoard board) : base(board) { }

      public override void Activate()
      {
        OnSelectionChanged(null);
      }

      public override void Deactivate()
      {
        UpdateTMASolution(GetSelectedUnit());
        HideForm();
      }

      public override void MouseDrag(MouseDragEventArgs e)
      {
        if(dragMode != DragMode.None)
        {
          if(dragMode == DragMode.Start)
          {
            position = Board.GetBoardPoint(e.Location);
            velocity = (dragStart - position) * (maxTime == 0 ? 1.0/VectorTime : 1.0/maxTime);
          }
          else if(dragMode == DragMode.Middle)
          {
            position = dragStart + (Board.GetBoardPoint(e.Location) - dragPoint);
          }
          else if(dragMode == DragMode.End)
          {
            velocity = (Board.GetBoardPoint(e.Location) - position) * (maxTime == 0 ? 1.0/VectorTime : 1.0/maxTime);
          }

          if(form.LockCourse || form.LockSpeed)
          {
            if(form.LockCourse && velocity != Vector2.Zero) velocity = new Vector2(0, velocity.Length).Rotated(-form.Course);
            if(form.LockSpeed) velocity = new Vector2(form.Speed, 0).Rotated(velocity == Vector2.Zero ? 0 : velocity.Angle);
            if(dragMode == DragMode.Start) position = dragStart - velocity * (maxTime == 0 ? VectorTime : maxTime);
          }

          OnArrowMoved();
        }
      }

      public override void MouseDragEnd(MouseDragEventArgs e)
      {
        dragMode = DragMode.None;
      }

      public override bool MouseDragStart(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left) // left drag on the line moves it
        {
          dragPoint = Board.GetBoardPoint(e.Location);
          BoardPoint end = position + velocity * (maxTime == 0 ? VectorTime : maxTime);
          double distanceFromStart = dragPoint.DistanceTo(position) * Board.ZoomFactor;
          double distanceFromEnd = dragPoint.DistanceTo(end + velocity.Normalized(3)) * Board.ZoomFactor;
          if(distanceFromStart <= 6 || distanceFromEnd <= 9)
          {
            dragMode  = distanceFromStart < distanceFromEnd ? DragMode.Start : DragMode.End;
            dragStart = dragMode == DragMode.Start ? end : position;
            return true;
          }

          Line2 tmaLine = new Line2(position, end);
          double distance = dragPoint.DistanceTo(tmaLine.ClosestPointOnSegment(dragPoint)) * Board.ZoomFactor;
          if(distance <= 4)
          {
            dragMode  = DragMode.Middle;
            dragStart = position;
            return true;
          }
        }

        return false;
      }

      public override void RenderDecorations(Graphics graphics)
      {
        UnitShape unit = GetSelectedUnit();
        if(unit != null)
        {
          PointF start = Board.GetRenderPoint(position);
          Vector2 crossTick = velocity.CrossVector.Normalized(6);

          List<float> errors = new List<float>(12);

          foreach(BearingObservation bearing in unit.Children.OfType<BearingObservation>().OrderBy(o => o.Time))
          {
            BoardPoint unitPoint = position + velocity*bearing.Time.TotalSeconds;
            PointF point = Board.GetRenderPoint(unitPoint);
            graphics.DrawLine(Board.tmaPen, point.X-(float)crossTick.X, point.Y+(float)crossTick.Y,
                              point.X+(float)crossTick.X, point.Y-(float)crossTick.Y);
            errors.Add((float)bearing.GetBearingLine().DistanceTo(unitPoint));
          }

          PointF end = Board.GetRenderPoint(position + velocity * (maxTime == 0 ? VectorTime : maxTime));
          graphics.DrawLine(Board.tmaPen, start, end);

          Vector2 capVector = (velocity == Vector2.Zero ? new Vector2(1, 0) : velocity).Normalized(6);
          PointF capEnd = new PointF(end.X + (float)capVector.X, end.Y - (float)capVector.Y);
          graphics.DrawArrow(Board.tmaPen, end, capEnd);

          if(errors.Count != 0)
          {
            const int StackWidth = 101, MinStackHeight = 128;

            SysRect dotStackRect = new SysRect(0, 0, StackWidth, Math.Max(MinStackHeight, errors.Count*6 + Board.Font.Height + 8));
            using(Brush dimBrush = new SolidBrush(Color.FromArgb(192, 0, 0, 0))) graphics.FillRectangle(dimBrush, dotStackRect);

            // the base error is 10 meters per pixel, but if the average error magnitude is greater than the available space, scale down by
            // some integer amount to let them see better
            const int BaseMPP = 10;
            float errorScale = errors.Select(err => Math.Abs(err)).Average() * (1f/(StackWidth/2*BaseMPP));
            string scaleString;
            if(errorScale > 1)
            {
              errorScale  = (float)Math.Ceiling(errorScale);
              scaleString = "1 : " + errorScale.ToString();
              errorScale  = (1f/BaseMPP) / errorScale;
            }
            else // otherwise, everything fits, so show it as-is
            {
              errorScale  = (1f/BaseMPP);
              scaleString = "1 : 1";
            }

            graphics.DrawString(scaleString, Board.Font, Brushes.White,
              dotStackRect.X + (int)Math.Round((dotStackRect.Width - graphics.MeasureString(scaleString, Board.Font).Width) * 0.5f),
              dotStackRect.Y + 2);
            graphics.DrawHLine(Pens.White, dotStackRect.X, dotStackRect.Y + Board.Font.Height + 4, dotStackRect.Right-1);
            graphics.DrawVLine(Pens.White, dotStackRect.X + dotStackRect.Width/2, dotStackRect.Y + Board.Font.Height + 4,
                               dotStackRect.Bottom-1);

            // TODO: should we position the dots vertically based on their time (i.e. if some observations are further apart in time, should
            // they be further apart vertically?)
            // TODO: what about dots that correspond to the same time (e.g. observations from two sensors?)
            for(int x=dotStackRect.X+dotStackRect.Width/2, y=dotStackRect.Y+Board.Font.Height+8, i=errors.Count-1; i >= 0; y += 6, i--)
            {
              float dotX = x + errors[i]*errorScale;
              if(dotX < dotStackRect.X) dotX = dotStackRect.X;
              else if(dotX > dotStackRect.Right-1) dotX = dotStackRect.Right-1;

              graphics.FillRectangle(Brushes.Lime, dotX-1, y, 3, 3);
            }
          }
        }
      }

      public override void OnSelectionChanged(Shape previousSelection)
      {
        UpdateTMASolution(GetSelectedUnit(previousSelection));

        if(Board.SelectedShape is UnitShape || Board.SelectedShape is PositionalDataShape)
        {
          ShowForm();
          Initialize();
        }
        else
        {
          HideForm();
        }
      }

      enum DragMode
      {
        None, Start, Middle, End
      }

      UnitShape GetSelectedUnit()
      {
        return GetSelectedUnit(Board.SelectedShape);
      }

      UnitShape GetSelectedUnit(Shape shape)
      {
        UnitShape unit = shape as UnitShape;
        if(unit == null && shape != null) unit = shape.Parent as UnitShape;
        return unit;
      }

      void HideForm()
      {
        if(form != null)
        {
          form.Hide();
          form = null;
        }
      }

      void Initialize()
      {
        UnitShape unit = GetSelectedUnit();
        if(unit != null)
        {
          if(unit.TMASolution == null)
          {
            position = unit.Position;
            velocity = unit.Velocity;
            form.LockCourse = form.LockSpeed = false;
          }
          else
          {
            position = unit.TMASolution.Position;
            velocity = unit.TMASolution.Velocity;
            form.LockCourse = unit.TMASolution.LockCourse;
            form.LockSpeed  = unit.TMASolution.LockSpeed;
          }

          // TODO: handle events that would change maxTime, like updating an observation's data
          maxTime = 0;
          foreach(BearingObservation bearing in unit.Children.OfType<BearingObservation>())
          {
            double time = bearing.Time.TotalSeconds;
            if(time > maxTime) maxTime = time;
          }
        }

        OnArrowMoved();
      }

      void OnArrowMoved()
      {
        form.Course = velocity == Vector2.Zero ? 0 : SwapBearing(velocity.Angle);
        form.Speed  = velocity.Length;
        Board.Invalidate();
      }

      void ShowForm()
      {
        if(form == null)
        {
          form = new TMAForm();
          form.Location = Board.PointToScreen(new Point(Board.Width - form.Width, 0));
          form.ApplySolution += form_ApplySolution;
          form.AutoSolve += form_AutoSolve;
          form.CourseChanged += form_CourseChanged;
          form.FormClosed += form_FormClosed;
          form.SpeedChanged += form_SpeedChanged;
          form.Show(Board.FindForm());
          Board.FindForm().Select(); // give focus back to the main form
        }
      }

      void UpdateTMASolution(UnitShape unit)
      {
        if(unit != null)
        {
          if(unit.TMASolution == null) unit.TMASolution = new TMASolution();
          unit.TMASolution.Position = position;
          unit.TMASolution.Velocity = velocity;
          unit.TMASolution.LockCourse = form.LockCourse;
          unit.TMASolution.LockSpeed  = form.LockSpeed;
        }
      }

      void form_ApplySolution(object sender, EventArgs e)
      {
        UnitShape unit = GetSelectedUnit();
        unit.Position  = position;
        unit.Speed     = velocity.Length;
        unit.Direction = SwapBearing(velocity.Angle);
        Board.Invalidate();
        Board.FindForm().Select();
      }

      void form_AutoSolve(object sender, EventArgs e)
      {
        AdamMil.Mathematics.Matrix4 matrix = new AdamMil.Mathematics.Matrix4();
        double[] constSum = new double[4];

        UnitShape unit = GetSelectedUnit();
        foreach(Observation observation in unit.Children.OfType<Observation>())
        {
          double time = observation.Time.TotalSeconds, timeSqr = time*time;
          BearingObservation bearing = observation as BearingObservation;
          if(bearing != null)
          {
            BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
            Vector2 bearingLine = new Vector2(0, 1).Rotated(-bearing.Bearing);
            double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

            matrix.M00 += sySqr;
            matrix.M01 += -sxy;
            matrix.M02 += sySqr*time;
            matrix.M03 += -sxy*time;
            constSum[0] += observerPosition.X*sySqr - observerPosition.Y*sxy;

            matrix.M10 += -sxy;
            matrix.M11 += sxSqr;
            matrix.M12 += -time*sxy;
            matrix.M13 += time*sxSqr;
            constSum[1] += observerPosition.Y*sxSqr - observerPosition.X*sxy;

            matrix.M20 += time*sySqr;
            matrix.M21 += -time*sxy;
            matrix.M22 += timeSqr*sySqr;
            matrix.M23 += -timeSqr*sxy;
            constSum[2] += observerPosition.X*time*sySqr - observerPosition.Y*time*sxy;

            matrix.M30 += -time*sxy;
            matrix.M31 += time*sxSqr;
            matrix.M32 += -timeSqr*sxy;
            matrix.M33 += timeSqr*sxSqr;
            constSum[3] += observerPosition.Y*time*sxSqr - observerPosition.X*time*sxy;
          }

          PointObservation point = observation as PointObservation;
          if(point != null)
          {
            matrix.M00 += 1;
            matrix.M02 += time;
            constSum[0] += point.Position.X;

            matrix.M11 += 1;
            matrix.M13 += time;
            constSum[1] += point.Position.Y;

            matrix.M20 += time;
            matrix.M22 += timeSqr;
            constSum[2] += time*point.Position.X;

            matrix.M31 += time;
            matrix.M33 += timeSqr;
            constSum[3] += time*point.Position.Y;
          }
        }

        try
        {
          LUDecomposition lud = new LUDecomposition(matrix.ToMatrix());
          AdamMil.Mathematics.Matrix values = lud.Solve(new AdamMil.Mathematics.Matrix(constSum, 1));

          position = new BoardPoint(values[0, 0], values[1, 0]);
          velocity = new Vector2(values[2, 0], values[3, 0]);
          if(velocity.Length < 0.000001) velocity = Vector2.Zero; // GDI+ crashes when trying to render short lines, so avoid small speeds
          OnArrowMoved();
        }
        catch(InvalidOperationException)
        {
          MessageBox.Show("Unable to solve for any TMA solution. More or better observational data is needed.", "No TMA solution",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        Board.FindForm().Select();
      }

      void form_CourseChanged(object sender, EventArgs e)
      {
        if(velocity != Vector2.Zero)
        {
          velocity = new Vector2(0, velocity.Length).Rotated(-form.Course);
          Board.Invalidate();
        }
      }

      void form_FormClosed(object sender, FormClosedEventArgs e)
      {
        if(Board.SelectedTool == this) Board.SelectedTool = Board.PointerTool;
      }

      void form_SpeedChanged(object sender, EventArgs e)
      {
        velocity = new Vector2(form.Speed, 0).Rotated(velocity == Vector2.Zero ? 0 : velocity.Angle);
        Board.Invalidate();
      }

      TMAForm form;
      BoardPoint position, dragStart, dragPoint;
      Vector2 velocity;
      double maxTime;
      DragMode dragMode;
    }
    #endregion

    public event EventHandler ReferenceShapeChanged, SelectionChanged, StatusTextChanged, ToolChanged;

    public BoardPoint BackgroundImageCenter
    {
      get { return _bgCenter; }
      set
      {
        if(value != BackgroundImageCenter)
        {
          _bgCenter = value;
          Invalidate();
        }
      }
    }

    public double BackgroundImageScale
    {
      get { return _bgScale; }
      set
      {
        if(value != BackgroundImageScale)
        {
          if(value <= 0) throw new ArgumentOutOfRangeException();
          _bgScale = value;
          Invalidate();
        }
      }
    }

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
          OnReferenceShapeChanged();
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
          Shape previousSelection = SelectedShape;
          _selectedShape = value;
          OnSelectionChanged(previousSelection);
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
          Tool previousTool = SelectedTool;
          Cursor = Cursors.Default;
          _currentTool = value;
          if(previousTool != null) previousTool.Deactivate();
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
    public readonly SetupBackgroundToolClass SetupBackgroundTool;
    public readonly TMAToolClass TMATool;

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
      Utility.Dispose(ref tmaPen);
      Utility.Dispose(ref observationPen);
      Utility.Dispose(ref selectedObservationPen);
      Utility.Dispose(ref normalBrush);
      Utility.Dispose(ref selectedBrush);
      Utility.Dispose(ref referenceBrush);
      Utility.Dispose(ref observationBrush);
      Utility.Dispose(ref backgroundImage);
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
            if(!(shape is PositionalDataShape) && ReferenceShape != null && shape.Parent != ReferenceShape &&
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
            if(shape.Children.Any(c => !(c is PositionalDataShape))) // if the shape has children that aren't observations...
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
            dragMode = DragMode.MoveItem;
            return;
          }
        }
        else if(e.Button == MouseButtons.Right) // right drag scrolls the view
        {
          dragStart = Center;
          dragPoint = GetBoardPoint(e.Location);
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
        SelectedShape.Position = GetBoardPoint(e.Location);
        Invalidate();
      }
      else if(dragMode == DragMode.Scroll)
      {
        Center = dragStart; // reset the camera so that the correct mouse movement can be calculated
        Center = dragStart - (GetBoardPoint(e.Location) - dragPoint);
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

      if(ReferenceShape != null)
      {
        BoardPoint cursor = GetBoardPoint(PointToClient(Cursor.Position));
        double angle = AngleBetween(ReferenceShape.Position, cursor) * MathConst.RadiansToDegrees;
        StatusText = angle.ToString("f2") + "°, " + GetDistanceString((cursor-ReferenceShape.Position).Length);
      }

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

      if(BackgroundImage != null)
      {
        double scale = BackgroundImageScale * ZoomFactor, width = BackgroundImage.Width*scale, height = BackgroundImage.Height*scale;
        double centerX = (BackgroundImageCenter.X-Center.X)*ZoomFactor + Width*0.5;
        double centerY = (Center.Y-BackgroundImageCenter.Y)*ZoomFactor + Height*0.5;
        e.Graphics.DrawImage(BackgroundImage, (float)(centerX-width*0.5), (float)(centerY-height*0.5), (float)width, (float)height);
      }

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

    internal const int VectorTime = 360; // the length of motion vectors
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
          if(!(child is PositionalDataShape)) // if it's not an observation, then make it a sibling of the deleted item
          {
            shape.Children.RemoveAt(i);
            shapes.Add(child);
          }
        }
      }

      // if the selected shape or the reference shape were deleted, unselect them or select something else
      if(SelectedShape != null && SelectedShape.Board == null)
      {
        // if the shape was positional data, select either the next or previous shape in the chain or the parent unit
        Shape newSelection = null;
        if(parent != null)
        {
          PositionalDataShape posData = shape as PositionalDataShape;
          if(posData != null)
          {
            UnitShape observer = posData is Observation ? ((Observation)posData).Observer : null;
            Predicate<PositionalDataShape> inSameChain = 
              s => s != null && s.GetType() == shape.GetType() && (!(s is Observation) || ((Observation)s).Observer == observer);

            if(index < parent.Children.Count)
            {
              posData = parent.Children[index] as PositionalDataShape;
              if(inSameChain(posData)) newSelection = posData;
            }
            if(newSelection == null && index > 0)
            {
              posData = parent.Children[index-1] as PositionalDataShape;
              if(inSameChain(posData)) newSelection = posData;
            }
            if(newSelection == null) newSelection = parent;
          }
        }

        SelectedShape = newSelection;
      }

      if(ReferenceShape != null && ReferenceShape.Board == null) ReferenceShape = null;
    }

    bool EditPositionalData(PositionalDataShape posData)
    {
      PositionalDataForm form = new PositionalDataForm(posData, UnitSystem);
      if(form.ShowDialog() == DialogResult.OK)
      {
        if(posData is BearingObservation) ((BearingObservation)posData).Bearing = form.Bearing;
        else posData.Position = form.Position;
        posData.Time = form.Time;

        // we'll sort the observations based on observer, type, and time, to allow observations to find the adjacent observations in a
        // chain just by taking the observations at adjacent indices
        
        // make note of the indexes of all the items
        Shape.ChildCollection shapes = posData.Parent.Children;
        int[] indexes = new int[shapes.Count];
        for(int i=0; i<indexes.Length; i++) indexes[i] = i;

        // sort the indexes
        Dictionary<UnitShape,int> observerNumbers = new Dictionary<UnitShape,int>();
        Array.Sort(indexes, (ai, bi) =>
        {
          // first sort observations after non-observations
          PositionalDataShape a = shapes[ai] as PositionalDataShape, b = shapes[bi] as PositionalDataShape;
          if(a == null) return b == null ? ai - bi : -1; // maintain the order between other types of shapes
          else if(b == null) return 1;

          // then sort observations by type
          Type aType = a.GetType(), bType = b.GetType();
          if(aType != bType) return aType.Name.CompareTo(bType.Name);

          // then sort by observer
          if(aType != typeof(Waypoint))
          {
            Observation oa = (Observation)a, ob = (Observation)b;
            if(oa.Observer != ob.Observer)
            {
              if(!observerNumbers.ContainsKey(oa.Observer)) observerNumbers.Add(oa.Observer, observerNumbers.Count);
              if(!observerNumbers.ContainsKey(ob.Observer)) observerNumbers.Add(ob.Observer, observerNumbers.Count);
              return observerNumbers[oa.Observer] - observerNumbers[ob.Observer];
            }
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
      PositionalDataShape observation = shape as PositionalDataShape;
      if(observation != null)
      {
        EditPositionalData(observation);
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

    void OnReferenceShapeChanged()
    {
      if(ReferenceShape == null && SelectedTool == AddObservationTool) SelectedTool = PointerTool;
      if(ReferenceShapeChanged != null) ReferenceShapeChanged(this, EventArgs.Empty);
    }

    void OnSelectionChanged(Shape previousSelection)
    {
      if(SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
      SelectedTool.OnSelectionChanged(previousSelection);
    }

    void OnStatusTextChanged()
    {
      if(StatusTextChanged != null) StatusTextChanged(this, EventArgs.Empty);
    }

    void OnToolChanged()
    {
      if(ToolChanged != null) ToolChanged(this, EventArgs.Empty);
      Invalidate();
    }

    internal Brush normalBrush, selectedBrush, referenceBrush, observationBrush;
    internal Pen normalPen, selectedPen, referencePen, tmaPen, observationPen, selectedObservationPen;
    Image backgroundImage;
    BoardPoint _bgCenter, _center, dragStart, dragPoint;
    double _zoom = 1, _bgScale = 1;
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