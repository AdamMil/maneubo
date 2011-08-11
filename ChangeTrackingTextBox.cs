using System;
using System.Windows.Forms;

namespace Maneubo
{

class ChangeTrackingTextBox : TextBox
{
  public bool WasChanged { get; set; }

  protected override void OnTextChanged(EventArgs e)
  {
    WasChanged = true;
    base.OnTextChanged(e);
  }
}

}