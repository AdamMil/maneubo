using System.Windows.Forms;
using System;

namespace Maneubo
{
  public partial class ProgramOptionsForm : Form
  {
    public ProgramOptionsForm()
    {
      InitializeComponent();
    }

    public int SaveTimeHotkey
    {
      get { return GetHotkey(chkSaveTime, chkTSAlt, chkTSCtrl, chkTSShift, txtTSChar); }
      set { SetHotkey(value, chkSaveTime, chkTSAlt, chkTSCtrl, chkTSShift, txtTSChar); }
    }

    public int ToggleStopwatchHotkey
    {
      get { return GetHotkey(chkToggleStopwatch, chkSTAlt, chkSTCtrl, chkSTShift, txtSTChar); }
      set { SetHotkey(value, chkToggleStopwatch, chkSTAlt, chkSTCtrl, chkSTShift, txtSTChar); }
    }

    const int HkAlt=1, HkCtrl=2, HkShift=4, HkNoRepeat=0x4000; // global hotkey flags

    void btnOK_Click(object sender, EventArgs e)
    {
      if((!chkSaveTime.Checked || ValidateHotkey(chkSaveTime.Text, txtTSChar)) &&
         (!chkToggleStopwatch.Checked || ValidateHotkey(chkToggleStopwatch.Text, txtSTChar)))
      {
        DialogResult = DialogResult.OK;
      }
    }

    static int GetHotkey(CheckBox chkEnable, CheckBox alt, CheckBox ctrl, CheckBox shift, TextBox txtChar)
    {
      if(!chkEnable.Checked) return 0;
      string text = txtChar.Text.Trim();
      if(text.Length != 1) throw new InvalidOperationException();
      return (((alt.Checked ? HkAlt : 0) | (ctrl.Checked ? HkCtrl : 0) | (shift.Checked ? HkShift : 0)) << 16) |
             char.ToUpperInvariant(text[0]);
    }

    static void SetHotkey(int value, CheckBox chkEnable, CheckBox alt, CheckBox ctrl, CheckBox shift, TextBox txtChar)
    {
      if(value == 0)
      {
        chkEnable.Checked = false;
        alt.Checked = ctrl.Checked = shift.Checked = false;
        txtChar.Clear();
      }
      else
      {
        int flags = value >> 16;
        chkEnable.Checked = true;
        alt.Checked   = (flags & HkAlt)   != 0;
        ctrl.Checked  = (flags & HkCtrl)  != 0;
        shift.Checked = (flags & HkShift) != 0;
        txtChar.Text  = new string((char)value, 1);
      }
    }

    static bool ValidateHotkey(string hotkeyName, TextBox txtChar)
    {
      string text = txtChar.Text.Trim();
      char hotkey = text.Length == 1 ? char.ToUpperInvariant(text[0]) : '\0';
      if(hotkey < 'A' || hotkey > 'Z')
      {
        MessageBox.Show(hotkeyName + ": You must specify a single English letter as the hotkey or uncheck the box at the left to " +
                        "disable it.", "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Error);
        txtChar.Focus();
        return false;
      }

      return true;
    }
  }
}
