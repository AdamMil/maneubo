using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using AdamMil.Collections;
using AdamMil.Mathematics;
using AdamMil.Mathematics.Geometry;
using AdamMil.Mathematics.LinearAlgebra;
using AdamMil.Utilities;
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;
using BoardPoint = AdamMil.Mathematics.Geometry.Point2;
using BoardRect  = AdamMil.Mathematics.Geometry.Rectangle;
using Matrix     = AdamMil.Mathematics.Matrix;
using SysPoint   = System.Drawing.Point;
using SysRect    = System.Drawing.Rectangle;

// TODO: mouseovers (e.g. waypoint speed, course, and time to waypoint (for waypoint), observer name (for observation),
//       parent (for all shapes), radius (for circles), effective speed/course (for units having waypoints), etc.)
// TODO: allow double-clicking shapes to edit their properties
// TODO: parent/child relationships should be useful somehow
// TODO: intercepts, relative <-> absolute motion, and other specialized calculations
// TODO: make auto TMA smarter (i.e. use distance to ray instead of distance to line, respect solution locking -- use quadratic
//       programming and/or some general non-linear minimizer. we can still use the linear form when we don't need locking, and then we
//       can check the output to see if it generated a bad result [i.e. if it used a reciprocal bearing line] and switch to quadratic
//       programming if so). it would also be good if we could specify ranges for quantities rather than exact values. for instance, we
//       could specify that the speed is between 5 and 15 knots, or that the course is between 75 and 105 degrees). it may also be useful
//       to allow auto TMA to optimize a solution given by the user
// TODO: eliminate the dependency on Microsoft.SolverFoundation by somehow creating our own QP implementation
// TODO: draw more data on lines (distance, angle, ?)
// TODO: time advancement?
// TODO: unit time? (e.g. marking when a unit first comes into being on the map. observations, TMA, etc. are calculated from that time)
// TODO: only allow unit shapes to have children (?). currently, we're not saving children for any other shapes, and only unit shapes can
//       have children added from the UI
// TODO: handle unit systems
// TODO: make stopwatch time labels into text boxes and allow editing while stopped
// TODO: save/load selection and reference shape
// TODO: allow adding waypoint when no ref unit selected (maybe split off waypoint adding into a separate tool)
// TODO: allow specifying the latitude and longitude of a point, and then displaying the coordinate in the status bar. (this requires
//       a map projection -- DW uses Mercator, it looks like)

// FIXME: if the tool changes, we have to cancel mouse drag (and notify the previously selected tool)

// TODO: about observations:
// 1. there should be a way to have observations from a towed array connected to a unit (possibly parent/child to move the array
//    automatically as the parent moves)
// 2. it should be possible for the UnitShape to act as a first observation, so it should have a Time field (but then who would the
//    observer be?)

namespace Maneubo
{
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

    public void Save(XmlWriter writer)
    {
      writer.WriteStartElement("tmaSolution");
      writer.WriteAttributeString("position", ManeuveringBoard.FormatXmlVector(Position));
      writer.WriteAttributeString("velocity", ManeuveringBoard.FormatXmlVector(Velocity));
      if(LockCourse) writer.WriteAttribute("lockCourse", LockCourse);
      if(LockSpeed) writer.WriteAttribute("lockSpeed", LockSpeed);
      writer.WriteEndElement();
    }

    public static TMASolution Load(XmlReader reader)
    {
      TMASolution solution = new TMASolution();
      solution.Position = ManeuveringBoard.ParseXmlPoint(reader.GetStringAttribute("position"));
      solution.Velocity = ManeuveringBoard.ParseXmlVector(reader.GetStringAttribute("velocity"));
      solution.LockCourse = reader.GetBoolAttribute("lockCourse");
      solution.LockSpeed  = reader.GetBoolAttribute("lockSpeed");
      reader.SkipChildren();
      reader.Read();
      return solution;
    }
  }
  #endregion

  #region ManeuveringBoard
  sealed class ManeuveringBoard : MouseCanvas
  {
    public ManeuveringBoard()
    {
      AddCircleTool = new AddCircleToolClass(this);
      AddLineTool = new AddLineToolClass(this);
      AddObservationTool = new AddObservationToolClass(this);
      AddUnitTool = new AddUnitToolClass(this);
      PointerTool = new PointerToolClass(this);
      SetupBackgroundTool = new SetupBackgroundToolClass(this);
      SetupProjectionTool = new SetupMapProjectionClass(this);
      TMATool = new TMAToolClass(this);

      BackColor = DefaultBoardBackColor;
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight | ControlStyles.FixedWidth | ControlStyles.Opaque |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.ContainerControl, false);
      RootShapes = new ShapeCollection(this);
      SelectedTool = PointerTool;

      // GDI+ sometimes throws an OutOfMemoryException when drawing dashed lines. it's a known problem, but Microsoft doesn't care.
      // infuriating! >:-[  so we'll distinguish between observations and non-observations with line width instead of dash style.
      // TODO: investigate drawing with GDI rather than GDI+
      _observationColor = DefaultObservationColor;
      _referenceColor   = DefaultReferenceColor;
      _scaleColor1      = DefaultScaleColor1;
      _scaleColor2      = DefaultScaleColor2;
      _selectedColor    = DefaultSelectedColor;
      _tmaColor         = DefaultTMAColor;
      _unselectedColor  = DefaultUnselectedColor;

      normalPen = new Pen(UnselectedColor);
      normalPen.Width = 2;
      selectedPen = new Pen(SelectedColor);
      selectedPen.Width = 2;
      referencePen = new Pen(ReferenceColor);
      referencePen.Width = 2;
      tmaPen = new Pen(TMAColor);
      tmaPen.Width = 2;
      observationPen = new Pen(ObservationColor);
      selectedObservationPen = (Pen)selectedPen.Clone();

      normalBrush      = new SolidBrush(normalPen.Color);
      selectedBrush    = new SolidBrush(selectedPen.Color);
      referenceBrush   = new SolidBrush(referencePen.Color);
      observationBrush = new SolidBrush(observationPen.Color);
      scaleBrush1      = new SolidBrush(ScaleColor1);
      scaleBrush2      = new SolidBrush(ScaleColor2);

      WasChanged = false;
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
        foreach(Shape shape in this) board.OnShapeRemoved(shape);
        base.ClearItems();
      }

      protected override void InsertItem(int index, Shape item)
      {
        base.InsertItem(index, item);
        board.OnShapeAdded(item);
      }

      protected override void RemoveItem(int index, Shape item)
      {
        base.RemoveItem(index, item);
        board.OnShapeRemoved(item);
      }

      protected override void SetItem(int index, Shape item)
      {
        Shape oldShape = this[index];
        if(item != oldShape)
        {
          base.SetItem(index, item);
          board.OnShapeRemoved(oldShape);
          board.OnShapeAdded(item);
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
      public virtual void SelectionChanged(Shape previousSelection) { }
      public virtual void ReferenceChanged(UnitShape previousReference) { }
      public virtual void ShapeChanged(Shape shape) { }
      public virtual void RenderDecorations(Graphics graphics) { }

      protected ManeuveringBoard Board { get; private set; }
    }
    #endregion

    #region AddCircleToolClass
    public sealed class AddCircleToolClass : Tool
    {
      public AddCircleToolClass(ManeuveringBoard board) : base(board) { }

      public override bool MouseDragStart(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left && (Control.ModifierKeys == Keys.Shift || Board.GetShapeUnderCursor(e.Location) == null))
        {
          circle = new CircleShape() { Position = Board.GetBoardPoint(e.Location) };
          Board.RootShapes.Add(circle);
          Board.SelectedShape = circle;
          return true;
        }
        return false;
      }

      public override void MouseDrag(MouseDragEventArgs e)
      {
        if(circle != null)
        {
          circle.Radius = (Board.GetBoardPoint(e.Location) - circle.Position).Length;
          Board.OnShapeChanged(circle);
        }
      }

      public override void MouseDragEnd(MouseDragEventArgs e)
      {
        circle = null;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        // this works because MouseDrag() is called before MouseMove(), so it has the most up-to-date information
        if(circle != null) Board.StatusText = circle.GetRadiusStatusText();
      }

      CircleShape circle;
    }
    #endregion

    #region AddLineToolClass
    public sealed class AddLineToolClass : Tool
    {
      public AddLineToolClass(ManeuveringBoard board) : base(board) { }

      public override bool MouseDragStart(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left && (Control.ModifierKeys == Keys.Shift || Board.GetShapeUnderCursor(e.Location) == null))
        {
          line = new LineShape() { Start = Board.GetBoardPoint(e.Location) };
          Board.RootShapes.Add(line);
          Board.SelectedShape = line;
          return true;
        }
        return false;
      }

      public override void MouseDrag(MouseDragEventArgs e)
      {
        if(line != null)
        {
          line.End = Board.GetBoardPoint(e.Location);
          Board.OnShapeChanged(line);
        }
      }

      public override void MouseDragEnd(MouseDragEventArgs e)
      {
        line = null;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        // this works because MouseDrag() is called before MouseMove(), so it has the most up-to-date information
        if(line != null) Board.StatusText = line.GetStatusText();
      }

      LineShape line;
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
          UnitShape unit;
          // clicking on a unit selects it. this way, the user doesn't have to change tools or right-click and dismiss the menu to change
          // the target for which we'll add an observation. holding shift overrides this
          if((Control.ModifierKeys & Keys.Shift) == 0)
          {
            Shape shape = Board.GetShapeUnderCursor(e.Location);
            if(shape is UnitShape ||
               Type == PositionalDataType.BearingLine && shape is BearingObservation ||
               Type == PositionalDataType.Point && shape is PointObservation ||
               Type == PositionalDataType.Waypoint && shape is Waypoint)
            {
              Board.SelectedShape = shape;
              return true;
            }
          }

          bool project = (Control.ModifierKeys & Keys.Control) != 0;

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
            if(Board.EditPositionalData(posData))
            {
              // if the user adds a bearing line, we want to let them add it by clicking a point that it must pass through at the time of
              // the observation rather than just using the bearing from time zero
              if(Type == PositionalDataType.BearingLine && project)
              {
                double bearing = ManeuveringBoard.AngleBetween(Board.ReferenceShape.GetPositionAt(posData.Time),
                                                               Board.GetBoardPoint(e.Location));
                ((BearingObservation)posData).Bearing = bearing;
              }
              Board.SelectedShape = posData;
            }
            else
            {
              Board.DeleteShape(posData, false);
            }
            return true;
          }
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        InvalidateIfApplicable();
      }

      public override void SelectionChanged(Shape previousSelection)
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
      public AddUnitToolClass(ManeuveringBoard board) : base(board)
      {
        Type = UnitShapeType.Surface;
      }

      public UnitShapeType Type { get; set; }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          UnitShape unit;
          
          // clicking on a unit selects it. this way, the user doesn't have to change tools or right-click and dismiss the menu to change
          // selection
          if(Control.ModifierKeys != Keys.Shift)
          {
            unit = Board.GetShapeUnderCursor(e.Location) as UnitShape;
            if(unit != null)
            {
              Board.SelectedShape = unit;
              return true;
            }
          }

          HashSet<string> names = new HashSet<string>();
          foreach(Shape shape in Board.EnumerateShapes())
          {
            if(!string.IsNullOrEmpty(shape.Name)) names.Add(shape.Name);
          }

          string name = "M1";
          for(int suffix=2; names.Contains(name); suffix++) name = "M" + suffix.ToInvariantString();

          unit = new UnitShape() { Name = name, Position = Board.GetBoardPoint(e.Location), Type = Type };
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

      public override void KeyPress(KeyEventArgs e, bool down)
      {
        if(down && e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
        {
          Board.SelectedTool = Board.PointerTool;
        }
      }

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
          Board.Invalidate(); // erase the scale line
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

    #region SetupMapProjectionClass
    public sealed class SetupMapProjectionClass : Tool
    {
      public SetupMapProjectionClass(ManeuveringBoard board) : base(board) { }

      public override void Deactivate()
      {
        Board.StatusText = "";
      }

      public override bool MouseClick(MouseEventArgs e)
      {
        if(e.Button == MouseButtons.Left)
        {
          MapProjectionForm form = new MapProjectionForm();

          if(Board.Projection != null)
          {
            BoardPoint point = Board.GetGeographicalPoint(Board.GetBoardPoint(e.Location));
            form.Longitude = point.X;
            form.Latitude  = point.Y;
          }

          if(form.ShowDialog() == DialogResult.OK)
          {
            switch(form.ProjectionType)
            {
              case MapProjectionType.AzimuthalEquidistant:
                Board.Projection = new AzimuthalEquidistantProjection(form.Longitude, form.Latitude);
                break;
            }
            Board.ProjectionCenter = Board.GetBoardPoint(e.Location);
            Board.SelectedTool     = Board.PointerTool;
            return true;
          }
        }

        return false;
      }

      public override void MouseMove(MouseEventArgs e)
      {
        Board.StatusText = "Click a point to set its coordinates...";
      }
    }
    #endregion

    #region TMAToolClass
    public sealed class TMAToolClass : Tool
    {
      public TMAToolClass(ManeuveringBoard board) : base(board) { }

      public override void Activate()
      {
        SelectionChanged(null);
      }

      public override void Deactivate()
      {
        UpdateTMASolution(GetSelectedUnit());
        HideForm();
      }

      public override void KeyPress(KeyEventArgs e, bool down)
      {
        if(down && form != null)
        {
          if(e.KeyCode == Keys.Enter && e.Modifiers == Keys.None || e.KeyCode == Keys.A && e.Modifiers == Keys.Shift)
          {
            form_ApplySolution(null, null);
          }
          else if(e.KeyCode == Keys.T && e.Modifiers == Keys.Shift)
          {
            form_AutoSolve(null, null);
          }
          else if(e.KeyCode == Keys.C && e.Modifiers == Keys.Shift)
          {
            form.FocusCourse();
          }
          else if(e.KeyCode == Keys.S && e.Modifiers == Keys.Shift)
          {
            form.FocusSpeed();
          }
          else
          {
            return;
          }
          e.Handled = true;
        }
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
        if(e.Button == MouseButtons.Left && form != null) // left drag on the line moves it
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

      public override void MouseMove(MouseEventArgs e)
      {
        if(dragMode != DragMode.None)
        {
          Board.StatusText = (SwapBearing(velocity.Angle) * MathConst.RadiansToDegrees).ToString("f2") + "°, " +
                             Board.GetSpeedString(velocity.Length);
        }
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

      public override void SelectionChanged(Shape previousSelection)
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

      public override void ShapeChanged(Shape shape)
      {
        if(shape is Observation && GetSelectedUnit() != null) CalculateTimeSpan();
      }

      enum DragMode
      {
        None, Start, Middle, End
      }

      void CalculateTimeSpan()
      {
        maxTime = 0;
        foreach(BearingObservation bearing in GetSelectedUnit().Children.OfType<BearingObservation>())
        {
          double time = bearing.Time.TotalSeconds;
          if(time > maxTime) maxTime = time;
        }
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

          CalculateTimeSpan();
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

      bool SolveLeastSquares(out BoardPoint position, out Vector2 velocity)
      {
        // this method handles three cases for bearing observations:
        // 1. if nothing is locked, then we minimize cross(S) · (A+V*t-P) for each observation, where S is the normalized bearing vector
        //    (known), A is the target position (unknown), V is the target velocity (unknown), and P is the observer position (known), for
        //    a total of 4 unknowns (two per unknown point/vector). cross(X) returns a vector perpendicular to X.
        //
        // 2. if only course is locked, we can minimize cross(S) · (A+V*s*t-P) where V is a normalized constant vector and s is a new
        //    scalar variable (3 unknowns)
        //
        // 3. if both course and speed are locked, then we minimize cross(S) · (A+V*t-P) where V is a non-normalized constant vector
        //    (2 unknowns)
        //
        // if only speed is locked, we can minimize cross(S) · (A+V*s*t-P) where s is a constant and V is constrained to be normalized.
        // unfortunately, that would be a quadratic constraint, requiring a non-linear solver. alternately, we could try to formulate the
        // course using a single unknown angle θ rather than a vector, but that would also result in non-linearity due to sine and cosine,
        // i think. so the case when only speed is locked is not handled by this method
        //
        // in all cases, the bearing minimization is subject to the error in which it places the target on a reciprocal bearing (since
        // it's just the other half of the same mathematical line). we would need a constraint to enforce the right direction, but we can't
        // specify that with linear least squares. so the caller has the responsibility of checking that the result looks sane
        //
        // for point observations, we try to minimize |A+V*t-O| in cases 1 and 3, and |A+V*s*t-O| in case 2 (where O is the observation
        // point)
        //
        // since this is least squares, we actually minimize the square of the error.
        //
        // in case 1 and 3, the error is cross(S) · (A+V*t-P) = -Sy(Ax+Vx*t-Px) + Sx(Ay+Vy*t-Py) (where Vx and Vy are either variables or
        // constansts). we square this to get (-Sy(Ax+Vx*t-Px) + Sx(Ay+Vy*t-Py))^2, which expands to a rather massive equation. then we
        // take the partial derivatives for each of the variables -- Ax,Ay,Vx,Vy in case 1 and Ax,Ay in case 3. where the partial
        // derivatives are all zero (i.e. where the gradient is a zero vector), we have a stationary point of the squared error function.
        // since the squared error function of a linear expression is a kind of quadratic surface that has only a single stationary
        // point, at its minimum. (i'm not sure, but i think it's an elliptic paraboloid with a number of dimensions equal to the number
        // of unknowns.) by solving the system of equations where all the partial derivatives are zero, we can find this minimum. the
        // derivative equations work out to be (after removing an unnecessary factor of 2 and setting them equal to zero):
        //
        // Ax: -Ay*Sx*Sy + Py*Sx*Sy + Ax*Sy^2 - Px*Sy^2 + Sy^2*t*Vx - Sx*Sy*t*Vy = 0
        // Ay: Ay*Sx^2 - Py*Sx^2 - Ax*Sx*Sy + Px*Sx*Sy - Sx*Sy*t*Vx + Sx^2*t*Vy = 0
        // Vx: -Ay*Sx*Sy*t + Py*Sx*Sy*t + Ax*Sy^2*t - Px*Sy^2*t + Sy^2*t^2*Vx - Sx*Sy*t^2*Vy = 0
        // Vy: Ay*Sx^2*t - Py*Sx^2*t - Ax*Sx*Sy*t + Px*Sx*Sy*t - Sx*Sy*t^2*Vx + Sx^2*t^2*Vy = 0
        // 
        // each term should have at most one variable factor, so it can be represented as a linear equation of the variables, potentially
        // with a constant term. we'll group the coefficients (assuming Vx,Vy are variables) and move the constant term to the other side
        // of the equals sign and get:
        //
        // Ax: Ax*(Sy^2) - Ay*(Sx*Sy) + Vx*(Sy^2*t) - Vy*(Sx*Sy*t) = Px*Sy^2 - Py*Sx*Sy
        // Ay: Ay*(Sx^2) - Ax*(Sx*Sy) + Vy*(Sx^2*t) - Vx*(Sx*Sy*t) = Py*Sx^2 - Px*Sx*Sy
        // Vx: Ax*(Sy^2*t) - Ay*(Sx*Sy*t) + Vx*(Sy^2*t^2) - Vy*(Sx*Sy*t^2) = Px*Sy^2*t - Py*Sx*Sy*t
        // Vy: Ay*(Sx^2*t) - Ax*(Sx*Sy*t) + Vy*(Sx^2*t^2) - Vx*(Sx*Sy*t^2) = Py*Sx^2*t - Px*Sx*Sy*t
        //
        // if Vx and Vy are constants, their terms get moved to the other side of the equals sign as well, and the partial derivatives for
        // Vx and Vy are removed. these are the normal equations, and they can be solved to give a least squares solution to the original
        // minimization problem
        //
        // for point observations, the error is |A+V*t-O| = sqrt((Ax+Vx*t-Ox)^2+(Ay+Vy*t-Oy)^2). we want to minimize the squared error, so
        // we simply strip off the sqrt() and get (Ax+Vx*t-Ox)^2 + (Ay+Vy*t-Oy)^2. then we take the partial derivatives again, yielding:
        //
        // Ax: Ax - Ox + Vx*t = 0
        // Ay: Ay - Oy + Vy*t = 0
        // vx: Ax*t - Ox*t + Vx*t^2 = 0
        // Vy: Ay*t - Oy*t + Vy*t^2 = 0
        //
        // Ax: Ax + Vx*t = Ox
        // Ay: Ay + Vy*t = Oy
        // vx: Ax*t + Vx*t^2 = Ox*t
        // Vy: Ay*t + Vy*t^2 = Oy*t
        //
        // in case 2, the error equations are a bit different, as they include a speed variable. they work out as follows:
        //
        // minimizing squared error (cross(S) · (A+V*s*t-P))^2: 
        // Ax: Ax*(Sy^2) - Ay*(Sx*Sy) + s*(Sy^2*Vx*t - Sx*Sy*Vy*t) = Px*Sy^2 - Py*Sx*Sy
        // Ay: Ay*(Sx^2) - Ax*(Sx*Sy) + s*(Sx^2*Vy*t - Sx*Sy*Vx*t) = Py*Sx^2 - Px*Sx*Sy
        //  s: Ax*(Sy^2*Vx*t - Sx*Sy*Vy*t) - Ay*(Sx^2*Vy*t - Sx*Sy*Vx*t) + s*(Sx^2*Vy^2*t^2 + Sy^2*Vx^2*t^2 - 2Sx*Sy*Vx*Vy*t^2) =
        //     Px*Sy^2*t*Vx - Py*Sx*Sy*t*Vx + Py*Sx^2*t*Vy - Px*Sx*Sy*t*Vy
        //
        // minimizing squared error |A+V*s*t-O|^2 = (Ax+Vx*s*t-Ox)^2 + (Ay+Vy*s*t-Oy)^2:
        // Ax: Ax + s*(Vx*t) = Ox
        // Ay: Ay + s*(Vy*t) = Oy
        //  s: Ax*(Vx*t) + Ay*(Vy*t) + s*(Vx^2*t^2 + Vy^2*t^2) = Ox*Vx*t + Oy*Vy*t
        Matrix matrix;
        double[] constSum;
        UnitShape unit = GetSelectedUnit();

        if(form.LockSpeed) // if speed is locked, then both course and speed should be locked
        {
          if(!form.LockCourse) throw new InvalidOperationException(); // double-check it
          matrix   = new Matrix(2, 2);
          constSum = new double[2];
          velocity = this.velocity; // the velocity is fixed

          foreach(Observation observation in unit.Children.OfType<Observation>())
          {
            double time = observation.Time.TotalSeconds;

            BearingObservation bearing = observation as BearingObservation;
            if(bearing != null)
            {
              BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
              Vector2 bearingLine = bearing.Vector;
              double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

              matrix[0,0] += sySqr; // Ax*(Sy^2)
              matrix[0,1] += -sxy;  // -Ay*(Sx*Sy)
              // = Px*Sy^2 - Py*Sx*Sy - Vx*(Sy^2*t) + Vy*(Sx*Sy*t)
              constSum[0] += observerPosition.X*sySqr - observerPosition.Y*sxy - sySqr*time*velocity.X + sxy*time*velocity.Y;

              matrix[1,0] += -sxy;  // -Ax*(Sx*Sy)
              matrix[1,1] += sxSqr; // +Ay*(Sx^2)
              // = Py*Sx^2 - Px*Sx*Sy - Vy*(Sx^2*t) + Vx*(Sx*Sy*t)
              constSum[1] += observerPosition.Y*sxSqr - observerPosition.X*sxy + sxy*time*velocity.X - sxSqr*time*velocity.Y;
            }

            PointObservation point = observation as PointObservation;
            if(point != null)
            {
              matrix[0,0] += 1; // Ax
              constSum[0] += point.Position.X - velocity.X*time; // = Ox - Vx*t

              matrix[1,0] += 1; // Ay
              constSum[1] += point.Position.Y - velocity.Y*time; // = Oy - Vy*t
            }
          } 
        }
        else if(form.LockCourse) // otherwise, only course is locked
        {
          Matrix3 m = new Matrix3();
          constSum = new double[3];

          velocity = this.velocity.Normal; // get the normalized velocity, which we'll incorporate into the equations

          foreach(Observation observation in unit.Children.OfType<Observation>())
          {
            double time = observation.Time.TotalSeconds, timeSqr = time*time;

            BearingObservation bearing = observation as BearingObservation;
            if(bearing != null)
            {
              BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
              Vector2 bearingLine = bearing.Vector;
              double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

              m.M00 += sySqr;                                    // Ax*(Sy^2)
              m.M01 += -sxy;                                     // -Ay*(Sx*Sy)
              m.M02 += time*(sySqr*velocity.X - sxy*velocity.Y); // +s*(Sy^2*Vx*t - Sx*Sy*Vy*t)
              constSum[0] += observerPosition.X*sySqr - observerPosition.Y*sxy; // = Px*Sy^2 - Py*Sx*Sy

              m.M10 += -sxy;                                     // -Ax*(Sx*Sy)
              m.M11 += sxSqr;                                    // +Ay*(Sx^2)
              m.M12 += time*(sxSqr*velocity.Y - sxy*velocity.X); // +s*(Sx^2*Vy*t - Sx*Sy*Vx*t)
              constSum[1] += observerPosition.Y*sxSqr - observerPosition.X*sxy; // = Py*Sx^2 - Px*Sx*Sy

              m.M20 += time*(sySqr*velocity.X - sxy*velocity.Y); // Ax*(Sy^2*Vx*t - Sx*Sy*Vy*t)
              m.M21 += time*(sxy*velocity.X - sxSqr*velocity.Y); // -Ay*(Sx^2*Vy*t - Sx*Sy*Vx*t)
              // +s*(Sx^2*Vy^2*t^2 + Sy^2*Vx^2*t^2 - 2Sx*Sy*Vx*Vy*t^2)
              m.M22 += timeSqr*(sxSqr*velocity.Y*velocity.Y + sySqr*velocity.X*velocity.X - 2*sxy*velocity.X*velocity.Y);
              // = Px*Sy^2*t*Vx - Py*Sx*Sy*t*Vx + Py*Sx^2*t*Vy - Px*Sx*Sy*t*Vy = t*(Vx*(Px*Sy^2 - Py*Sx*Sy) + Vy*(Py*Sx^2 - Px*Sx*Sy))
              constSum[2] += time * (velocity.X * (observerPosition.X*sySqr - observerPosition.Y*sxy) +
                                     velocity.Y * (observerPosition.Y*sxSqr - observerPosition.X*sxy));
            }

            PointObservation point = observation as PointObservation;
            if(point != null)
            {
              m.M00 += 1;    // Ax
              m.M02 += velocity.X*time; // +s*(Vx*t)
              constSum[0] += point.Position.X; // = Ox

              m.M11 += 1;    // Ay
              m.M12 += velocity.Y*time; // +S*(Vy*t)
              constSum[1] += point.Position.Y; // = Oy

              m.M20 += velocity.X*time; // Ax*(Vx*t)
              m.M21 += velocity.Y*time; // +Ay*(Vy*t)
              m.M22 += timeSqr * (velocity.X*velocity.X + velocity.Y*velocity.Y); // + s*(Vx^2*t^2 + Vy^2*t^2)
              constSum[2] += time*(point.Position.X*velocity.X + point.Position.Y*velocity.Y); // = Ox*Vx*t + Oy*Vy*t
            }
          }

          matrix = m.ToMatrix();
        }
        else // otherwise, nothing is locked
        {
          Matrix4 m = new Matrix4();
          constSum = new double[4];
          velocity = new Vector2(); // ensure 'velocity' is assigned, to pacify the compiler

          foreach(Observation observation in unit.Children.OfType<Observation>())
          {
            double time = observation.Time.TotalSeconds, timeSqr = time*time;
            BearingObservation bearing = observation as BearingObservation;
            if(bearing != null)
            {
              BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
              Vector2 bearingLine = bearing.Vector;
              double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

              m.M00 += sySqr;      // Ax*(Sy^2)
              m.M01 += -sxy;       // -Ay*(Sx*Sy)
              m.M02 += sySqr*time; // +Vx*(Sy^2*t)
              m.M03 += -sxy*time;  // -Vy*(Sx*Sy*t)
              constSum[0] += observerPosition.X*sySqr - observerPosition.Y*sxy; // = Px*Sy^2 - Py*Sx*Sy

              m.M10 += -sxy;        // -Ax*(Sx*Sy)
              m.M11 += sxSqr;       // +Ay*(Sx^2)
              m.M12 += -time*sxy;   // -Vx*(Sx*Sy*t)
              m.M13 += time*sxSqr;  // +Vy*(Sx^2*t)
              constSum[1] += observerPosition.Y*sxSqr - observerPosition.X*sxy; // = Py*Sx^2 - Px*Sx*Sy

              m.M20 += time*sySqr;    // Ax*(Sy^2*t)
              m.M21 += -time*sxy;     // -Ay*(Sx*Sy*t)
              m.M22 += timeSqr*sySqr; // +Vx*(Sy^2*t^2)
              m.M23 += -timeSqr*sxy;  // -Vy*(Sx*Sy*t^2)
              constSum[2] += observerPosition.X*time*sySqr - observerPosition.Y*time*sxy; // = Px*Sy^2*t - Py*Sx*Sy*t

              m.M30 += -time*sxy;     // -Ax*(Sx*Sy*t)
              m.M31 += time*sxSqr;    // +Ay*(Sx^2*t)
              m.M32 += -timeSqr*sxy;  // -Vx*(Sx*Sy*t^2)
              m.M33 += timeSqr*sxSqr; // +Vy*(Sx^2*t^2)
              constSum[3] += observerPosition.Y*time*sxSqr - observerPosition.X*time*sxy; // = Py*Sx^2*t - Px*Sx*Sy*t
            }

            PointObservation point = observation as PointObservation;
            if(point != null)
            {
              m.M00 += 1;    // Ax
              m.M02 += time; // +Vx*t
              constSum[0] += point.Position.X; // = Ox

              m.M11 += 1;    // Ay
              m.M13 += time; // +Vy*t
              constSum[1] += point.Position.Y; // = Oy

              m.M20 += time;    // Ax*t
              m.M22 += timeSqr; // +Vx*t^2
              constSum[2] += time*point.Position.X; // = Ox*t

              m.M31 += time;    // Ay*t
              m.M33 += timeSqr; // +Vy*t^2
              constSum[3] += time*point.Position.Y; // = Oy*t
            }
          }

          matrix = m.ToMatrix();
        }

        try
        {
          // now that we have our matrix, we'll try to solve it via LU decomposition. other methods would also work
          Matrix values = new LUDecomposition(matrix).Solve(new Matrix(constSum, 1));

          position = new BoardPoint(values[0, 0], values[1, 0]);
          if(matrix.Width == 4) // if we have the velocity as two variables...
          {
            velocity = new Vector2(values[2, 0], values[3, 0]); // construct it from them directly
          }
          else if(matrix.Width == 3) // if we have the velocity as a normal times a speed...
          {
            velocity *= values[2, 0]; // multiply the normal by the speed
          }
          if(velocity.Length < 0.000001) velocity = Vector2.Zero; // GDI+ crashes when rendering short lines, so avoid small speeds

          return true;
        }
        catch(InvalidOperationException) // the matrix was singular, and couldn't be solved
        {
          position = new BoardPoint();
          return false;
        }
      }

      bool SolveQP(out BoardPoint position, out Vector2 velocity)
      {
        // this method handles three cases for bearing observations:
        // 1. if nothing is locked, then we minimize cross(S) · (A+V*t-P) for each observation, where S is the normalized bearing vector
        //    (known), A is the target position (unknown), V is the target velocity (unknown), and P is the observer position (known), for
        //    a total of 4 unknowns (two per unknown point/vector). cross(X) returns a vector perpendicular to X.
        //
        // 2. if only course is locked, we can minimize cross(S) · (A+V*s*t-P) where V is a normalized constant vector and s is a new
        //    scalar variable (3 unknowns)
        //
        // 3. if both course and speed are locked, then we minimize cross(S) · (A+V*t-P) where V is a non-normalized constant vector
        //    (2 unknowns)
        //
        // if only speed is locked, we can minimize cross(S) · (A+V*s*t-P) where s is a constant and V is constrained to be normalized.
        // unfortunately, that would be a quadratic constraint, and cannot be solved with quadratic programming, which allows only linear
        // constraints
        //
        // the above formulas are subject to the error in which it places the target on a reciprocal bearing (since it's just the other
        // half of the same mathematical line). to deal with this, we add constrain S · (A+V*t-P) to be non-negative. this has the result
        // of ensuring that it places the target in the direction in which the bearing ray points
        //
        // for point observations, we try to minimize |A+V*t-O| in cases 1 and 3, and |A+V*s*t-O| in case 2 (where O is the observation
        // point)
        //
        // in all cases, we actually minimize the square of the error
        //
        // in case 1 and 3, the error is cross(S) · (A+V*t-P) = -Sy(Ax+Vx*t-Px) + Sx(Ay+Vy*t-Py) (where Vx and Vy are either variables or
        // constansts). we square this to get (-Sy(Ax+Vx*t-Px) + Sx(Ay+Vy*t-Py))^2, which expands to a rather massive equation. our goal
        // in quadratic programming is to simply find the coefficients of all the variables and products of two variables. for instance,
        // given the formula a*x^2 + b*x + c*d*x*y - e*y^2 + f, for variables x and y, we would have the following: x^2 whose coefficient
        // is a, x whose coefficient is b, x*y whose coefficient is c*d, and y^2 whose coefficient is -e. we can ignore all terms that
        // don't involve any variables, since they merely offset the solution by some constant and don't affect the values of the
        // variables that correspond to the minimum.
        InteriorPointSolver solver = new InteriorPointSolver();
        UnitShape unit = GetSelectedUnit();

        try
        {
          if(!form.LockCourse)
          {
            if(form.LockSpeed) throw new InvalidOperationException();

            // finding the coefficients for case 1 bearing observations produces:
            // Ax:    Py*Sx*Sy - Px*Sy^2
            // Ax^2:  0.5 * Sy^2
            // Ax*Ay: -Sx * Sy
            // Ax*Vx: t * Sy^2
            // Ax*Vy: -Sx * Sy * t
            // Ay:    Px*Sx*Sy - Py*Sx^2
            // Ay^2:  0.5 * Sx^2
            // Ay*Vx: -Sx * Sy * t
            // Ay*Vy: t * Sx^2
            // Vx:    Py*Sx*Sy*t - Px*t*Sy^2
            // Vx^2:  0.5 * Sy^2 * t^2
            // Vx*Vy: -Sx * Sy * t^2
            // Vy:    Px*Sx*Sy*t - Py*t*Sx^2
            // Vy^2:  0.5 * Sx^2 * t^2
            //
            // for point observations, we get:
            // Ax:    -Ox
            // Ax^2:  0.5
            // Ax*Vx: t
            // Ay:    -Oy
            // Ay^2:  0.5
            // Ay*Vy: t
            // Vx:    -Ox*t
            // Vx^2:  0.5 * t^2
            // Vy:    -Oy*t
            // Vy^2:  0.5 * t^2
            int iax, iay, ivx, ivy;
            solver.AddVariable("ax", out iax);
            solver.AddVariable("ay", out iay);
            solver.AddVariable("vx", out ivx);
            solver.AddVariable("vy", out ivy);

            double ax=0, axSqr=0, axay=0, axvx=0, axvy=0, ay=0, aySqr=0, ayvx=0, ayvy=0, vx=0, vxSqr=0, vxvy=0, vy=0, vySqr=0;
            foreach(Observation observation in unit.Children.OfType<Observation>())
            {
              double time = observation.Time.TotalSeconds, timeSqr = time*time;

              BearingObservation bearing = observation as BearingObservation;
              if(bearing != null)
              {
                BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
                Vector2 bearingLine = bearing.Vector;
                double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;
                double sxyt = sxy * time;

                ax    += observerPosition.Y * sxy - observerPosition.X * sySqr; // py*sx*sy - px*sy^2
                axSqr += 0.5 * sySqr; // 0.5 * sy^2
                axay  -= sxy; // -sx*sy
                axvx  += time * sySqr; // t * sy^2
                axvy  -= sxyt; // -sx*sy*t
                ay    += observerPosition.X * sxy - observerPosition.Y * sxSqr; // px*sx*sy - py*sx^2
                aySqr += 0.5 * sxSqr; // 0.5 * sx^2
                ayvx  -= sxyt; // -sx*sy*t
                ayvy  += time * sxSqr; // t*sx^2
                vx    += observerPosition.Y * sxyt - observerPosition.X * time * sySqr; // py*sx*sy*t - px*t*sy^2
                vxSqr += 0.5 * sySqr * timeSqr; // 0.5 * sy^2 * t^2
                vxvy  -= sxy * timeSqr; // -sx * sy * t^2
                vy    += observerPosition.X * sxyt - observerPosition.Y * time * sxSqr; // px*sx*sy*t - py*t*sx^2
                vySqr += 0.5 * sxSqr * timeSqr; // Vy^2: 0.5 * sx^2 * t^2

                // Ax*sx + Ay*sy + sx*t*Vx + sy*t*Vy >= px*sx + py*sy
                int row;
                solver.AddRow(bearing, out row);
                solver.SetCoefficient(row, iax, bearingLine.X);
                solver.SetCoefficient(row, iay, bearingLine.Y);
                solver.SetCoefficient(row, ivx, bearingLine.X * time);
                solver.SetCoefficient(row, ivy, bearingLine.Y * time);
                solver.SetBounds(row, observation.Position.X*bearingLine.X + observation.Position.Y*bearingLine.Y,
                                 Rational.PositiveInfinity);
              }

              PointObservation point = observation as PointObservation;
              if(point != null)
              {
                ax    -= observation.Position.X;
                axSqr += 0.5;
                axvx  += time;
                ay    -= observation.Position.Y;
                ayvy  += time;
                aySqr += 0.5;
                vx    -= observation.Position.X * time;
                vy    -= observation.Position.Y * time;
                vxSqr += 0.5 * timeSqr;
                vySqr += 0.5 * timeSqr;
              }
            }

            int goal;
            solver.AddRow("error", out goal);
            solver.SetCoefficient(goal, iax, ax);
            solver.SetCoefficient(goal, axSqr, iax, iax);
            solver.SetCoefficient(goal, axay, iax, iay);
            solver.SetCoefficient(goal, axvx, iax, ivx);
            solver.SetCoefficient(goal, axvy, iax, ivy);
            solver.SetCoefficient(goal, iay, ay);
            solver.SetCoefficient(goal, aySqr, iay, iay);
            solver.SetCoefficient(goal, ayvx, iay, ivx);
            solver.SetCoefficient(goal, ayvy, iay, ivy);
            solver.SetCoefficient(goal, ivx, vx);
            solver.SetCoefficient(goal, vxSqr, ivx, ivx);
            solver.SetCoefficient(goal, vxvy, ivx, ivy);
            solver.SetCoefficient(goal, ivy, vy);
            solver.SetCoefficient(goal, vySqr, ivy, ivy);
            solver.AddGoal(goal, 0, true);

            ILinearSolution solution = solver.Solve(new InteriorPointSolverParams());
            if(solution.Result == LinearResult.Optimal || solution.Result == LinearResult.Feasible)
            {
              position = new BoardPoint((double)solution.GetValue(iax), (double)solution.GetValue(iay));
              velocity = new Vector2((double)solution.GetValue(ivx), (double)solution.GetValue(ivy));
              if(velocity.Length < 0.000001) velocity = Vector2.Zero; // GDI+ crashes when rendering short lines, so avoid small speeds
              return true;
            }
          }
          else if(!form.LockSpeed)
          {
            // in case 2, the error equations are a bit different, as they include a speed variable. for bearings, they work out as follows:
            // Ax:    Py*Sx*Sy - Px*Sy^2
            // Ax^2:  0.5 * Sy^2
            // Ax*Ay: -Sx * Sy
            // Ax*s:  Vx*t*Sy^2 - Vy*t*Sx*Sy
            // Ay:    Px*Sx*Sy - Py*Sx^2
            // Ay^2:  0.5 * Sx^2
            // Ay*s:  Vy*t*Sx^2 - Vx*t*Sx*Sy
            // s:     t * (Py*Sx*Sy*Vx + Px*Sx*Sy*Vy - Px*Vx*Sy^2 - Py*Vy*Sx^2) ***
            // s^2:   0.5 * t^2 * (Sy^2*Vx^2 - Sx*Sy*Vx*Vy + Sx^2*Vy^2)
            //
            // for point observations, we get:
            // Ax:   -Ox
            // Ax^2: 0.5
            // Ax*s: Vx * t
            // Ay:   -Oy
            // Ay^2: 0.5
            // Ay*s: Vy * t
            // s:    -t * (Ox*Vx + Oy*Vy)
            // s^2:  0.5 * t^2 * (Vx^2 + Vy^2)
            int iax, iay, ispeed;
            solver.AddVariable("ax", out iax);
            solver.AddVariable("ay", out iay);
            solver.AddVariable("speed", out ispeed);

            velocity = this.velocity.Normal;
            double ax=0, axSqr=0, axay=0, axs=0, ay=0, aySqr=0, ays=0, s=0, sSqr=0;
            foreach(Observation observation in unit.Children.OfType<Observation>())
            {
              double time = observation.Time.TotalSeconds;

              BearingObservation bearing = observation as BearingObservation;
              if(bearing != null)
              {
                BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
                Vector2 bearingLine = bearing.Vector;
                double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

                ax    += observerPosition.Y * sxy - observerPosition.X * sySqr; // py*sx*sy - px*sy^2
                axSqr += 0.5 * sySqr; // 0.5 * sy^2
                axay  -= sxy; // -sx*sy
                axs   += velocity.X*time*sySqr - velocity.Y*time*sxy; // Vx*t*Sy^2 - Vy*t*Sx*Sy
                ay    += observerPosition.X * sxy - observerPosition.Y * sxSqr; // px*sx*sy - py*sx^2
                aySqr += 0.5 * sxSqr; // 0.5 * sx^2
                ays   += velocity.Y*time*sxSqr - velocity.X*time*sxy;
                s     += time * (velocity.X * (observation.Position.Y*sxy - observation.Position.X*sySqr) +
                                 velocity.Y * (observation.Position.X*sxy - observation.Position.Y*sxSqr));
                sSqr  += 0.5 * time*time * (sySqr*velocity.X*velocity.X + sxSqr*velocity.Y*velocity.Y - sxy*velocity.X*velocity.Y);

                // Ax*sx + Ay*sy + sx*t*s*Vx + sy*t*s*Vy >= px*sx + py*sy
                int row;
                solver.AddRow(bearing, out row);
                solver.SetCoefficient(row, iax, bearingLine.X);
                solver.SetCoefficient(row, iay, bearingLine.Y);
                solver.SetCoefficient(row, ispeed, time * (bearingLine.X*velocity.X + bearingLine.Y*velocity.Y));
                solver.SetBounds(row, observation.Position.X*bearingLine.X + observation.Position.Y*bearingLine.Y,
                                 Rational.PositiveInfinity);
              }

              PointObservation point = observation as PointObservation;
              if(point != null)
              {
                ax    -= observation.Position.X;
                axSqr += 0.5;
                axs   += velocity.X * time;
                ay    -= observation.Position.Y;
                ays   += velocity.Y * time;
                aySqr += 0.5;
                s     -= time * (observation.Position.X*velocity.X + observation.Position.Y*velocity.Y);
                sSqr  += 0.5 * time*time * (velocity.X*velocity.X + velocity.Y*velocity.Y);
              }
            }

            int goal;
            solver.AddRow("error", out goal);
            solver.SetCoefficient(goal, iax, ax);
            solver.SetCoefficient(goal, axSqr, iax, iax);
            solver.SetCoefficient(goal, axay, iax, iay);
            solver.SetCoefficient(goal, axs, iax, ispeed);
            solver.SetCoefficient(goal, iay, ay);
            solver.SetCoefficient(goal, aySqr, iay, iay);
            solver.SetCoefficient(goal, ays, iay, ispeed);
            solver.SetCoefficient(goal, ispeed, s);
            solver.SetCoefficient(goal, sSqr, ispeed, ispeed);
            solver.AddGoal(goal, 0, true);

            ILinearSolution solution = solver.Solve(new InteriorPointSolverParams());
            if(solution.Result == LinearResult.Optimal || solution.Result == LinearResult.Feasible)
            {
              position = new BoardPoint((double)solution.GetValue(iax), (double)solution.GetValue(iay));
              velocity *= (double)solution.GetValue(ispeed);
              if(velocity.Length < 0.000001) velocity = Vector2.Zero; // GDI+ crashes when rendering short lines, so avoid small speeds
              return true;
            }
          }
          else
          {
            // in case 3, for bearings, we get:
            // Ax:    Py*Sx*Sy - Px*Sy^2 - Sx*Sy*Vy*t + Sy^2*Vx*t
            // Ax^2:  0.5 * Sy^2
            // Ax*Ay: -Sx*Sy
            // Ay:    Px*Sx*Sy - Py*Sx^2 - Sx*Sy*Vx*t + Sx^2*Vy*t
            // Ay^2:  0.5 * Sx^2
            // 
            // for point observations, we get:
            // Ax: Vx*t - Ox
            // Ay: Vy*t - Oy
            // Ax^2: 0.5
            // Ay^2: 0.5
            int iax, iay;
            solver.AddVariable("ax", out iax);
            solver.AddVariable("ay", out iay);

            velocity = this.velocity;
            double ax=0, axSqr=0, axay=0, ay=0, aySqr=0;
            foreach(Observation observation in unit.Children.OfType<Observation>())
            {
              double time = observation.Time.TotalSeconds;

              BearingObservation bearing = observation as BearingObservation;
              if(bearing != null)
              {
                BoardPoint observerPosition = bearing.GetEffectiveObserverPosition();
                Vector2 bearingLine = bearing.Vector;
                double sxy = bearingLine.X*bearingLine.Y, sxSqr = bearingLine.X*bearingLine.X, sySqr = bearingLine.Y*bearingLine.Y;

                ax    += observerPosition.Y*sxy - observerPosition.X*sySqr - time * (sxy*velocity.Y - sySqr*velocity.X);
                axSqr += 0.5 * sySqr; // 0.5 * sy^2
                axay  -= sxy; // -sx*sy
                ay    += observerPosition.X*sxy - observerPosition.Y*sxSqr - time * (sxy*velocity.X - sxSqr*velocity.Y);
                aySqr += 0.5 * sxSqr; // 0.5 * sx^2

                // Ax*sx + Ay*sy >= px*sx + py*sy - sx*t*Vx - sy*t*Vy
                int row;
                solver.AddRow(bearing, out row);
                solver.SetCoefficient(row, iax, bearingLine.X);
                solver.SetCoefficient(row, iay, bearingLine.Y);
                solver.SetBounds(row, observation.Position.X*bearingLine.X + observation.Position.Y*bearingLine.Y -
                                 time * (bearingLine.X*velocity.X + bearingLine.Y*velocity.Y), Rational.PositiveInfinity);
              }

              PointObservation point = observation as PointObservation;
              if(point != null)
              {
                ax    += velocity.X*time - observation.Position.X;
                axSqr += 0.5;
                ay    += velocity.Y*time - observation.Position.Y;
                aySqr += 0.5;
              }
            }

            int goal;
            solver.AddRow("error", out goal);
            solver.SetCoefficient(goal, iax, ax);
            solver.SetCoefficient(goal, axSqr, iax, iax);
            solver.SetCoefficient(goal, axay, iax, iay);
            solver.SetCoefficient(goal, iay, ay);
            solver.SetCoefficient(goal, aySqr, iay, iay);
            solver.AddGoal(goal, 0, true);

            ILinearSolution solution = solver.Solve(new InteriorPointSolverParams());
            if(solution.Result == LinearResult.Optimal || solution.Result == LinearResult.Feasible)
            {
              position = new BoardPoint((double)solution.GetValue(iax), (double)solution.GetValue(iay));
              return true;
            }
          }
        }
        catch(InvalidModelDataException) { } // if the model isn't convex, just fail

        position = new BoardPoint();
        velocity = new Vector2();
        return false;
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
        BoardPoint position;
        Vector2 velocity;

        // except for the case where only speed is locked, we can first attempt a linear least squares solution. in reality, we also have
        // linear inequality constraints that it must adhere to (in particular, that it's using the bearing rays and not any reciprocal
        // bearing rays), but it often gets a solution that happens to comply with the constraints. we'll check the solution against the
        // constraints, and if the constraints are violated, we'll invoke a more complex solver. (also, if only speed is locked, we must
        // invoke a more complex solver.) this may not not be perfect, as it might be the case that a positional observation mixed in can
        // pull the solution in the right direction enough to cause it to pass the check below, despite the solution being non-optimal
        // against the bearing lines. (i'm not sure of this.) but if so, it should be rare enough that we can disregard it for now.
        //
        // TODO: if we decide to keep the QP implementation, then we can just use it all the time. no need, really, to use the least
        // squares solution at all. on the other hand, the least squares solution can usually generate a result when the solution is
        // ambiguous, in which case we might want to use it as a backup to the QP algorithm, if the user doesn't mind
        bool solutionSucceeded;
        if(form.LockCourse || !form.LockSpeed)
        {
          solutionSucceeded = SolveLeastSquares(out position, out velocity);
          if(solutionSucceeded) // if the solution succeeded, check it against the constraints
          {
            foreach(BearingObservation bearing in GetSelectedUnit().Children.OfType<BearingObservation>())
            {
              // if we take the dot product between the bearing line and the vector to the target, we get the cosine of the angle between
              // them times their magnitudes. what really matters is the sign of the result. for angles from -90 to +90, the cosine of the
              // angle will be non-negative. this represents the area in the direction that the bearing line points (or perpendicular to
              // it). if the answer is negative, it represents the area away from which the bearing line points, meaning that the solution
              // may have put the target on a reciprocal bearing line. it may also mean that the best fit just happened to have one point
              // on the wrong side of the observer. we can't tell these apart, so we need to invoke a solver that can handle constraints
              BoardPoint targetPos = position + velocity*bearing.Time.TotalSeconds;
              if(bearing.Vector.DotProduct(targetPos - bearing.GetEffectiveObserverPosition()) < 0)
              {
                solutionSucceeded = SolveQP(out position, out velocity); // try using a quadratic programming solver
                break;
              }
            }
          }
        }
        else
        {
          MessageBox.Show("Auto TMA with only speed locked is not supported yet. Either unlock speed or lock both speed and course.",
                          "Not implemented", MessageBoxButtons.OK, MessageBoxIcon.Error);
          solutionSucceeded = false;
          goto done;
        }

        if(!solutionSucceeded)
        {
          MessageBox.Show("Unable to find a single TMA solution. More or better observational data are needed.", "No TMA solution",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
          this.position = position;
          this.velocity = velocity;
          OnArrowMoved();
        }

        done:
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

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BoardPoint BackgroundImageCenter
    {
      get { return _bgCenter; }
      set
      {
        if(value != BackgroundImageCenter)
        {
          _bgCenter = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double BackgroundImageScale
    {
      get { return _bgScale; }
      set
      {
        if(value != BackgroundImageScale)
        {
          if(value <= 0) throw new ArgumentOutOfRangeException();
          _bgScale = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ObservationColor
    {
      get { return _observationColor; }
      set
      {
        if(value != ObservationColor)
        {
          _observationColor = value;
          observationPen.Color   = value;
          observationBrush.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ReferenceColor
    {
      get { return _referenceColor; }
      set
      {
        if(value != ReferenceColor)
        {
          _referenceColor = value;
          referencePen.Color   = value;
          referenceBrush.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ScaleColor1
    {
      get { return _scaleColor1; }
      set
      {
        if(value != ScaleColor1)
        {
          _scaleColor1 = value;
          scaleBrush1.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ScaleColor2
    {
      get { return _scaleColor2; }
      set
      {
        if(value != ScaleColor2)
        {
          _scaleColor2 = value;
          scaleBrush2.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SelectedColor
    {
      get { return _selectedColor; }
      set
      {
        if(value != SelectedColor)
        {
          _selectedColor = value;
          selectedPen.Color   = value;
          selectedBrush.Color = value;
          selectedObservationPen.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TMAColor
    {
      get { return _tmaColor; }
      set
      {
        if(value != TMAColor)
        {
          _tmaColor = value;
          tmaPen.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color UnselectedColor
    {
      get { return _unselectedColor; }
      set
      {
        if(value != UnselectedColor)
        {
          _unselectedColor = value;
          normalPen.Color   = value;
          normalBrush.Color = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string StatusText
    {
      get { return _statusText; }
      private set
      {
        if(value == null) value = string.Empty;
        if(value != StatusText)
        {
          _statusText = value;
          OnStatusTextChanged();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MapProjection Projection
    {
      get { return _projection; }
      set
      {
        if(value != Projection)
        {
          _projection = value;
          WasChanged = true;
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BoardPoint ProjectionCenter
    {
      get { return _projectionCenter; }
      set
      {
        if(value != ProjectionCenter)
        {
          _projectionCenter = value;
          WasChanged = true;
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UnitShape ReferenceShape
    {
      get { return _referenceShape; }
      set
      {
        if(value != ReferenceShape)
        {
          if(value != null && value.Board != this) throw new ArgumentException("The shape does not belong to this maneuvering board.");
          UnitShape previousReference = ReferenceShape;
          _referenceShape = value;
          OnReferenceShapeChanged(previousReference);
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ShapeCollection RootShapes { get; private set; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Tool SelectedTool
    {
      get { return _currentTool; }
      set
      {
        if(value != SelectedTool)
        {
          if(value == null) throw new ArgumentNullException();
          if(!IsValidTool(value)) throw new InvalidOperationException();
          Tool previousTool = SelectedTool;
          Cursor = Cursors.Default;
          _currentTool = value;
          if(previousTool != null) previousTool.Deactivate();
          SelectedTool.Activate();
          OnToolChanged();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowAllObservations
    {
      get { return _showAllObservations; }
      set
      {
        if(value != ShowAllObservations)
        {
          _showAllObservations = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UnitSystem UnitSystem
    {
      get { return _unitSystem; }
      set
      {
        if(value != UnitSystem)
        {
          _unitSystem = value;
          WasChanged = true;
          Invalidate();
        }
      }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool WasChanged { get; set; }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    public void Clear()
    {
      SelectedTool = PointerTool;
      ReferenceShape = null;
      SelectedShape = null;
      RootShapes.Clear();
      BackgroundImage = null;
      Center = BackgroundImageCenter = BoardPoint.Empty;
      BackgroundImageScale = ZoomFactor = 1;
      WasChanged = false;
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

    public BoardPoint GetGeographicalPoint(BoardPoint point)
    {
      if(Projection == null) throw new InvalidOperationException();
      double longitude, latitude;
      Projection.Unproject(point.X - ProjectionCenter.X, ProjectionCenter.Y - point.Y, out longitude, out latitude);
      return new BoardPoint(longitude, latitude);
    }

    public PointF GetRenderPoint(BoardPoint point)
    {
      return new PointF((float)((point.X-Center.X)*ZoomFactor + Width*0.5), (float)(Height*0.5 - (point.Y-Center.Y)*ZoomFactor));
    }

    public Shape GetShapeUnderCursor(SysPoint point)
    {
      Handle handle;
      return GetShapeUnderCursor(point, out handle);
    }

    public Shape GetShapeUnderCursor(SysPoint point, out Handle handle)
    {
      List<KeyValuePair<Shape, KeyValuePair<double, Handle>>> shapes = new List<KeyValuePair<Shape, KeyValuePair<double, Handle>>>();
      foreach(Shape shape in EnumerateShapes())
      {
        KeyValuePair<double,Handle> distance = shape.GetSelectionDistance(point);
        if(!double.IsNaN(distance.Key)) shapes.Add(new KeyValuePair<Shape, KeyValuePair<double, Handle>>(shape, distance));
      }

      if(shapes.Count == 0)
      {
        handle = null;
        return null;
      }
      else
      {
        shapes.Sort((a, b) => // sort the shapes to establish priority
        {
          // consider unit shapes before other types of shapes
          if(a.Key is UnitShape)
          {
            if(!(b.Key is UnitShape)) return -1;
          }
          else if(b.Key is UnitShape) return 1;

          // then sort by distance
          return a.Value.Key.CompareTo(b.Value.Key);
        });

        handle = shapes[0].Value.Value;
        return shapes[0].Key;
      }
    }

    public void Load(string fileName)
    {
      XmlReaderSettings settings = new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true };
      using(XmlReader reader = XmlReader.Create(fileName, settings))
      {
        if(!reader.Read() || reader.NodeType == XmlNodeType.XmlDeclaration && !reader.Read() || !reader.LocalName.OrdinalEquals("moboard"))
        {
          throw new InvalidDataException("Unrecognized file format.");
        }
        else if(reader.GetInt32Attribute("version") != 1)
        {
          throw new InvalidDataException("The file is not supported by this version of Maneubo.");
        }
        else
        {
          try
          {
            Clear();

            ShowAllObservations = reader.GetBoolAttribute("showAllObservations");
            BackColor        = ParseColor(reader.GetAttribute("backgroundColor"),  DefaultBoardBackColor);
            ObservationColor = ParseColor(reader.GetAttribute("observationColor"), DefaultObservationColor);
            ReferenceColor   = ParseColor(reader.GetAttribute("referenceColor"),   DefaultReferenceColor);
            ScaleColor1      = ParseColor(reader.GetAttribute("scaleColor1"),      DefaultScaleColor1);
            ScaleColor2      = ParseColor(reader.GetAttribute("scaleColor2"),      DefaultScaleColor2);
            SelectedColor    = ParseColor(reader.GetAttribute("selectedColor"),    DefaultSelectedColor);
            TMAColor         = ParseColor(reader.GetAttribute("tmaColor"),         DefaultTMAColor);
            UnselectedColor  = ParseColor(reader.GetAttribute("unselectedColor"),  DefaultUnselectedColor);
            UnitSystem       = ParseUnitSystem(reader.GetAttribute("unitSystem"), UnitSystem.NauticalMetric);
            Projection       = null;

            Dictionary<Observation, string> observers = new Dictionary<Observation, string>();
            Dictionary<string, UnitShape> unitsById = new Dictionary<string, UnitShape>();
            while(reader.Read() && reader.NodeType == XmlNodeType.Element)
            {
              switch(reader.LocalName)
              {
                case "projection":
                {
                  ProjectionCenter = ParseXmlPoint(reader.GetAttribute("mapPoint"));
                  BoardPoint lonLat = ParseXmlPoint(reader.GetAttribute("lonLat"));
                  switch(reader.GetStringAttribute("type", "").ToLowerInvariant())
                  {
                    case "azimuthalequidistant":
                      Projection = new AzimuthalEquidistantProjection(lonLat.X, lonLat.Y);
                      break;
                    default: throw new InvalidDataException("Unknown projection type: " + reader.GetAttribute("type"));
                  }
                  break;
                }
                case "shapes":
                  if(!reader.IsEmptyElement)
                  {
                    reader.Read(); // move to the first shape, or the end element
                    while(reader.NodeType == XmlNodeType.Element) RootShapes.Add(Shape.Load(reader, observers, unitsById));
                  }
                  break;
                case "view":
                  Center     = ParseXmlPoint(reader.GetStringAttribute("cameraPosition", "0,0"));
                  ZoomFactor = reader.GetDoubleAttribute("zoom", 1);
                  reader.SkipChildren();
                  break;
                case "bgImage":
                  BackgroundImageCenter = ParseXmlPoint(reader.GetStringAttribute("centerPosition", "0,0"));
                  BackgroundImageScale  = reader.GetDoubleAttribute("zoom", 1);
                  if(!reader.IsEmptyElement)
                  {
                    reader.Read(); // move to the image data or the the end element
                    if(reader.LocalName == "imageData")
                    {
                      using(var stream = new GZipStream(new MemoryStream(Convert.FromBase64String(reader.ReadElementContentAsString())),
                                                        CompressionMode.Decompress))
                      {
                        BackgroundImage = Image.FromStream(stream);
                      }
                    }
                  }
                  break;
              }
            }

            foreach(KeyValuePair<Observation, string> pair in observers)
            {
              UnitShape observer;
              if(pair.Value == null || !unitsById.TryGetValue(pair.Value, out observer))
              {
                throw new InvalidDataException("Unit with ID " + (pair.Value == null ? "NULL" : pair.Value) + " does not exist.");
              }
              pair.Key.Observer = observer;
            }

            foreach(UnitShape unit in EnumerateShapes().OfType<UnitShape>()) unit.SortChildren();
            WasChanged = false;
          }
          catch
          {
            Clear();
            throw;
          }
        }
      }
    }

    public void Save(string fileName)
    {
      XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, IndentChars = "\t", NewLineOnAttributes = false };
      using(XmlWriter writer = XmlWriter.Create(fileName, settings))
      {
        writer.WriteStartElement("m", "moboard", "http://www.adammil.net/Maneubo/moboard");
        writer.WriteAttributeString("version", "1");

        switch(UnitSystem)
        {
          case UnitSystem.NauticalMetric: writer.WriteAttributeString("unitSystem", "nauticalMetric"); break;
          case UnitSystem.NauticalImperial: writer.WriteAttributeString("unitSystem", "nauticalImperial"); break;
          case UnitSystem.Metric: writer.WriteAttributeString("unitSystem", "terrestrialMetric"); break;
          case UnitSystem.Imperial: writer.WriteAttributeString("unitSystem", "terrestrialImperial"); break;
        }

        writer.WriteAttribute("showAllObservations", ShowAllObservations);
        writer.WriteAttributeString("backgroundColor", GetColorString(BackColor));
        writer.WriteAttributeString("observationColor", GetColorString(ObservationColor));
        writer.WriteAttributeString("referenceColor", GetColorString(ReferenceColor));
        writer.WriteAttributeString("scaleColor1", GetColorString(ScaleColor1));
        writer.WriteAttributeString("scaleColor2", GetColorString(ScaleColor2));
        writer.WriteAttributeString("selectedColor", GetColorString(SelectedColor));
        writer.WriteAttributeString("tmaColor", GetColorString(TMAColor));
        writer.WriteAttributeString("unselectedColor", GetColorString(UnselectedColor));

        if(Projection != null)
        {
          writer.WriteStartElement("projection");
          string type = null;
          if(Projection is AzimuthalEquidistantProjection) type = "azimuthalEquidistant";
          writer.WriteAttributeString("type", type);
          writer.WriteAttributeString("mapPoint", FormatXmlVector(ProjectionCenter));
          writer.WriteAttributeString("lonLat", FormatXmlVector(new BoardPoint(Projection.CenterLongitude, Projection.CenterLatitude)));
        }

        if(RootShapes.Count != 0)
        {
          writer.WriteStartElement("shapes");
          foreach(Shape shape in RootShapes) shape.Save(writer);
          writer.WriteEndElement();
        }

        writer.WriteStartElement("view");
        writer.WriteAttributeString("cameraPosition", FormatXmlVector(Center));
        writer.WriteAttribute("zoom", ZoomFactor);
        writer.WriteEndElement();

        if(BackgroundImage != null)
        {
          writer.WriteStartElement("bgImage");
          writer.WriteAttributeString("centerPosition", FormatXmlVector(BackgroundImageCenter));
          writer.WriteAttribute("zoom", BackgroundImageScale);

          using(MemoryStream bgStream = new MemoryStream(128*1024))
          {
            try
            {
              using(GZipStream gzip = new GZipStream(bgStream, CompressionMode.Compress, true))
              {
                BackgroundImage.Save(gzip, BackgroundImage.RawFormat);
              }
            }
            catch // with some image formats, an ArgumentNullException or a "generic GDI+ error" occurs within .Save(Stream), so try saving
            {     // into a temp file in that case, which seems more reliable
              bgStream.SetLength(0);
              string tempFile = Path.GetTempFileName();
              try
              {
                BackgroundImage.Save(tempFile);
                using(Stream imageStream = File.OpenRead(tempFile))
                using(GZipStream gzip = new GZipStream(bgStream, CompressionMode.Compress, true))
                {
                  imageStream.CopyTo(gzip);
                }
              }
              finally
              {
                File.Delete(tempFile);
              }
            }
            writer.WriteElementString("imageData", Convert.ToBase64String(bgStream.ToArray(), Base64FormattingOptions.InsertLineBreaks));
          }
          writer.WriteEndElement(); // bgImage
        }

        writer.WriteEndElement(); // moboard
      }

      WasChanged = false;
    }

    public readonly AddCircleToolClass AddCircleTool;
    public readonly AddLineToolClass AddLineTool;
    public readonly AddObservationToolClass AddObservationTool;
    public readonly AddUnitToolClass AddUnitTool;
    public readonly PointerToolClass PointerTool;
    public readonly SetupBackgroundToolClass SetupBackgroundTool;
    public readonly SetupMapProjectionClass SetupProjectionTool;
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
    }

    protected override void OnBackgroundImageChanged(EventArgs e)
    {
      base.OnBackgroundImageChanged(e);
      WasChanged = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      if(!e.Handled)
      {
        SelectedTool.KeyPress(e, true);
        if(!e.Handled)
        {
          if(e.KeyCode == Keys.F2 && e.Modifiers == Keys.None && SelectedShape != null)
          {
            EditShape(SelectedShape);
          }
          else if(e.KeyCode == Keys.Delete && (e.Modifiers == Keys.None || e.Modifiers == Keys.Shift) && SelectedShape != null)
          {
            DeleteShape(SelectedShape, e.Modifiers == Keys.Shift);
          }
          else if(e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
          {
            SelectedTool = PointerTool;
          }
          else
          {
            return;
          }

          e.Handled = true;
        }
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
            SetShortcut(menu.Items.Add("&Edit shape data", null, (o, ea) => EditShape(shape)), Keys.F2);
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
            SetShortcut(menu.Items.Add("&Delete", null, (o, ea) => DeleteShape(shape, false)), Keys.Delete);
            if(shape.Children.Any(c => !(c is PositionalDataShape))) // if the shape has children that aren't observations...
            {
              SetShortcut(menu.Items.Add("&Delete recursively", null, (o, ea) => DeleteShape(shape, true)), Keys.Delete | Keys.Shift);
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
          Shape shape = GetShapeUnderCursor(e.Location, out dragHandle);
          if(shape != null)
          {
            SelectedShape = shape;
            dragMode = DragMode.MoveItem;
            if(dragHandle != null) dragStart = dragHandle.GetStartPoint(shape, GetBoardPoint(e.Location));
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
        BoardPoint dragPoint = GetBoardPoint(e.Location);
        if(dragHandle != null) dragHandle.Update(SelectedShape, dragStart, dragPoint);
        else SelectedShape.Position = dragPoint;
        OnShapeChanged(SelectedShape);
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
      dragHandle = null;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      string statusText = null;
      if(dragHandle != null)
      {
        statusText = dragHandle.GetStatusText(SelectedShape);
      }
      else
      {
        BoardPoint cursor = GetBoardPoint(PointToClient(Cursor.Position));

        if(Projection != null)
        {
          BoardPoint lonLat = GetGeographicalPoint(cursor);
          statusText = GetLatitudeString(lonLat.Y) + ", " + GetLongitudeString(lonLat.X);
        }

        if(ReferenceShape != null)
        {
          double angle = AngleBetween(ReferenceShape.Position, cursor) * MathConst.RadiansToDegrees;
          statusText = (statusText == null ? null : statusText + "; ") +
                       angle.ToString("f2") + "°, " + GetDistanceString((cursor-ReferenceShape.Position).Length);
        }
      }

      StatusText = statusText;

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

      System.Drawing.Drawing2D.Matrix originalTransform = e.Graphics.Transform;
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
        for(int i=0; i<5; x += xInc, i++) e.Graphics.FillRectangle((i & 1) == 0 ? scaleBrush1 : scaleBrush2, x, y, xInc, 4);
        // center the scale text horizontally under the right end of the scale bar
        string scaleText = GetDistanceString(scaleDistance, unit);
        e.Graphics.DrawString(scaleText, Font, scaleBrush1, x-e.Graphics.MeasureString(scaleText, Font).Width/2, y+8,
                              StringFormat.GenericTypographic);
      }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      Invalidate();
    }

    internal const int VectorTime = 360; // the displayed length of motion vectors, in seconds

    internal string GetDistanceString(double meters)
    {
      return GetDistanceString(meters, UnitSystem);
    }

    internal static string GetLatitudeString(double value)
    {
      return GetLonLatString(value, "N", "S");
    }

    internal static string GetLongitudeString(double value)
    {
      return GetLonLatString(value, "W", "E");
    }

    internal string GetSpeedString(double metersPerSecond)
    {
      return GetSpeedString(metersPerSecond, UnitSystem);
    }

    internal static string FormatXmlVector(BoardPoint point)
    {
      return point.X.ToString("R") + ", " + point.Y.ToString("R");
    }

    internal static string FormatXmlVector(Vector2 vector)
    {
      return vector.X.ToString("R") + ", " + vector.Y.ToString("R");
    }

    internal static BoardPoint ParseXmlPoint(string pointStr)
    {
      return ParseXmlVector(pointStr).ToPoint();
    }

    internal static Vector2 ParseXmlVector(string vectorStr)
    {
      int comma = vectorStr.IndexOf(',');
      double x, y;
      if(comma == -1 || !double.TryParse(vectorStr.Substring(0, comma), out x) || !double.TryParse(vectorStr.Substring(comma+1), out y))
      {
        throw new InvalidDataException("\"" + vectorStr + "\" does not represent a valid point or vector.");
      }

      return new Vector2(x, y);
    }

    /// <summary>Converts from a Cartesian bearing to a nautical angle and vice versa.</summary>
    internal static double SwapBearing(double radians)
    {
      // a Cartesian bearing is such that a right-pointing vector is at 0 degrees, and upward-pointing vector is at 90 degrees, etc. but we
      // want up to be 0, right to be 90, etc. to change the direction of rotation, we can subtract from 2pi. 
      // then, to offset it so that up is zero instead of right, we can add 90 degrees. so effectively, we can subtract from 2pi + pi/2.
      // we can also perform the same operation to convert back
      double angle = Math.PI*2.5 - radians;
      if(angle >= Math.PI*2) angle -= Math.PI*2; // then we need to normalize the angle, since it can be >= 360 degrees after that
      return angle;
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

    void DeselectInvalidTool()
    {
      if(!IsValidTool(SelectedTool)) SelectedTool = PointerTool;
    }

    bool EditPositionalData(PositionalDataShape posData)
    {
      PositionalDataForm form = new PositionalDataForm(posData, UnitSystem);
      if(form.ShowDialog() == DialogResult.OK)
      {
        if(posData is BearingObservation) ((BearingObservation)posData).Bearing = form.Bearing;
        else posData.Position = form.Position;
        posData.Time = form.Time;

        ((UnitShape)posData.Parent).SortChildren();

        OnShapeChanged(posData);
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
            unit.Type             = form.UnitType;
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

          OnShapeChanged(shape);
        }
      }
    }

    bool IsValidTool(Tool tool)
    {
      if(ReferenceShape == null && tool == AddObservationTool || BackgroundImage == null && tool == SetupBackgroundTool) return false;
      return true;
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

    void OnReferenceShapeChanged(UnitShape previousReference)
    {
      DeselectInvalidTool();
      if(ReferenceShapeChanged != null) ReferenceShapeChanged(this, EventArgs.Empty);
    }

    void OnSelectionChanged(Shape previousSelection)
    {
      DeselectInvalidTool();
      if(SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
      SelectedTool.SelectionChanged(previousSelection);
    }

    void OnShapeAdded(Shape shape)
    {
      shape.SetBoard(this);
      shape.ID = shapeId++;
      SelectedTool.ShapeChanged(shape);
      Invalidate();
      WasChanged = true;
    }

    void OnShapeChanged(Shape shape)
    {
      SelectedTool.ShapeChanged(shape);
      Invalidate();
      WasChanged = true;
    }

    void OnShapeRemoved(Shape shape)
    {
      shape.SetBoard(null);
      SelectedTool.ShapeChanged(shape);
      if(RootShapes.Count == 0) shapeId = 0;
      Invalidate();
      WasChanged = true;
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

    internal SolidBrush normalBrush, selectedBrush, referenceBrush, observationBrush, scaleBrush1, scaleBrush2;
    internal Pen normalPen, selectedPen, referencePen, tmaPen, observationPen, selectedObservationPen;
    Color _observationColor, _referenceColor, _scaleColor1, _scaleColor2, _selectedColor, _tmaColor, _unselectedColor;
    BoardPoint _bgCenter, _center, _projectionCenter, dragStart, dragPoint;
    double _zoom = 1, _bgScale = 1;
    Tool _currentTool;
    MapProjection _projection;
    UnitShape _referenceShape;
    Shape _selectedShape;
    string _statusText;
    Handle dragHandle;
    uint shapeId;
    UnitSystem _unitSystem;
    DragMode dragMode;
    bool _showAllObservations;

    static string GetColorString(Color color)
    {
      return "#" + color.R.ToString("x2") + color.G.ToString("x2") + color.B.ToString("x2");
    }

    static string GetLonLatString(double value, string negativeSuffix, string positiveSuffix)
    {
      while(value > Math.PI) value -= Math.PI*2;
      while(value <= -Math.PI) value += Math.PI*2;
      value *= MathConst.RadiansToDegrees;

      string suffix = value < 0 ? negativeSuffix : value > 0 ? positiveSuffix : null;
      if(value < 0) value = -value;

      string str = ((int)value).ToInvariantString() + "°";

      double minutes = (value - (int)value) * 60;
      if(minutes != 0)
      {
        str = str + " " + ((int)minutes).ToInvariantString() + "′";
        double seconds = (minutes - (int)minutes) * 60;
        if(seconds != 0) str = str + " " + seconds.ToString("0.##") + "″";
      }
      if(suffix != null) str = str + " " + suffix;
      return str;
    }

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

    static Color ParseColor(string xmlColor, Color defaultValue)
    {
      if(xmlColor == null || xmlColor.Trim().Length == 0) return defaultValue;
      try
      {
        byte[] color = BinaryUtility.FromHex(xmlColor.Substring(1));
        if(color.Length == 3) return Color.FromArgb(color[0], color[1], color[2]);
      }
      catch { }

      throw new InvalidDataException("Invalid color value: " + xmlColor);
    }

    static UnitSystem ParseUnitSystem(string xmlValue, UnitSystem defaultValue)
    {
      if(xmlValue == null || xmlValue.Trim().Length == 0) return defaultValue;
      switch(xmlValue.Trim().ToLowerInvariant())
      {
        case "nauticalmetric": return UnitSystem.NauticalMetric;
        case "nauticalimperial": return UnitSystem.NauticalImperial;
        case "terrestrialmetric": return UnitSystem.Metric;
        case "terrestrialimperial": return UnitSystem.Imperial;
        default: throw new InvalidDataException("Invalid unit system: " + xmlValue);
      }
    }

    static void SetShortcut(ToolStripItem item, Keys shortcutKeys)
    {
      ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
      menuItem.ShortcutKeys = shortcutKeys;
    }

    // these correspond to the LengthUnit enum (Meter, Kilometer, Foot, Yard, Kiloyard, Mile, NauticalMile)
    static readonly double[] conversionsToMeters = new double[] { 1, 1000, 0.3048, 0.9144, 914.4, 1609.344, 1852 };
    static readonly string[] lengthAbbreviations = new string[] { "m", "km", "ft", "yd", "kyd", "mi", "nmi" };

    // these correspond to the SpeedUnit enum (MetersPerSecond, KilometersPerHour, MilesPerHour, Knots)
    static readonly double[] conversionsToMPS = new double[] { 1, 1000.0/3600, 1609.344/3600, 1852.0/3600 };
    static readonly string[] speedAbbreviations = new string[] { "m/s", "kph", "mph", "kn" };

    static readonly Color DefaultBoardBackColor = Color.FromArgb(0xad, 0xd8, 0xe6);
    static readonly Color DefaultObservationColor = Color.Gray;
    static readonly Color DefaultReferenceColor = Color.Red;
    static readonly Color DefaultScaleColor1 = Color.Black;
    static readonly Color DefaultScaleColor2 = Color.White;
    static readonly Color DefaultSelectedColor = Color.Blue;
    static readonly Color DefaultTMAColor = Color.DarkGreen;
    static readonly Color DefaultUnselectedColor = Color.Black;
  }
  #endregion
}