using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AdamMil.Collections;
using AdamMil.Utilities;

namespace Maneubo
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
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
        else if(e.KeyCode == Keys.F3) tbUnitShape.ShowDropDown();
        else if(e.KeyCode == Keys.F4) tbWaypointType.ShowDropDown();
        else return;
        e.Handled = true;
      }
    }

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
        board.BackgroundImage       = form.Image;
        board.BackgroundImageCenter = board.Center;
        board.BackgroundImageScale  = ((double)board.Width / form.Image.Width) / board.ZoomFactor;
        board.SelectedTool = board.SetupBackgroundTool;
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

    void board_BackgroundImageChanged(object sender, EventArgs e)
    {
      miRemoveBackground.Enabled = board.BackgroundImage != null;
      if(board.SelectedTool == board.SetupBackgroundTool) board.SelectedTool = board.PointerTool;
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
      else if(board.SelectedTool == board.SetupBackgroundTool) toolBarButton = tbSetBackground;
      else throw new NotImplementedException();

      foreach(ToolStripButton button in toolStrip.Items.OfType<ToolStripButton>()) button.Checked = button == toolBarButton;
    }

    void miAbout_Click(object sender, EventArgs e)
    {
      new AboutBox().ShowDialog();
    }

    void miBackgroundImage_Click(object sender, EventArgs e)
    {
      SetBackgroundImage();
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

    void tbTMA_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.TMATool;
    }

    string fileName;
  }
}
