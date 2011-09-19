using System.Windows.Forms;

namespace Maneubo
{
  partial class BackgroundScaleForm : DataForm
  {
    public BackgroundScaleForm()
    {
      InitializeComponent();
    }

    public BackgroundScaleForm(double pixels, double meters, UnitSystem unitSystem) : this()
    {
      lblPixels.Text = pixels.ToString("0.##") + " pixels represents...";
      txtLength.Text = ManeuveringBoard.GetDistanceString(meters, unitSystem);
      txtLength.Tag  = meters;
      txtLength.WasChanged = false;
      this.unitSystem = unitSystem;
    }

    public double Distance
    {
      get
      {
        double distance;
        if(txtLength.WasChanged) TryParseLength(txtLength.Text, unitSystem, out distance);
        else distance = (double)txtLength.Tag;
        return distance;
      }
    }

    void btnOK_Click(object sender, System.EventArgs e)
    {
      double length;
      if(txtLength.WasChanged)
      {
        if(TryParseLength(txtLength.Text, unitSystem, out length))
        {
          txtLength.Tag = length;
        }
        else
        {
          if(string.IsNullOrEmpty(txtLength.Text.Trim())) ShowRequiredMessage("Distance");
          else ShowInvalidLength(txtLength.Text);
          return;
        }
      }

      DialogResult = DialogResult.OK;
    }

    readonly UnitSystem unitSystem;
  }
}
