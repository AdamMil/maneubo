using System;
using System.Windows.Forms;

namespace Maneubo
{
  partial class AdvanceTimeForm : DataForm
  {
    public AdvanceTimeForm()
    {
      InitializeComponent();
    }

    public bool AdvanceUnitsWithWaypoints
    {
      get { return chkWaypoints.Checked; }
    }

    public TimeSpan Time { get; private set; }

    void btnOK_Click(object sender, EventArgs e)
    {
      string timeStr = txtTime.Text.Trim();
      bool negative = false, relative;
      if(timeStr.Length != 0 && timeStr[0] == '-')
      {
        timeStr  = timeStr.Substring(1);
        negative = true;
      }

      TimeSpan time;
      if(!TryParseTime(timeStr, out time, out relative) || (relative & negative))
      {
        ShowInvalidTime(txtTime.Text, false);
        return;
      }
      Time = negative ? -time : time;
      DialogResult = DialogResult.OK;
    }
  }
}
