using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace Maneubo
{
  public partial class StopwatchForm : Form
  {
    public StopwatchForm()
    {
      InitializeComponent();
    }

    public void SaveTime()
    {
      lstTimes.Items.Add(GetTimeLabel(currentClock));
      btnClear.Enabled = true;
    }

    public void Toggle()
    {
      timer.Enabled     = !timer.Enabled;
      btnStartStop.Text = timer.Enabled ? "&Stop" : "&Start";
      if(timer.Enabled)
      {
        currentClock.Start();
        if(!totalClock.IsRunning) totalClock.Start();
      }
      else
      {
        currentClock.Stop();
      }
    }

    void btnClear_Click(object sender, System.EventArgs e)
    {
      lstTimes.Items.Clear();
      btnClear.Enabled = btnCopy.Enabled = false;
    }

    void btnCopy_Click(object sender, System.EventArgs e)
    {
      if(lstTimes.SelectedIndex != -1)
      {
        for(int i=0; i<2; i++)
        {
          try
          {
            Clipboard.SetText((string)lstTimes.SelectedItem);
            break;
          }
          catch
          {
            System.Threading.Thread.Sleep(10);
          }
        }
      }
    }

    void btnSave_Click(object sender, System.EventArgs e)
    {
      SaveTime();
    }

    void btnStartStop_Click(object sender, System.EventArgs e)
    {
      Toggle();
    }

    void btnReset_Click(object sender, System.EventArgs e)
    {
      currentClock.Reset();
      totalClock.Reset();
      lblCurrent.Text = lblTotal.Text = "0:00:00";
      if(timer.Enabled)
      {
        currentClock.Start();
        totalClock.Start();
      }
    }

    void lstTimes_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      btnCopy_Click(null, null);
    }

    void lstTimes_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      btnCopy.Enabled = lstTimes.SelectedIndex != -1;
    }

    void timer_Tick(object sender, System.EventArgs e)
    {
      lblCurrent.Text = GetTimeLabel(currentClock);
      lblTotal.Text   = GetTimeLabel(totalClock);
    }

    readonly Stopwatch currentClock = new Stopwatch(), totalClock = new Stopwatch();

    static string GetTimeLabel(Stopwatch stopwatch)
    {
      uint seconds = (uint)((stopwatch.ElapsedMilliseconds + 500) / 1000), minutes = seconds/60;
      return (minutes/60).ToString(CultureInfo.InvariantCulture) + ":" + (minutes%60).ToString("d2", CultureInfo.InvariantCulture) + ":" +
             (seconds%60).ToString("d2", CultureInfo.InvariantCulture);
    }
  }
}
