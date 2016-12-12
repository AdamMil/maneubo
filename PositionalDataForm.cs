using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AdamMil.Mathematics.Geometry;
using System.ComponentModel;
using System.Collections.Generic;

namespace Maneubo
{
  partial class PositionalDataForm : DataForm
  {
    public PositionalDataForm()
    {
      InitializeComponent();
    }

    public PositionalDataForm(PositionalDataShape posData, UnitSystem unitSystem) : this()
    {
      this.unitSystem = unitSystem;

      txtTime.Text = ManeuveringBoard.GetTimeString(posData.Time);
      txtTime.Focus();
      txtTime.SelectAll();

      Text = grpObservation.Text = (posData is Waypoint ? "Waypoint" : "Observation") + " Data";

      PositionalDataShape previousPosition = null;
      UnitShape observer = posData is Observation ? ((Observation)posData).Observer : null;
      for(int index = posData.Parent.Children.IndexOf(posData)-1; index >= 0; index--)
      {
        previousPosition = posData.Parent.Children[index] as PositionalDataShape;
        if(previousPosition != null && previousPosition.GetType() == posData.GetType() &&
           (!(previousPosition is Observation) || ((Observation)previousPosition).Observer == observer))
        {
          break;
        }
        else
        {
          previousPosition = null;
        }
      }
      if(previousPosition != null) previousTime = previousPosition.Time;

      if(posData is BearingObservation)
      {
        double bearing = ((BearingObservation)posData).Bearing;
        grpPrevious.Enabled = false;
        grpObserved.Enabled = false;
        txtObserverDistance.Enabled = false;
        txtObserverBearing.Text = (bearing * MathConst.RadiansToDegrees).ToString("0.##");
        txtObserverBearing.Tag  = bearing;
        txtObservedBearing.WasChanged = false; // ignore programmatic change
      }
      else
      {
        this.posDataPoint = posData.Position;
        FillRelativeTo((UnitShape)posData.Parent, txtObservedBearing, txtObservedDistance, out observedPoint);

        if(posData is PointObservation)
        {
          FillRelativeTo(observer, txtObserverBearing, txtObserverDistance, out observerPoint);
        }
        else
        {
          grpObserver.Enabled = false;
          grpObserved.Text = "Relative to Unit";
        }

        if(previousPosition == null) grpPrevious.Enabled = false;
        else FillRelativeTo(previousPosition, txtPreviousBearing, txtPreviousDistance, out previousPoint);

        waypoint = posData is Waypoint;
        if(waypoint) waypointTimes = posData.Parent.Children.OfType<Waypoint>().Where(w => w != posData).Select(w => w.Time).ToList();
      }
    }

    public double Bearing
    {
      get { return (double)txtObserverBearing.Tag; }
    }

    public Point2 Position
    {
      get { return posDataPoint.Value; }
    }

    public TimeSpan Time
    {
      get
      {
        TimeSpan time;
        TryParseTime(txtTime.Text, out time);
        return time;
      }
    }

    void FillRelativeTo(ChangeTrackingTextBox txtBearing, ChangeTrackingTextBox txtDistance, Point2 relativePoint)
    {
      double angle = ManeuveringBoard.AngleBetween(relativePoint, posDataPoint.Value);
      txtBearing.Text = (angle * MathConst.RadiansToDegrees).ToString("0.##");
      txtBearing.Tag  = angle;
      double distance = relativePoint.DistanceTo(posDataPoint.Value);
      txtDistance.Text = ManeuveringBoard.GetDistanceString(distance, unitSystem);
      txtDistance.Tag  = distance;
      txtBearing.WasChanged = txtDistance.WasChanged = false;
    }

    void FillRelativeTo(Shape relativeTo, ChangeTrackingTextBox txtBearing, ChangeTrackingTextBox txtDistance, out Point2 relativePoint)
    {
      FillRelativeTo(txtBearing, txtDistance, relativeTo.Position);
      relativePoint = relativeTo.Position;
    }

    void FillRelativeTo(Shape relativeTo, ChangeTrackingTextBox txtBearing, ChangeTrackingTextBox txtDistance, out Point2? relativePoint)
    {
      FillRelativeTo(txtBearing, txtDistance, relativeTo.Position);
      relativePoint = relativeTo.Position;
    }

    bool TryParseTime(string text, out TimeSpan timeSpan)
    {
      bool relative;
      if(!TryParseTime(text, out timeSpan, out relative) || relative && !previousTime.HasValue) return false;
      if(relative) timeSpan += previousTime.Value;
      return true;
    }

    void UpdateObservationPoint(ChangeTrackingTextBox txtBearing, ChangeTrackingTextBox txtDistance, Point2 fromPoint,
                                ChangeTrackingTextBox txtOtherBearing1, ChangeTrackingTextBox txtOtherDistance1, Point2? otherPoint1,
                                ChangeTrackingTextBox txtOtherBearing2, ChangeTrackingTextBox txtOtherDistance2, Point2? otherPoint2)
    {
      if(Validate(txtBearing, txtDistance))
      {
        double bearing = (double)txtBearing.Tag, distance = (double)txtDistance.Tag;
        posDataPoint = fromPoint + new Vector2(0, distance).Rotate(-bearing);

        if(otherPoint1.HasValue) FillRelativeTo(txtOtherBearing1, txtOtherDistance1, otherPoint1.Value);
        if(otherPoint2.HasValue) FillRelativeTo(txtOtherBearing2, txtOtherDistance2, otherPoint2.Value);
        txtBearing.WasChanged = txtDistance.WasChanged = false;
      }
    }

    bool Validate(ChangeTrackingTextBox txtBearing, ChangeTrackingTextBox txtDistance)
    {
      double value;
      if(txtBearing.WasChanged)
      {
        if(TryParseAngle(txtBearing.Text, out value))
        {
          txtBearing.Tag = value;
        }
        else
        {
          if(string.IsNullOrEmpty(txtBearing.Text.Trim())) ShowRequiredMessage("Bearing");
          else ShowInvalidAngle(txtBearing.Text);
          txtBearing.Focus();
          return false;
        }
      }

      if(txtDistance.WasChanged)
      {
        if(TryParseLength(txtDistance.Text, unitSystem, out value))
        {
          txtDistance.Tag = value;
        }
        else
        {
          if(string.IsNullOrEmpty(txtDistance.Text.Trim())) ShowRequiredMessage("Distance");
          else ShowInvalidLength(txtDistance.Text);
          txtDistance.Focus();
          return false;
        }
      }

      return true;
    }

    void btnOK_Click(object sender, EventArgs e)
    {
      // if the user presses Enter, the Leave event of the textbox won't be raised, so we need to ensure that the same logic gets executed.
      // first we'll ensure that the logic would succeed if executed. if not, then we abort
      if(!Validate(txtObservedBearing, txtObservedDistance) || !Validate(txtObserverBearing, txtObserverDistance) ||
         !Validate(txtPreviousBearing, txtPreviousDistance))
      {
        return;
      }

      // now that we know the Leave event should succeed, force it to be raised
      btnOK.Focus();

      TimeSpan time;
      if(!TryParseTime(txtTime.Text, out time))
      {
        if(string.IsNullOrEmpty(txtTime.Text.Trim())) ShowRequiredMessage("Time");
        else ShowInvalidTime(txtTime.Text, true, true);
        return;
      }
      else if(waypointTimes != null && waypointTimes.Contains(time))
      {
        MessageBox.Show("Two waypoints cannot have the same time.", "Invalid time", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      DialogResult = DialogResult.OK;
    }

    void txtPrevious_Leave(object sender, EventArgs e)
    {
      if(txtPreviousBearing.WasChanged || txtPreviousDistance.WasChanged)
      {
        UpdateObservationPoint(txtPreviousBearing, txtPreviousDistance, previousPoint.Value,
                               txtObservedBearing, txtObservedDistance, observedPoint,
                               txtObserverBearing, txtObserverDistance, observerPoint);
      }
    }

    void txtObserver_Leave(object sender, EventArgs e)
    {
      if(txtObserverBearing.WasChanged || txtObserverDistance.WasChanged)
      {
        if(posDataPoint.HasValue)
        {
          UpdateObservationPoint(txtObserverBearing, txtObserverDistance, observerPoint.Value,
                                 txtObservedBearing, txtObservedDistance, observedPoint,
                                 txtPreviousBearing, txtPreviousDistance, previousPoint);
        }
        else
        {
          Validate(txtObserverBearing, txtObserverDistance);
        }
      }
    }

    void txtObserved_Leave(object sender, EventArgs e)
    {
      if(txtObservedBearing.WasChanged || txtObservedDistance.WasChanged)
      {
        UpdateObservationPoint(txtObservedBearing, txtObservedDistance, observedPoint,
                               txtObserverBearing, txtObserverDistance, observerPoint,
                               txtPreviousBearing, txtPreviousDistance, previousPoint);
      }
    }

    readonly Point2 observedPoint;
    readonly Point2? observerPoint, previousPoint;
    readonly TimeSpan? previousTime;
    readonly UnitSystem unitSystem;
    readonly List<TimeSpan> waypointTimes;
    readonly bool waypoint;

    Point2? posDataPoint;
  }
}
