﻿using System;
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

    bool CloseBoard()
    {
      if(!TrySaveChanges()) return false;
      board.ReferenceShape = null;
      board.SelectedShape = null;
      board.RootShapes.Clear();
      board.WasChanged = false;
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
      this.fileName = fileName;
      throw new NotImplementedException();
    }

    void NewBoard()
    {
      if(CloseBoard())
      {
        UnitShape ownShip = new UnitShape() { Name="Own ship" };
        board.Center     = AdamMil.Mathematics.Geometry.Point2.Empty;
        board.ZoomFactor = 1.0/32;
        board.RootShapes.Add(ownShip);
        board.ReferenceShape = ownShip;
      }
    }

    bool SaveBoard()
    {
      return fileName == null ? SaveBoardAs() : SaveBoard(fileName);
    }

    bool SaveBoard(string fileName)
    {
      this.fileName = fileName;
      throw new NotImplementedException();
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

    void board_SelectionChanged(object sender, EventArgs e)
    {
      tbAddObservation.Enabled = tbWaypointType.Enabled = board.SelectedShape is UnitShape || board.SelectedShape is Observation;
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
      else throw new NotImplementedException();

      foreach(ToolStripButton button in toolStrip.Items.OfType<ToolStripButton>()) button.Checked = button == toolBarButton;
    }

    void miContactShape_Click(object sender, EventArgs e)
    {
      tbAddUnit.Image = ((ToolStripMenuItem)sender).Image;
      foreach(ToolStripMenuItem item in tbUnitShape.DropDownItems) item.Checked = item == sender;
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
      board.AddObservationTool.Type = (ObservationType)menuItem.Tag;
    }

    void miOpen_Click(object sender, EventArgs e)
    {
      OpenBoard();
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

    void tbPointer_Click(object sender, EventArgs e)
    {
      board.SelectedTool = board.PointerTool;
    }

    string fileName;
  }
}