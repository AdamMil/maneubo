using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Maneubo
{

class ChangeTrackingTextBox : TextBox
{
  [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public bool WasChanged { get; set; }

  protected override void OnTextChanged(EventArgs e)
  {
    WasChanged = true;
    base.OnTextChanged(e);
  }
}

}