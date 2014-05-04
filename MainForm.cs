using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AdamMil.Utilities;

namespace Maneubo
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
      board.WasChanged = false; // prevent the setting of default property values from affecting whether the board is considered changed
      NewBoard();
      board.SelectedTool = board.AddUnitTool;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      if(!e.Cancel && !TrySaveChanges()) e.Cancel = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      if(!e.Handled && e.Modifiers == Keys.None)
      {
        if(e.KeyCode == Keys.P) tbPointer.PerformClick();
        else if(e.KeyCode == Keys.U) tbAddUnit.PerformClick();
        else if(e.KeyCode == Keys.O) tbAddObservation.PerformClick();
        else if(e.KeyCode == Keys.T) tbTMA.PerformClick();
        else if(e.KeyCode == Keys.L) tbLine.PerformClick();
        else if(e.KeyCode == Keys.C) tbCircle.PerformClick();
        else if(e.KeyCode == Keys.I) tbIntercept.PerformClick();
        else if(e.KeyCode == Keys.F3) tbUnitShape.ShowDropDown();
        else if(e.KeyCode == Keys.F4) tbWaypointType.ShowDropDown();
        else return;
        e.Handled = true;
      }
    }

    protected override void WndProc(ref Message m)
    {
      base.WndProc(ref m);
      if(m.Msg == 0x312) // WM_HOTKEY
      {
        switch(m.WParam.ToInt32())
        {
          case SaveTimeKeyId:
            if(stopwatch != null && !stopwatch.IsDisposed) stopwatch.SaveTime();
            break;
          case ToggleStopwatchKeyId:
            if(stopwatch != null && !stopwatch.IsDisposed) stopwatch.Toggle();
            break;
        }
      }
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);

      // see if NoRepeat is supported. the actual key registered doesn't matter, since we'll immediately unregister it
      noRepeatSupported = RegisterHotKey(Handle, 0, HkAlt|HkCtrl|HkShift|HkNoRepeat, 'W');
      UnregisterHotKey(Handle, 0);

      string dataDirectory = Program.GetDataDirectory();
      if(dataDirectory != null)
      {
        string configFile = Path.Combine(dataDirectory, "config.txt");
        if(File.Exists(configFile))
        {
          using(StreamReader reader = new StreamReader(configFile))
          {
            while(true)
            {
              string line = reader.ReadLine();
              if(line == null) break;
              line = line.Trim();
              if(line.Length == 0) continue;

              int equals = line.IndexOf('=');
              if(equals == -1) continue;

              string key = line.Substring(0, equals).Trim(), value = line.Substring(equals+1).Trim();
              switch(key.ToLowerInvariant())
              {
                case "savetimehotkey": int.TryParse(value, out saveTimeKey); break;
                case "togglestopwatchhotkey": int.TryParse(value, out toggleStopwatchKey); break;
              }
            }
          }
        }
      }

      UpdateHotKeys();
    }

    const int SaveTimeKeyId=0, ToggleStopwatchKeyId=1; // global hotkey IDs
    const int HkAlt=1, HkCtrl=2, HkShift=4, HkNoRepeat=0x4000; // global hotkey flags

    bool CloseBoard()
    {
      if(!TrySaveChanges()) return false;
      board.Clear();
      fileName = null;
      return true;
    }

    void OpenBoard()
    {
      OpenFileDialog dialog = new OpenFileDialog();
      dialog.DefaultExt = "vmb";
      dialog.Filter     = "Virtual Maneuvering Boards (*.vmb)|*.vmb|All Files (*.*)|*.*";
      dialog.Title      = "Select the maneuvering board to open.";
      dialog.SupportMultiDottedExtensions = true;
      if(fileName != null)
      {
        string directory = Path.GetDirectoryName(fileName);
        if(Directory.Exists(directory)) dialog.InitialDirectory = directory;
      }

      if(dialog.ShowDialog() == DialogResult.OK && TrySaveChanges()) OpenBoard(dialog.FileName);
    }

    void OpenBoard(string fileName)
    {
      try
      {
        board.Load(fileName);
        this.fileName = fileName;
      }
      catch(Exception ex)
      {
        MessageBox.Show("An error occurred while loading " + fileName + ". (" + ex.Message + ")", "Load failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    void NewBoard()
    {
      if(CloseBoard())
      {
        UnitShape ownShip = new UnitShape() { Name = "Own ship", Type = UnitShapeType.OwnShip };
        board.Center     = AdamMil.Mathematics.Geometry.Point2.Empty;
        board.ZoomFactor = 1.0/32;
        board.RootShapes.Add(ownShip);
        board.ReferenceShape = ownShip;
        board.SelectedShape  = ownShip;
        board.WasChanged     = false;
      }
    }

    bool SaveBoard()
    {
      return fileName == null ? SaveBoardAs() : SaveBoard(fileName);
    }

    bool SaveBoard(string fileName)
    {
      try
      {
        board.Save(fileName);
        this.fileName = fileName;
        return true;
      }
      catch(Exception ex)
      {
        MessageBox.Show("An error occurred while save " + fileName + ". (" + ex.Message + ")", "Save failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return false;
    }

    bool SaveBoardAs()
    {
      SaveFileDialog dialog = new SaveFileDialog();
      dialog.DefaultExt = "vmb";
      dialog.Filter     = "Virtual Maneuvering Boards (*.vmb)|*.vmb|All Files (*.*)|*.*";
      dialog.Title      = "Where would you like to save this file?";
      dialog.SupportMultiDottedExtensions = true;
      if(fileName != null)
      {
        string directory = Path.GetDirectoryName(fileName);
        if(Directory.Exists(directory)) dialog.InitialDirectory = directory;
      }

      return dialog.ShowDialog() == DialogResult.OK && SaveBoard(dialog.FileName);
    }

    bool SetBackgroundImage()
    {
      BackgroundImageForm form = new BackgroundImageForm();
      if(form.ShowDialog() == DialogResult.OK)
      {
        if(board.BackgroundImage == null) // only set the center and scale if there was no background image alread. otherwise, reuse the
        {                                 // previous settings
          board.BackgroundImageCenter = board.Center;
          board.BackgroundImageScale  = ((double)board.Width / form.Image.Width) / board.ZoomFactor;
        }
        board.BackgroundImage = form.Image;
        board.SelectedTool    = board.SetupBackgroundTool;
        return true;
      }
      return false;
    }

    bool TrySaveChanges()
    {
      if(board.WasChanged)
      {
        DialogResult result = MessageBox.Show("Save changes to your maneuvering board?", "Save changes?", MessageBoxButtons.YesNoCancel,
                                              MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
        if(result == DialogResult.Cancel || result == DialogResult.Yes && !SaveBoard()) return false;
      }
      return true;
    }

    void UpdateHotKeys()
    {
      UnregisterHotKey(Handle, SaveTimeKeyId);
      UnregisterHotKey(Handle, ToggleStopwatchKeyId);

      int noRepeatFlag = noRepeatSupported ? HkNoRepeat : 0;
      if(saveTimeKey != 0) RegisterHotKey(Handle, SaveTimeKeyId, saveTimeKey>>16, saveTimeKey&0xFFFF);
      if(toggleStopwatchKey != 0) RegisterHotKey(Handle, ToggleStopwatchKeyId, toggleStopwatchKey>>16, toggleStopwatchKey&0xFFFF);
    }

    void board_BackgroundImageChanged(object sender, EventArgs e)
    {
      miRemoveBackground.Enabled = board.BackgroundImage != null;
      if(board.SelectedTool == board.SetupBackgroundTool) board.SelectedTool = board.PointerTool;
    }

    void board_ReferenceShapeChanged(object sender, EventArgs e)
    {
      tbAddObservation.Enabled = board.ReferenceShape != null;
    }

    void board_StatusTextChanged(object sender, EventArgs e)
    {
      lblToolStatus.Text = board.StatusText;
    }

    void board_ToolChanged(object sender, EventArgs e)
    {
      ToolStripButton toolBarButton;
      if(board.SelectedTool == board.PointerTool) toolBarButton = tbPointer;
      else if(board.SelectedTool == board.AddUnitTool) toolBarButton = tbAddUnit;
      else if(board.SelectedTool == board.AddObservationTool) toolBarButton = tbAddObservation;
      else if(board.SelectedTool == board.TMATool) toolBarButton = tbTMA;
      else if(board.SelectedTool == board.AddLineTool) toolBarButton = tbLine;
      else if(board.SelectedTool == board.AddCircleTool) toolBarButton = tbCircle;
      else if(board.SelectedTool == board.InterceptTool) toolBarButton = tbIntercept;
      else if(board.SelectedTool == board.SetupBackgroundTool) toolBarButton = tbSetBackground;
      else if(board.SelectedTool == board.SetupProjectionTool) toolBarButton = tbSetProjection;
      else throw new NotImplementedException();

      foreach(ToolStripButton button in toolStrip.Items.OfType<ToolStripButton>()) button.Checked = button == toolBarButton;
    }

    void miAbout_Click(object sender, EventArgs e)
    {
      using(AboutBox form = new AboutBox()) form.ShowDialog();
    }

    void miAdvanceTime_Click(object sender, EventArgs e)
    {
      using(AdvanceTimeForm form = new AdvanceTimeForm())
      {
        if(form.ShowDialog() == DialogResult.OK) board.AdvanceTime(form.Time, form.AdvanceUnitsWithWaypoints);
      }
    }

    void miBackgroundImage_Click(object sender, EventArgs e)
    {
      SetBackgroundImage();
    }

    void miBoardOptions_Click(object sender, EventArgs e)
    {
      using(BoardOptionsForm form = new BoardOptionsForm(board))
      {
        if(form.ShowDialog() == DialogResult.OK)
        {
          board.ShowAllObservations = form.ShowAllObservations;
          board.UnitSystem       = form.UnitSystem;
          board.BackColor        = form.BoardBackgroundColor;
          board.ObservationColor = form.ObservationColor;
          board.ReferenceColor   = form.ReferenceColor;
          board.ScaleColor1      = form.ScaleColor1;
          board.ScaleColor2      = form.ScaleColor2;
          board.SelectedColor    = form.SelectedColor;
          board.TMAColor         = form.TMAColor;
          board.UnselectedColor  = form.UnselectedColor;
        }
      }
    }

    void miContactShape_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      tbAddUnit.Image = menuItem.Image;
      foreach(ToolStripMenuItem item in tbUnitShape.DropDownItems) item.Checked = item == sender;
      board.AddUnitTool.Type = (UnitShapeType)menuItem.Tag;
      tbAddUnit.PerformClick();
    }

    void miExit_Click(object sender, EventArgs e)
    {
      Close();
    }

    void miNew_Click(object sender, EventArgs e)
    {
      NewBoard();
    }

    void miObsType_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
      tbAddObservation.Image = menuItem.Image;
      foreach(ToolStripMenuItem item in tbWaypointType.DropDownItems) item.Checked = item == sender;
      board.AddObservationTool.Type = (PositionalDataType)menuItem.Tag;
      tbAddObservation.PerformClick();
    }

    void miOpen_Click(object sender, EventArgs e)
    {
      OpenBoard();
    }

    void miProgramOptions_Click(object sender, EventArgs e)
    {
      using(ProgramOptionsForm form = new ProgramOptionsForm())
      {
        form.SaveTimeHotkey        = saveTimeKey;
        form.ToggleStopwatchHotkey = toggleStopwatchKey;
        if(form.ShowDialog() == DialogResult.OK)
        {
          saveTimeKey        = form.SaveTimeHotkey;
          toggleStopwatchKey = form.ToggleStopwatchHotkey;
          UpdateHotKeys();

          string dataPath = Program.GetDataDirectory();
          if(dataPath != null)
          {
            string configFile = Path.Combine(dataPath, "config.txt");
            try
            {
              using(StreamWriter writer = new StreamWriter(configFile))
              {
                writer.WriteLine("saveTimeHotkey=" + saveTimeKey.ToStringInvariant());
                writer.WriteLine("toggleStopwatchHotkey=" + toggleStopwatchKey.ToStringInvariant());
              }
              return;
            }
            catch { }
          }

          if(dataPath == null) dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
          MessageBox.Show("Unable to save program settings to " + dataPath, "Unable to save settings",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    void miQuickInterceptTool_Click(object sender, EventArgs e)
    {
      using(InterceptForm form = new InterceptForm(null, null, board.UnitSystem)) form.ShowDialog();
    }

    void miRemoveBackground_Click(object sender, EventArgs e)
    {
      board.BackgroundImage = null;
    }

    void miSave_Click(object sender, EventArgs e)
    {
      SaveBoard();
    }

    void miSaveAs_Click(object sender, EventArgs e)
    {
      SaveBoardAs();
    }

    void miStopwatch_Click(object sender, EventArgs e)
    {
      if(stopwatch == null || stopwatch.IsDisposed)
      {
        stopwatch = new StopwatchForm();
        stopwatch.StartPosition = FormStartPosition.Manual;
        stopwatch.Location = PointToScreen(new Point(board.Right-stopwatch.Width, board.Top));
      }
      if(!stopwatch.Visible) stopwatch.Show();
      else stopwatch.WindowState = FormWindowState.Normal;
    }

    void tbAddObservation_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.AddObservationTool;
    }

    void tbAddUnit_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.AddUnitTool;
    }

    void tbCircle_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.AddCircleTool;
    }

    void tbIntercept_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.InterceptTool;
    }

    void tbLine_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.AddLineTool;
    }

    void tbPointer_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.PointerTool;
    }

    void tbSetBackground_Click(object sender, EventArgs e)
    {
      if(board.BackgroundImage != null || SetBackgroundImage()) board.SelectedTool = board.SetupBackgroundTool;
    }

    void tbSetProjection_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.SetupProjectionTool;
    }

    void tbTMA_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.TMATool;
    }

    StopwatchForm stopwatch;
    string fileName;
    int saveTimeKey, toggleStopwatchKey;
    bool noRepeatSupported;

    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
  }
}
