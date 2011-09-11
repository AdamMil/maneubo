using System.Windows.Forms;
using System.Drawing;

namespace Maneubo
{
  partial class BoardOptionsForm : Form
  {
    public BoardOptionsForm()
    {
      InitializeComponent();
    }

    public BoardOptionsForm(ManeuveringBoard board) : this()
    {
      chkShowAllObservations.Checked = board.ShowAllObservations;
      cmbUnitSystem.SelectedIndex = (int)board.UnitSystem;
      btnBackground.BackColor   = board.BackColor;
      btnObservations.BackColor = board.ObservationColor;
      btnReference.BackColor    = board.ReferenceColor;
      btnScale1.BackColor       = board.ScaleColor1;
      btnScale2.BackColor       = board.ScaleColor2;
      btnSelected.BackColor     = board.SelectedColor;
      btnTMA.BackColor          = board.TMAColor;
      btnUnselected.BackColor   = board.UnselectedColor;
    }

    public Color BoardBackgroundColor
    {
      get { return btnBackground.BackColor; }
    }

    public Color ObservationColor
    {
      get { return btnObservations.BackColor; }
    }

    public Color ReferenceColor
    {
      get { return btnReference.BackColor; }
    }

    public Color ScaleColor1
    {
      get { return btnScale1.BackColor; }
    }

    public Color ScaleColor2
    {
      get { return btnScale2.BackColor; }
    }

    public Color SelectedColor
    {
      get { return btnSelected.BackColor; }
    }

    public Color TMAColor
    {
      get { return btnTMA.BackColor; }
    }

    public Color UnselectedColor
    {
      get { return btnUnselected.BackColor; }
    }

    public bool ShowAllObservations
    {
      get { return chkShowAllObservations.Checked; }
    }

    public UnitSystem UnitSystem
    {
      get { return (UnitSystem)cmbUnitSystem.SelectedIndex; }
    }

    void btnColor_Click(object sender, System.EventArgs e)
    {
      Button button = (Button)sender;
      colorDialog.Color = button.BackColor;
      if(colorDialog.ShowDialog() == DialogResult.OK) button.BackColor = colorDialog.Color;
    }
  }
}
