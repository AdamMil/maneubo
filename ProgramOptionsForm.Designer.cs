namespace Maneubo
{
  partial class ProgramOptionsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if(disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.GroupBox grpHotkeys;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.txtTSChar = new System.Windows.Forms.TextBox();
      this.chkTSShift = new System.Windows.Forms.CheckBox();
      this.chkTSCtrl = new System.Windows.Forms.CheckBox();
      this.chkTSAlt = new System.Windows.Forms.CheckBox();
      this.chkSaveTime = new System.Windows.Forms.CheckBox();
      this.txtSTChar = new System.Windows.Forms.TextBox();
      this.chkSTShift = new System.Windows.Forms.CheckBox();
      this.chkSTCtrl = new System.Windows.Forms.CheckBox();
      this.chkSTAlt = new System.Windows.Forms.CheckBox();
      this.chkToggleStopwatch = new System.Windows.Forms.CheckBox();
      grpHotkeys = new System.Windows.Forms.GroupBox();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      grpHotkeys.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpHotkeys
      // 
      grpHotkeys.Controls.Add(this.txtTSChar);
      grpHotkeys.Controls.Add(this.chkTSShift);
      grpHotkeys.Controls.Add(this.chkTSCtrl);
      grpHotkeys.Controls.Add(this.chkTSAlt);
      grpHotkeys.Controls.Add(this.chkSaveTime);
      grpHotkeys.Controls.Add(this.txtSTChar);
      grpHotkeys.Controls.Add(this.chkSTShift);
      grpHotkeys.Controls.Add(this.chkSTCtrl);
      grpHotkeys.Controls.Add(this.chkSTAlt);
      grpHotkeys.Controls.Add(this.chkToggleStopwatch);
      grpHotkeys.Location = new System.Drawing.Point(13, 13);
      grpHotkeys.Name = "grpHotkeys";
      grpHotkeys.Size = new System.Drawing.Size(340, 70);
      grpHotkeys.TabIndex = 0;
      grpHotkeys.TabStop = false;
      grpHotkeys.Text = "Global Hotkeys";
      // 
      // txtTSChar
      // 
      this.txtTSChar.Location = new System.Drawing.Point(289, 40);
      this.txtTSChar.Name = "txtTSChar";
      this.txtTSChar.Size = new System.Drawing.Size(37, 20);
      this.txtTSChar.TabIndex = 9;
      // 
      // chkTSShift
      // 
      this.chkTSShift.AutoSize = true;
      this.chkTSShift.Location = new System.Drawing.Point(235, 42);
      this.chkTSShift.Name = "chkTSShift";
      this.chkTSShift.Size = new System.Drawing.Size(47, 17);
      this.chkTSShift.TabIndex = 8;
      this.chkTSShift.Text = "Shift";
      this.chkTSShift.UseVisualStyleBackColor = true;
      // 
      // chkTSCtrl
      // 
      this.chkTSCtrl.AutoSize = true;
      this.chkTSCtrl.Location = new System.Drawing.Point(188, 42);
      this.chkTSCtrl.Name = "chkTSCtrl";
      this.chkTSCtrl.Size = new System.Drawing.Size(41, 17);
      this.chkTSCtrl.TabIndex = 7;
      this.chkTSCtrl.Text = "Ctrl";
      this.chkTSCtrl.UseVisualStyleBackColor = true;
      // 
      // chkTSAlt
      // 
      this.chkTSAlt.AutoSize = true;
      this.chkTSAlt.Location = new System.Drawing.Point(144, 42);
      this.chkTSAlt.Name = "chkTSAlt";
      this.chkTSAlt.Size = new System.Drawing.Size(38, 17);
      this.chkTSAlt.TabIndex = 6;
      this.chkTSAlt.Text = "Alt";
      this.chkTSAlt.UseVisualStyleBackColor = true;
      // 
      // chkSaveTime
      // 
      this.chkSaveTime.AutoSize = true;
      this.chkSaveTime.Location = new System.Drawing.Point(10, 42);
      this.chkSaveTime.Name = "chkSaveTime";
      this.chkSaveTime.Size = new System.Drawing.Size(125, 17);
      this.chkSaveTime.TabIndex = 5;
      this.chkSaveTime.Text = "Stopwatch save &time";
      this.chkSaveTime.UseVisualStyleBackColor = true;
      // 
      // txtSTChar
      // 
      this.txtSTChar.Location = new System.Drawing.Point(289, 17);
      this.txtSTChar.Name = "txtSTChar";
      this.txtSTChar.Size = new System.Drawing.Size(37, 20);
      this.txtSTChar.TabIndex = 4;
      // 
      // chkSTShift
      // 
      this.chkSTShift.AutoSize = true;
      this.chkSTShift.Location = new System.Drawing.Point(235, 19);
      this.chkSTShift.Name = "chkSTShift";
      this.chkSTShift.Size = new System.Drawing.Size(47, 17);
      this.chkSTShift.TabIndex = 3;
      this.chkSTShift.Text = "Shift";
      this.chkSTShift.UseVisualStyleBackColor = true;
      // 
      // chkSTCtrl
      // 
      this.chkSTCtrl.AutoSize = true;
      this.chkSTCtrl.Location = new System.Drawing.Point(188, 19);
      this.chkSTCtrl.Name = "chkSTCtrl";
      this.chkSTCtrl.Size = new System.Drawing.Size(41, 17);
      this.chkSTCtrl.TabIndex = 2;
      this.chkSTCtrl.Text = "Ctrl";
      this.chkSTCtrl.UseVisualStyleBackColor = true;
      // 
      // chkSTAlt
      // 
      this.chkSTAlt.AutoSize = true;
      this.chkSTAlt.Location = new System.Drawing.Point(144, 19);
      this.chkSTAlt.Name = "chkSTAlt";
      this.chkSTAlt.Size = new System.Drawing.Size(38, 17);
      this.chkSTAlt.TabIndex = 1;
      this.chkSTAlt.Text = "Alt";
      this.chkSTAlt.UseVisualStyleBackColor = true;
      // 
      // chkToggleStopwatch
      // 
      this.chkToggleStopwatch.AutoSize = true;
      this.chkToggleStopwatch.Location = new System.Drawing.Point(10, 19);
      this.chkToggleStopwatch.Name = "chkToggleStopwatch";
      this.chkToggleStopwatch.Size = new System.Drawing.Size(125, 17);
      this.chkToggleStopwatch.TabIndex = 0;
      this.chkToggleStopwatch.Text = "Stopwatch &start/stop";
      this.chkToggleStopwatch.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(197, 93);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 1;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(278, 93);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 2;
      btnCancel.Text = "&Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // ProgramOptionsForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(365, 126);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(grpHotkeys);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ProgramOptionsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Maneubo Options";
      grpHotkeys.ResumeLayout(false);
      grpHotkeys.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.CheckBox chkToggleStopwatch;
    private System.Windows.Forms.TextBox txtTSChar;
    private System.Windows.Forms.CheckBox chkTSShift;
    private System.Windows.Forms.CheckBox chkTSCtrl;
    private System.Windows.Forms.CheckBox chkTSAlt;
    private System.Windows.Forms.CheckBox chkSaveTime;
    private System.Windows.Forms.TextBox txtSTChar;
    private System.Windows.Forms.CheckBox chkSTShift;
    private System.Windows.Forms.CheckBox chkSTCtrl;
    private System.Windows.Forms.CheckBox chkSTAlt;
  }
}