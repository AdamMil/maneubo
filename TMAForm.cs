using System;
using AdamMil.Mathematics.Geometry;
using System.ComponentModel;

namespace Maneubo
{
  partial class TMAForm : DataForm
  {
    public TMAForm()
    {
      InitializeComponent();
    }

    public TMAForm(UnitSystem unitSystem) : this()
    {
      this.unitSystem = unitSystem;
    }

    public event EventHandler ApplySolution, AutoSolve, CourseChanged, SpeedChanged;

    public double Course
    {
      get { return txtCourse.Tag == null ? 0 : (double)txtCourse.Tag; }
      set
      {
        txtCourse.Tag  = value;
        txtCourse.Text = (value * MathConst.RadiansToDegrees).ToString("0.##");
        txtCourse.WasChanged = false;
      }
    }

    public double Speed
    {
      get { return txtSpeed.Tag == null ? 0 : (double)txtSpeed.Tag; }
      set
      {
        txtSpeed.Tag  = value;
        txtSpeed.Text = ManeuveringBoard.GetSpeedString(value, unitSystem);
        txtSpeed.WasChanged = false;
      }
    }

    public bool LockCourse
    {
      get { return chkLockCourse.Checked; }
      set { chkLockCourse.Checked = value; }
    }

    public bool LockSpeed
    {
      get { return chkLockSpeed.Checked; }
      set { chkLockSpeed.Checked = value; }
    }

    void btnApply_Click(object sender, EventArgs e)
    {
      if(ApplySolution != null) ApplySolution(this, EventArgs.Empty);
    }

    void btnAuto_Click(object sender, EventArgs e)
    {
      if(AutoSolve != null) AutoSolve(this, EventArgs.Empty);
    }

    void txtCourse_Validating(object sender, CancelEventArgs e)
    {
      if(txtCourse.WasChanged)
      {
        double value;
        if(double.TryParse(txtCourse.Text.Trim(), out value))
        {
          value *= MathConst.DegreesToRadians;
          while(value < 0) value += Math.PI*2;
          while(value >= Math.PI*2) value -= Math.PI*2;
          txtCourse.Tag = value;
          if(CourseChanged != null) CourseChanged(this, EventArgs.Empty);
        }
        else if(string.IsNullOrEmpty(txtCourse.Text.Trim()))
        {
          Course = Course; // invoke the property setter to reset the text
        }
        else
        {
          ShowInvalidDirection(txtCourse.Text);
          e.Cancel = true;
          return;
        }

        txtCourse.WasChanged = false;
      }
    }

    void txtSpeed_Validating(object sender, CancelEventArgs e)
    {
      if(txtSpeed.WasChanged)
      {
        double value;
        if(TryParseSpeed(txtSpeed.Text, unitSystem, out value))
        {
          txtSpeed.Tag = value;
          if(SpeedChanged != null) SpeedChanged(this, EventArgs.Empty);
        }
        else if(string.IsNullOrEmpty(txtSpeed.Text.Trim()))
        {
          Speed = Speed; // invoke the property setter to reset the text
        }
        else
        {
          ShowInvalidSpeed(txtSpeed.Text);
          e.Cancel = true;
          return;
        }
        txtCourse.WasChanged = false;
      }
    }

    readonly UnitSystem unitSystem;
  }
}
