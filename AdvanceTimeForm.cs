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

    public TimeSpan Time
    {
      get { return _time; }
      set
      {
        txtTime.Text = ManeuveringBoard.GetTimeString(value);
        _time = value;
      }
    }

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
        ShowInvalidTime(txtTime.Text, false, true);
        return;
      }
      Time = negative ? -time : time;
      DialogResult = DialogResult.OK;
    }

    TimeSpan _time;
  }
}
