using System;
using System.Windows.Forms;
using AdamMil.Mathematics.Geometry;
using AdamMil.Utilities;

namespace Maneubo
{
  sealed partial class ShapeDataForm : DataForm
  {
    public ShapeDataForm()
    {
      InitializeComponent();
    }

    public ShapeDataForm(Shape shape, UnitSystem unitSystem) : this()
    {
      if(shape == null) throw new ArgumentNullException();

      this.unitSystem = unitSystem;

      txtName.Text = shape.Name;
      lblParent.Text = shape.Parent == null ? "<none>" : string.IsNullOrEmpty(shape.Parent.Name) ? "<unnamed shape>" : shape.Parent.Name;

      UnitShape unit = shape as UnitShape;
      if(unit != null)
      {
        txtSize.Enabled   = false;

        txtDirection.Tag  = unit.Direction;
        txtDirection.Text = (unit.Direction * MathConst.RadiansToDegrees).ToString("0.##");
        txtSpeed.Tag      = unit.Speed;
        txtSpeed.Text     = ManeuveringBoard.GetSpeedString(unit.Speed, unitSystem);
        cmbType.SelectedIndex = (int)unit.Type;

        if(unit.Parent == null) chkRelative.Enabled = false;
        else chkRelative.Checked = unit.IsMotionRelative;
      }
      else
      {
        txtSpeed.Enabled    = false;
        chkRelative.Enabled = false;
        cmbType.Enabled     = false;

        LineShape line = shape as LineShape;
        if(line != null)
        {
          double angle = ManeuveringBoard.AngleBetween(line.Start, line.End), length = line.Length;
          txtDirection.Text = (angle * MathConst.RadiansToDegrees).ToString("0.##");
          txtDirection.Tag  = angle;
          txtSize.Text      = ManeuveringBoard.GetDistanceString(length, unitSystem);
          txtSize.Tag       = length;
        }
        else
        {
          CircleShape circle = shape as CircleShape;
          if(circle != null)
          {
            txtDirection.Enabled  = false;
            txtSize.Text = ManeuveringBoard.GetDistanceString(circle.Radius, unitSystem);
            txtSize.Tag  = circle.Radius;
            lblSize.Text = "Radius";
          }
          else
          {
            throw new NotImplementedException();
          }
        }
      }
      
      // set these to false, since they may have been set to true by the programmatic changes above
      directionTextChanged = sizeTextChanged = speedTextChanged = false;
    }

    public double Direction
    {
      get
      {
        double direction;
        if(!directionTextChanged)
        {
          direction = txtDirection.Tag == null ? 0 : (double)txtDirection.Tag;
        }
        else if(double.TryParse(txtDirection.Text.Trim(), out direction))
        {
          direction *= MathConst.DegreesToRadians;
          while(direction < 0) direction += Math.PI*2;
          while(direction >= Math.PI*2) direction -= Math.PI*2;
        }
        return direction;
      }
    }

    public bool IsMotionRelative
    {
      get { return chkRelative.Checked; }
    }

    public string ShapeName
    {
      get { return StringUtility.MakeNullIfEmpty(txtName.Text.Trim()); }
    }

    public double ShapeSize
    {
      get
      {
        double size;
        if(!sizeTextChanged) size = txtSize.Tag == null ? 0 : (double)txtSize.Tag;
        else TryParseLength(txtSize.Text, out size);
        return size;
      }
    }

    public double Speed
    {
      get
      {
        double speed;
        if(!speedTextChanged) speed = txtSpeed.Tag == null ? 0 : (double)txtSpeed.Tag;
        else TryParseSpeed(txtSpeed.Text, unitSystem, out speed);
        return speed;
      }
    }

    public UnitShapeType UnitType
    {
      get { return (UnitShapeType)cmbType.SelectedIndex; }
    }

    void btnOK_Click(object sender, EventArgs e)
    {
      double value;
      if(directionTextChanged && !double.TryParse(txtDirection.Text.Trim(), out value))
      {
        if(string.IsNullOrEmpty(txtDirection.Text.Trim())) ShowRequiredMessage("Direction");
        else ShowInvalidDirection(txtDirection.Text);
        txtDirection.Focus();
        return;
      }

      if(sizeTextChanged && !TryParseLength(txtSize.Text, out value))
      {
        if(string.IsNullOrEmpty(txtSize.Text.Trim())) ShowRequiredMessage("Size");
        else ShowInvalidLength(txtSize.Text);
        txtSize.Focus();
        return;
      }

      if(speedTextChanged && !TryParseSpeed(txtSpeed.Text, unitSystem, out value))
      {
        if(string.IsNullOrEmpty(txtSpeed.Text.Trim())) ShowRequiredMessage("Speed");
        else ShowInvalidSpeed(txtSpeed.Text);
        txtSpeed.Focus();
        return;
      }

      DialogResult = DialogResult.OK;
    }

    void txtDirection_TextChanged(object sender, EventArgs e)
    {
      directionTextChanged = true;
    }

    void txtSize_TextChanged(object sender, EventArgs e)
    {
      sizeTextChanged = true;
    }

    void txtSpeed_TextChanged(object sender, EventArgs e)
    {
      speedTextChanged = true;
    }

    readonly UnitSystem unitSystem;
    bool directionTextChanged, sizeTextChanged, speedTextChanged;
  }
}
