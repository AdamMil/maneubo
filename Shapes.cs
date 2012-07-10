using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using AdamMil.Collections;
using AdamMil.Mathematics.Geometry;
using AdamMil.Utilities;
using BoardPoint = AdamMil.Mathematics.Geometry.Point2;
using BoardRect  = AdamMil.Mathematics.Geometry.Rectangle;
using SysPoint   = System.Drawing.Point;
using System.Globalization;
using System.IO;

namespace Maneubo
{
  #region Handle
  abstract class Handle
  {
    public virtual BoardPoint GetStartPoint(Shape shape, BoardPoint dragStart)
    {
      return dragStart;
    }

    public virtual string GetStatusText(Shape shape)
    {
      return null;
    }

    public abstract void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint);
  }
  #endregion

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

    public abstract KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point);

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

    public static Shape Load(XmlReader reader, Dictionary<Observation,string> observers, Dictionary<string,UnitShape> unitsById)
    {
      Shape shape;
      switch(reader.LocalName)
      {
        case "circle": shape = CircleShape.Load(reader); break;
        case "line": shape = LineShape.Load(reader); break;
        case "unit": shape = UnitShape.Load(reader, observers, unitsById); break;
        case "bearingObservation": shape = BearingObservation.Load(reader, observers); break;
        case "pointObservation": shape = PointObservation.Load(reader, observers); break;
        case "waypoint": shape = Waypoint.Load(reader); break;
        default: throw new System.IO.InvalidDataException("Unknown shape tag: " + reader.LocalName);
      }

      reader.Read();
      return shape;
    }

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

    protected internal abstract void Save(XmlWriter writer);

    internal uint ID { get; set; }

    internal string GetXmlId()
    {
      return "u" + ID.ToInvariantString();
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

    public override KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      // get the minimum from either the center point or the circle itself, whichever is smaller
      double distanceFromCenter = boardPoint.DistanceTo(Position), distanceFromEdge = Math.Abs(distanceFromCenter - Radius);
      if(distanceFromCenter < distanceFromEdge)
      {
        distanceFromCenter *= Board.ZoomFactor;
        return new KeyValuePair<double, Handle>(distanceFromCenter <= 5 ? distanceFromCenter : double.NaN, null);
      }
      else
      {
        distanceFromEdge *= Board.ZoomFactor;
        return new KeyValuePair<double, Handle>(distanceFromEdge <= 4 ? distanceFromEdge : double.NaN, edgeHandle);
      }
    }

    public static CircleShape Load(XmlReader reader)
    {
      CircleShape circle = new CircleShape();
      circle.Position = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("position"));
      circle.Radius   = reader.GetDoubleAttribute("radius");
      return circle;
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor;
      float centerX = (float)Position.X * scale, centerY = -(float)Position.Y * scale;
      graphics.DrawCircle(Pen, centerX, centerY, (float)Radius * scale);

      const float CrossSize = 5;
      graphics.DrawLine(Pen, new PointF(centerX, centerY-CrossSize/2), new PointF(centerX, centerY+CrossSize/2));
      graphics.DrawLine(Pen, new PointF(centerX-CrossSize/2, centerY), new PointF(centerX+CrossSize/2, centerY));

      RenderName(graphics, centerX, centerY+CrossSize/2+2);
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("circle");
      writer.WriteAttributeString("position", ManeuveringBoard.FormatXmlVector(Position));
      writer.WriteAttribute("radius", Radius);
      writer.WriteEndElement();
    }

    internal string GetRadiusStatusText()
    {
      return "Radius: " + Board.GetDistanceString(Radius);
    }

    #region EdgeHandle
    sealed class EdgeHandle : Handle
    {
      public override string GetStatusText(Shape shape)
      {
        return ((CircleShape)shape).GetRadiusStatusText();
      }

      public override void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint)
      {
        CircleShape circle = (CircleShape)shape;
        circle.Radius = (dragPoint - circle.Position).Length;
      }
    }
    #endregion

    double _radius;

    static readonly EdgeHandle edgeHandle = new EdgeHandle();
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

    public override KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      double distanceFromStart = boardPoint.DistanceTo(Start) * Board.ZoomFactor;
      double distanceFromEnd = boardPoint.DistanceTo(End) * Board.ZoomFactor;
      if(distanceFromStart <= 5 || distanceFromEnd <= 5)
      {
        return new KeyValuePair<double, Handle>(Math.Min(distanceFromStart, distanceFromEnd),
                                                distanceFromStart < distanceFromEnd ? startHandle : endHandle);
      }

      double distanceFromSegment = new Line2(Start, End).SegmentDistanceTo(boardPoint) * Board.ZoomFactor;
      return new KeyValuePair<double,Handle>(distanceFromSegment <= 4 ? distanceFromSegment : double.NaN, moveHandle);
    }

    public static LineShape Load(XmlReader reader)
    {
      LineShape line = new LineShape();
      line.Start = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("start"));
      line.End   = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("end"));
      return line;
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor;
      float startX = (float)Start.X * scale, startY = -(float)Start.Y * scale, endX = (float)End.X * scale, endY = -(float)End.Y * scale;
      graphics.DrawLine(Pen, startX, startY, endX, endY);

      graphics.FillRectangle(Brush, startX-2, startY-2, 4, 4);
      graphics.FillRectangle(Brush, endX-2, endY-2, 4, 4);
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("line");
      writer.WriteAttributeString("start", ManeuveringBoard.FormatXmlVector(Start));
      writer.WriteAttributeString("end", ManeuveringBoard.FormatXmlVector(End));
      writer.WriteEndElement();
    }

    internal string GetStatusText()
    {
      Vector2 vector = Vector;
      return "Line: " + (ManeuveringBoard.SwapBearing(vector.Angle) * MathConst.RadiansToDegrees).ToString("f2") + "°, " +
             Board.GetDistanceString(vector.Length);
    }

    #region LineHandle
    abstract class LineHandle : Handle
    {
      public override string GetStatusText(Shape shape)
      {
        return ((LineShape)shape).GetStatusText();
      }
    }
    #endregion

    #region StartHandle
    sealed class StartHandle : LineHandle
    {
      public override void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint)
      {
        ((LineShape)shape).Start = dragPoint;
      }
    }
    #endregion

    #region EndHandle
    sealed class EndHandle : LineHandle
    {
      public override void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint)
      {
        ((LineShape)shape).End = dragPoint;
      }
    }
    #endregion

    #region MoveHandle
    sealed class MoveHandle : LineHandle
    {
      public override BoardPoint GetStartPoint(Shape shape, BoardPoint dragStart)
      {
        // return a "point" (actually a vector) representing the difference between the drag start and the line center
        return (dragStart - ((LineShape)shape).Position).ToPoint();
      }

      public override void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint)
      {
        ((LineShape)shape).Position = new BoardPoint(dragPoint.X - dragStart.X, dragPoint.Y - dragStart.Y);
      }
    }
    #endregion

    static readonly Handle startHandle = new StartHandle(), endHandle = new EndHandle(), moveHandle = new MoveHandle();
  }
  #endregion

  #region UnitShapeType
  enum UnitShapeType
  {
    Air, Boat, Helicopter, Land, OwnShip, Subsurface, Surface, Unknown, Weapon
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
    public bool IsMotionRelative { get; set; } // TODO: save/load this

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

    public UnitShapeType Type { get; set; }

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

    public override KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point)
    {
      BoardPoint boardPoint = Board.GetBoardPoint(point);
      double distance = Math.Max(Math.Abs(Position.X - boardPoint.X), Math.Abs(Position.Y - boardPoint.Y)) * Board.ZoomFactor;
      if(distance <= 6) return new KeyValuePair<double,Handle>(distance, null);

      // if we're drawing a motion vector and the vector is not controlled by waypoints, allow the user to grab that, too
      if(ShouldDrawMotionVector && Speed > 0 && !Children.Any(c => c is Waypoint))
      {
        Vector2 vector = new Vector2(0, Speed*ManeuveringBoard.VectorTime).Rotated(-Direction);
        // use two pixels before the endpoint as the hotspot, since that's closer to the center of mass of the arrow head
        vector.Normalize(vector.Length-2/Board.ZoomFactor);
        distance = boardPoint.DistanceTo(Position + vector) * Board.ZoomFactor;
        if(distance <= 5) return new KeyValuePair<double,Handle>(distance, vectorHandle);
      }

      return new KeyValuePair<double, Handle>(double.NaN, null);
    }

    public override void Render(Graphics graphics)
    {
      float scale = (float)Board.ZoomFactor, x = (float)Position.X * scale, y = -(float)Position.Y * scale;

      // don't draw the velocity vector if we're using the TMA tool for this shape, since they often coincide
      Vector2 velocity = GetEffectiveVelocity();
      if(ShouldDrawMotionVector && velocity.LengthSqr > 0)
      {
        // if the unit has a speed, draw the velocity vector. the vector will be drawn with a length equal to the distance traveled in
        // six minutes
        velocity = new Vector2(velocity.X, -velocity.Y) * scale * ManeuveringBoard.VectorTime;
        graphics.DrawArrow(Pen, x, y, x+(float)velocity.X, y+(float)velocity.Y);
      }

      switch(Type)
      {
        case UnitShapeType.Air:
          graphics.FillRectangle(Brush, x-1, y-1, 2, 2);
          graphics.DrawLine(Pen, x-6, y, x-6, y-6);
          graphics.DrawLine(Pen, x-6, y-6, x+6, y-6);
          graphics.DrawLine(Pen, x+6, y-6, x+6, y);
          RenderName(graphics, x, y+3);
          break;
        case UnitShapeType.Boat:
        {
          var oldMatrix = graphics.Transform;

          double angle = velocity.Angle;
          if(double.IsNaN(angle)) angle = 0;
          else angle += Math.PI/2;

          graphics.TranslateTransform(x, y);
          graphics.RotateTransform((float)(angle * MathConst.RadiansToDegrees));
          graphics.TranslateTransform(-x, -y);

          graphics.DrawLine(Pen, x-5, y-2, x-5, y+7);
          graphics.DrawLine(Pen, x-5, y+7, x+5, y+7);
          graphics.DrawLine(Pen, x+5, y+7, x+5, y-2);
          graphics.DrawLine(Pen, x+5, y-2, x,   y-7);
          graphics.DrawLine(Pen, x,   y-7, x-5, y-2);

          graphics.Transform = oldMatrix;
          RenderName(graphics, x, y+9);
          break;
        }
        case UnitShapeType.Helicopter:
          graphics.DrawLine(Pen, x-6, y, x+6, y);
          graphics.DrawLine(Pen, x-6, y, x-6, y+6);
          graphics.DrawLine(Pen, x+6, y, x+6, y+6);
          graphics.DrawLine(Pen, x, y-6, x, y+5);
          graphics.DrawLine(Pen, x-3, y-6, x+3, y-6);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.Land:
          graphics.DrawLine(Pen, x-6, y-6, x+6, y+6);
          graphics.DrawLine(Pen, x+6, y-6, x-6, y+6);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.OwnShip:
          graphics.DrawCircle(Pen, x, y, 6);
          graphics.FillRectangle(Brush, x-1, y-1, 2, 2);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.Subsurface:
          graphics.FillRectangle(Brush, x-1, y-1, 2, 2);
          graphics.DrawLine(Pen, x-6, y, x-6, y+6);
          graphics.DrawLine(Pen, x-6, y+6, x+6, y+6);
          graphics.DrawLine(Pen, x+6, y+6, x+6, y);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.Surface:
          graphics.DrawRectangle(Pen, x-6, y-6, 12, 12);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.Unknown:
          graphics.DrawLine(Pen, x, y-6, x-6, y);
          graphics.DrawLine(Pen, x-6, y, x, y+6);
          graphics.DrawLine(Pen, x, y+6, x+6, y);
          graphics.DrawLine(Pen, x+6, y, x, y-6);
          graphics.DrawRectangle(Pen, x-2, y-2, 4, 4);
          RenderName(graphics, x, y+8);
          break;
        case UnitShapeType.Weapon:
          graphics.DrawCircle(Pen, x, y, 4.5f);
          graphics.DrawLine(Pen, x, y, x-5, y-5);
          graphics.DrawLine(Pen, x, y, x+5, y-5);
          graphics.DrawLine(Pen, x, y, x, y+7);
          RenderName(graphics, x, y+8);
          break;
      }
    }

    public static new UnitShape Load(XmlReader reader, Dictionary<Observation,string> observers, Dictionary<string,UnitShape> unitsById)
    {
      UnitShape shape = new UnitShape();
      shape.Name = reader.GetAttribute("name");
      shape.Position = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("position"));
      shape.Direction = reader.GetDoubleAttribute("course");
      shape.Speed = reader.GetDoubleAttribute("speed");
      shape.Type = Utility.ParseEnum<UnitShapeType>(reader.GetStringAttribute("type", "unknown"), true);

      string id = reader.GetAttribute("id");
      if(!string.IsNullOrEmpty(id)) unitsById.Add(id, shape);

      if(!reader.IsEmptyElement)
      {
        reader.Read();
        while(reader.NodeType == XmlNodeType.Element)
        {
          if(reader.LocalName == "tmaSolution")
          {
            shape.TMASolution = TMASolution.Load(reader);
          }
          else if(reader.LocalName == "children" && !reader.IsEmptyElement)
          {
            reader.Read(); // move to either the first child or the end element
            while(reader.NodeType == XmlNodeType.Element) shape.Children.Add(Shape.Load(reader, observers, unitsById));
            reader.ReadEndElement();
          }
          else
          {
            throw new InvalidDataException("Expected element " + reader.LocalName);
          }
        }
      }

      return shape;
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("unit");
      writer.WriteAttributeString("id", GetXmlId());
      if(!string.IsNullOrEmpty(Name)) writer.WriteAttributeString("name", Name);
      writer.WriteAttributeString("position", ManeuveringBoard.FormatXmlVector(Position));
      writer.WriteAttribute("course", Direction);
      writer.WriteAttribute("speed", Speed);

      string typeString = Type.ToString();
      writer.WriteAttributeString("type", typeString.Substring(0, 1).ToLowerInvariant() + typeString.Substring(1));

      if(TMASolution != null) TMASolution.Save(writer);

      if(Children.Count != 0)
      {
        writer.WriteStartElement("children");
        foreach(Shape shape in Children) shape.Save(writer);
        writer.WriteEndElement();
      }

      writer.WriteEndElement();
    }

    internal void SortChildren()
    {
      // we'll sort the observations based on observer, type, and time, to allow observations to find the adjacent observations in a
      // chain just by taking the observations at adjacent indices

      // make note of the indexes of all the items
      int[] indexes = new int[Children.Count];
      for(int i=0; i<indexes.Length; i++) indexes[i] = i;

      // sort the indexes
      Dictionary<UnitShape, int> observerNumbers = new Dictionary<UnitShape, int>();
      Array.Sort(indexes, (ai, bi) =>
      {
        // first sort observations after non-observations
        PositionalDataShape a = Children[ai] as PositionalDataShape, b = Children[bi] as PositionalDataShape;
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
          Children.Swap(i, indexes[i]);
          indexes[backIndexes[i]] = indexes[i];
          backIndexes[indexes[i]] = backIndexes[i];
        }
      }
    }

    #region VectorHandle
    sealed class VectorHandle : Handle
    {
      public override BoardPoint GetStartPoint(Shape shape, BoardPoint dragStart)
      {
        return shape.Position;
      }

      public override string GetStatusText(Shape shape)
      {
        UnitShape unit = (UnitShape)shape;
        return (unit.Direction * MathConst.RadiansToDegrees).ToString("f2") + "°, " + unit.Board.GetSpeedString(unit.Speed);
      }

      public override void Update(Shape shape, BoardPoint dragStart, BoardPoint dragPoint)
      {
        UnitShape unit = (UnitShape)shape;
        Vector2 vector = dragPoint - dragStart;
        unit.Direction = ManeuveringBoard.SwapBearing(vector.Angle);
        unit.Speed     = vector.Length * (1.0 / ManeuveringBoard.VectorTime);
      }
    }
    #endregion

    bool ShouldDrawMotionVector
    {
      get
      {
        Shape selected = Board.SelectedShape;
        return Board.SelectedTool != Board.TMATool ||
               selected != this && (selected == null || selected.Parent != this || !(selected is Observation));
      }
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

    static readonly VectorHandle vectorHandle = new VectorHandle();
  }
  #endregion

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
      CenterText(graphics, x, y, GetTimeString());
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteAttributeString("time", GetTimeString());
    }

    protected static void LoadPositionalData(PositionalDataShape shape, XmlReader reader)
    {
      string timeStr = reader.GetStringAttribute("time");
      Match m = timeRe.Match(reader.GetStringAttribute("time"));
      if(!m.Success) throw new System.IO.InvalidDataException("\"" + timeStr + "\" is not a valid time value.");

      int hours = 0, minutes = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), seconds = 0;
      if(m.Groups[2].Success)
      {
        hours   = minutes;
        minutes = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        if(m.Groups[3].Success) seconds = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
      }

      shape.Time = new TimeSpan(hours, minutes, seconds);
    }

    string GetTimeString()
    {
      string timeStr = ((int)Time.TotalHours).ToString() + ":" + Time.Minutes.ToString("d2");
      if(Time.Seconds != 0) timeStr += ":" + Time.Seconds.ToString("d2");
      return timeStr;
    }

    static readonly Regex timeRe = new Regex(@"^\s*(\d+)(?::(\d+)(?::(\d+))?)?\s*$");
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

    protected bool ShouldRender
    {
      get
      {
        return Board.ShowAllObservations ||
               Board.SelectedShape != null && (Board.SelectedShape == Parent || Board.SelectedShape.Parent == Parent);
      }
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteAttributeString("observer", Observer.GetXmlId());
      base.Save(writer);
    }
  }
  #endregion

  #region BearingObservation
  sealed class BearingObservation : Observation
  {
    public double Bearing { get; set; }

    public override BoardPoint Position
    {
      get { return GetEffectiveObserverPosition() + new Vector2(0, 1).Rotated(-Bearing); }
      set { Bearing = ManeuveringBoard.AngleBetween(GetEffectiveObserverPosition(), value); }
    }

    public Vector2 Vector
    {
      get { return new Vector2(0, 1).Rotated(-Bearing); }
    }

    public Line2 GetBearingLine()
    {
      return new Line2(GetEffectiveObserverPosition(), Vector);
    }

    public BoardPoint GetEffectiveObserverPosition()
    {
      return Observer.GetPositionAt(Time);
    }

    public override KeyValuePair<double, Handle> GetSelectionDistance(SysPoint point)
    {
      double distance;
      if(!ShouldRender)
      {
        distance = double.NaN;
      }
      else
      {
        BoardPoint boardPoint = Board.GetBoardPoint(point);
        BoardPoint observerPosition = GetEffectiveObserverPosition();
        Line2 bearingLine = new Line2(observerPosition, new Vector2(0, 1).Rotated(-Bearing));
        distance = Math.Abs(bearingLine.DistanceTo(boardPoint)) * Board.ZoomFactor;

        // the distance measurement considers the entire infinite line, but we only want to consider the ray starting from the observer, so
        // we'll ignore the portion of the line on the other side of the observer point
        if(distance > 4 || bearingLine.ClosestPointOnSegment(boardPoint) == bearingLine.Start &&
           boardPoint.DistanceTo(bearingLine.Start)*Board.ZoomFactor > 4)
        {
          distance = double.NaN;
        }
      }
      return new KeyValuePair<double, Handle>(distance, null);
    }

    public override void Render(Graphics graphics)
    {
      if(ShouldRender)
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

    public static BearingObservation Load(XmlReader reader, Dictionary<Observation,string> observers)
    {
      BearingObservation shape = new BearingObservation();
      LoadPositionalData(shape, reader);
      shape.Bearing = reader.GetDoubleAttribute("bearing");
      observers.Add(shape, reader.GetAttribute("observer"));
      return shape;
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("bearingObservation");
      writer.WriteAttribute("bearing", Bearing);
      base.Save(writer);
      writer.WriteEndElement();
    }
  }
  #endregion

  #region PointObservation
  sealed class PointObservation : Observation
  {
    public override BoardPoint Position { get; set; }

    public override KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point)
    {
      double distance = ShouldRender ? Board.GetBoardPoint(point).DistanceTo(Position) * Board.ZoomFactor : double.NaN;
      return new KeyValuePair<double,Handle>(distance <= 6 ? distance : double.NaN, null);
    }

    public override void Render(Graphics graphics)
    {
      if(ShouldRender)
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

    public static PointObservation Load(XmlReader reader, Dictionary<Observation,string> observers)
    {
      PointObservation shape = new PointObservation();
      LoadPositionalData(shape, reader);
      shape.Position = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("position"));
      observers.Add(shape, reader.GetAttribute("observer"));
      return shape;
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("pointObservation");
      writer.WriteAttributeString("position", ManeuveringBoard.FormatXmlVector(Position));
      base.Save(writer);
      writer.WriteEndElement();
    }
  }
  #endregion

  #region Waypoint
  sealed class Waypoint : PositionalDataShape
  {
    public Waypoint()
    {
      Time = new TimeSpan(0, 1, 0); // waypoints aren't allowed to have zero times because it leads to infinite velocities
    }

    public override BoardPoint Position { get; set; }

    public override KeyValuePair<double,Handle> GetSelectionDistance(SysPoint point)
    {
      double distance = Board.GetBoardPoint(point).DistanceTo(Position) * Board.ZoomFactor;
      return new KeyValuePair<double,Handle>(distance <= 6 ? distance : double.NaN, null);
    }

    public override void Render(Graphics graphics)
    {
      if(ShouldRender)
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

    public static Waypoint Load(XmlReader reader)
    {
      Waypoint shape = new Waypoint();
      LoadPositionalData(shape, reader);
      shape.Position = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("position"));
      return shape;
    }

    protected internal override void Save(XmlWriter writer)
    {
      writer.WriteStartElement("waypoint");
      writer.WriteAttributeString("position", ManeuveringBoard.FormatXmlVector(Position));
      base.Save(writer);
      writer.WriteEndElement();
    }

    bool ShouldRender
    {
      get
      {
        return Board.ShowAllObservations ||
               Board.ReferenceShape != null && Board.ReferenceShape == Parent ||
               Board.SelectedShape != null && (Board.SelectedShape == Parent || Board.SelectedShape.Parent == Parent);
      }
    }
  }
  #endregion
}