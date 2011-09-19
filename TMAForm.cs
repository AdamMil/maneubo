using System;
using System.ComponentModel;
using System.Windows.Forms;
using AdamMil.Mathematics.Geometry;

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

    public event EventHandler ApplySolution, AutoSolve, CourseChanged, Optimize, SpeedChanged;

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

    public double? MinCourse
    {
      get { return ParseCourse(txtMinCourse); }
    }

    public double? MaxCourse
    {
      get { return ParseCourse(txtMaxCourse); }
    }

    public double? MinSpeed
    {
      get { return ParseSpeed(txtMinSpeed); }
    }

    public double? MaxSpeed
    {
      get { return ParseSpeed(txtMaxSpeed); }
    }

    public void FocusCourse()
    {
      txtCourse.Focus();
    }

    public void FocusSpeed()
    {
      txtSpeed.Focus();
    }

    public void ToggleCourseLock()
    {
      LockCourse = !LockCourse;
    }

    public void ToggleSpeedLock()
    {
      LockSpeed = !LockSpeed;
    }

    double? ParseSpeed(TextBox textBox)
    {
      double speed;
      return TryParseSpeed(textBox.Text, unitSystem, out speed) ? (double?)speed : null;
    }

    bool ValidateSpeed(TextBox textBox)
    {
      double speed;
      if(!string.IsNullOrEmpty(textBox.Text.Trim()) && !TryParseSpeed(textBox.Text, unitSystem, out speed))
      {
        ShowInvalidSpeed(textBox.Text);
        textBox.Focus();
        return false;
      }

      return true;
    }

    void btnApply_Click(object sender, EventArgs e)
    {
      if(ApplySolution != null) ApplySolution(this, EventArgs.Empty);
    }

    void btnAuto_Click(object sender, EventArgs e)
    {
      if(!ValidateCourse(txtMinCourse) || !ValidateCourse(txtMaxCourse) || !ValidateSpeed(txtMinSpeed) || !ValidateSpeed(txtMaxSpeed))
      {
        return;
      }
      if(AutoSolve != null) AutoSolve(this, EventArgs.Empty);
    }

    void btnOptimize_Click(object sender, EventArgs e)
    {
      if(Optimize != null) Optimize(this, EventArgs.Empty);
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
          ShowInvalidAngle(txtCourse.Text);
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

    static double? ParseCourse(TextBox textBox)
    {
      double angle;
      return TryParseAngle(textBox.Text, out angle) ? (double?)angle : null;
    }

    static bool ValidateCourse(TextBox textBox)
    {
      double angle;
      if(!string.IsNullOrEmpty(textBox.Text.Trim()) && !TryParseAngle(textBox.Text, out angle))
      {
        ShowInvalidAngle(textBox.Text);
        textBox.Focus();
        return false;
      }

      return true;
    }
  }
}
