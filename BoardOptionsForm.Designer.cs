namespace Maneubo
{
  partial class BoardOptionsForm
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
      System.Windows.Forms.GroupBox grpDisplay;
      System.Windows.Forms.Label lblUnitSystem;
      System.Windows.Forms.Label lblBackground;
      System.Windows.Forms.Label lblUnselected;
      System.Windows.Forms.Label lblSelected;
      System.Windows.Forms.Label lblReference;
      System.Windows.Forms.Label lblObservations;
      System.Windows.Forms.Label lblTMA;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Label lblScaleBar;
      this.cmbUnitSystem = new System.Windows.Forms.ComboBox();
      this.chkShowAllObservations = new System.Windows.Forms.CheckBox();
      this.grpColors = new System.Windows.Forms.GroupBox();
      this.btnTMA = new System.Windows.Forms.Button();
      this.btnObservations = new System.Windows.Forms.Button();
      this.btnReference = new System.Windows.Forms.Button();
      this.btnSelected = new System.Windows.Forms.Button();
      this.btnUnselected = new System.Windows.Forms.Button();
      this.btnBackground = new System.Windows.Forms.Button();
      this.colorDialog = new System.Windows.Forms.ColorDialog();
      this.btnScale1 = new System.Windows.Forms.Button();
      this.btnScale2 = new System.Windows.Forms.Button();
      grpDisplay = new System.Windows.Forms.GroupBox();
      lblUnitSystem = new System.Windows.Forms.Label();
      lblBackground = new System.Windows.Forms.Label();
      lblUnselected = new System.Windows.Forms.Label();
      lblSelected = new System.Windows.Forms.Label();
      lblReference = new System.Windows.Forms.Label();
      lblObservations = new System.Windows.Forms.Label();
      lblTMA = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      lblScaleBar = new System.Windows.Forms.Label();
      grpDisplay.SuspendLayout();
      this.grpColors.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpDisplay
      // 
      grpDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      grpDisplay.Controls.Add(this.cmbUnitSystem);
      grpDisplay.Controls.Add(lblUnitSystem);
      grpDisplay.Controls.Add(this.chkShowAllObservations);
      grpDisplay.Location = new System.Drawing.Point(11, 9);
      grpDisplay.Name = "grpDisplay";
      grpDisplay.Size = new System.Drawing.Size(267, 70);
      grpDisplay.TabIndex = 0;
      grpDisplay.TabStop = false;
      grpDisplay.Text = "Display Options";
      // 
      // cmbUnitSystem
      // 
      this.cmbUnitSystem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbUnitSystem.FormattingEnabled = true;
      this.cmbUnitSystem.Items.AddRange(new object[] {
            "Nautical (metric)",
            "Nautical (imperial)",
            "Terrestrial (metric)",
            "Terrestrial (imperial)"});
      this.cmbUnitSystem.Location = new System.Drawing.Point(75, 16);
      this.cmbUnitSystem.Name = "cmbUnitSystem";
      this.cmbUnitSystem.Size = new System.Drawing.Size(181, 21);
      this.cmbUnitSystem.TabIndex = 1;
      // 
      // lblUnitSystem
      // 
      lblUnitSystem.AutoSize = true;
      lblUnitSystem.Location = new System.Drawing.Point(7, 20);
      lblUnitSystem.Name = "lblUnitSystem";
      lblUnitSystem.Size = new System.Drawing.Size(61, 13);
      lblUnitSystem.TabIndex = 0;
      lblUnitSystem.Text = "&Unit system";
      lblUnitSystem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // chkShowAllObservations
      // 
      this.chkShowAllObservations.AutoSize = true;
      this.chkShowAllObservations.Location = new System.Drawing.Point(10, 43);
      this.chkShowAllObservations.Name = "chkShowAllObservations";
      this.chkShowAllObservations.Size = new System.Drawing.Size(200, 17);
      this.chkShowAllObservations.TabIndex = 2;
      this.chkShowAllObservations.Text = "Show all &observations and waypoints";
      this.chkShowAllObservations.UseVisualStyleBackColor = true;
      // 
      // lblBackground
      // 
      lblBackground.AutoSize = true;
      lblBackground.Location = new System.Drawing.Point(10, 20);
      lblBackground.Name = "lblBackground";
      lblBackground.Size = new System.Drawing.Size(91, 13);
      lblBackground.TabIndex = 0;
      lblBackground.Text = "Background color";
      lblBackground.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblUnselected
      // 
      lblUnselected.AutoSize = true;
      lblUnselected.Location = new System.Drawing.Point(10, 49);
      lblUnselected.Name = "lblUnselected";
      lblUnselected.Size = new System.Drawing.Size(98, 13);
      lblUnselected.TabIndex = 2;
      lblUnselected.Text = "Unselected shapes";
      lblUnselected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblSelected
      // 
      lblSelected.AutoSize = true;
      lblSelected.Location = new System.Drawing.Point(10, 78);
      lblSelected.Name = "lblSelected";
      lblSelected.Size = new System.Drawing.Size(86, 13);
      lblSelected.TabIndex = 4;
      lblSelected.Text = "Selected shapes";
      lblSelected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblReference
      // 
      lblReference.AutoSize = true;
      lblReference.Location = new System.Drawing.Point(10, 107);
      lblReference.Name = "lblReference";
      lblReference.Size = new System.Drawing.Size(82, 13);
      lblReference.TabIndex = 6;
      lblReference.Text = "Reference units";
      lblReference.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblObservations
      // 
      lblObservations.AutoSize = true;
      lblObservations.Location = new System.Drawing.Point(10, 136);
      lblObservations.Name = "lblObservations";
      lblObservations.Size = new System.Drawing.Size(69, 13);
      lblObservations.TabIndex = 8;
      lblObservations.Text = "Observations";
      lblObservations.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTMA
      // 
      lblTMA.AutoSize = true;
      lblTMA.Location = new System.Drawing.Point(10, 165);
      lblTMA.Name = "lblTMA";
      lblTMA.Size = new System.Drawing.Size(48, 13);
      lblTMA.TabIndex = 10;
      lblTMA.Text = "TMA bar";
      lblTMA.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      btnOK.Location = new System.Drawing.Point(122, 320);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 2;
      btnOK.Text = "OK";
      btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(203, 320);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpColors
      // 
      this.grpColors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpColors.Controls.Add(this.btnScale2);
      this.grpColors.Controls.Add(this.btnScale1);
      this.grpColors.Controls.Add(lblScaleBar);
      this.grpColors.Controls.Add(this.btnTMA);
      this.grpColors.Controls.Add(lblTMA);
      this.grpColors.Controls.Add(this.btnObservations);
      this.grpColors.Controls.Add(lblObservations);
      this.grpColors.Controls.Add(this.btnReference);
      this.grpColors.Controls.Add(lblReference);
      this.grpColors.Controls.Add(this.btnSelected);
      this.grpColors.Controls.Add(lblSelected);
      this.grpColors.Controls.Add(this.btnUnselected);
      this.grpColors.Controls.Add(lblUnselected);
      this.grpColors.Controls.Add(lblBackground);
      this.grpColors.Controls.Add(this.btnBackground);
      this.grpColors.Location = new System.Drawing.Point(11, 86);
      this.grpColors.Name = "grpColors";
      this.grpColors.Size = new System.Drawing.Size(267, 225);
      this.grpColors.TabIndex = 1;
      this.grpColors.TabStop = false;
      this.grpColors.Text = "&Colors";
      // 
      // btnTMA
      // 
      this.btnTMA.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnTMA.Location = new System.Drawing.Point(119, 160);
      this.btnTMA.Name = "btnTMA";
      this.btnTMA.Size = new System.Drawing.Size(25, 23);
      this.btnTMA.TabIndex = 11;
      this.btnTMA.UseVisualStyleBackColor = true;
      this.btnTMA.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnObservations
      // 
      this.btnObservations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnObservations.Location = new System.Drawing.Point(119, 131);
      this.btnObservations.Name = "btnObservations";
      this.btnObservations.Size = new System.Drawing.Size(25, 23);
      this.btnObservations.TabIndex = 9;
      this.btnObservations.UseVisualStyleBackColor = true;
      this.btnObservations.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnReference
      // 
      this.btnReference.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnReference.Location = new System.Drawing.Point(119, 102);
      this.btnReference.Name = "btnReference";
      this.btnReference.Size = new System.Drawing.Size(25, 23);
      this.btnReference.TabIndex = 7;
      this.btnReference.UseVisualStyleBackColor = true;
      this.btnReference.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnSelected
      // 
      this.btnSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSelected.Location = new System.Drawing.Point(119, 73);
      this.btnSelected.Name = "btnSelected";
      this.btnSelected.Size = new System.Drawing.Size(25, 23);
      this.btnSelected.TabIndex = 5;
      this.btnSelected.UseVisualStyleBackColor = true;
      this.btnSelected.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnUnselected
      // 
      this.btnUnselected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnUnselected.Location = new System.Drawing.Point(119, 44);
      this.btnUnselected.Name = "btnUnselected";
      this.btnUnselected.Size = new System.Drawing.Size(25, 23);
      this.btnUnselected.TabIndex = 3;
      this.btnUnselected.UseVisualStyleBackColor = true;
      this.btnUnselected.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnBackground
      // 
      this.btnBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnBackground.Location = new System.Drawing.Point(119, 15);
      this.btnBackground.Name = "btnBackground";
      this.btnBackground.Size = new System.Drawing.Size(25, 23);
      this.btnBackground.TabIndex = 1;
      this.btnBackground.UseVisualStyleBackColor = true;
      this.btnBackground.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // btnScale1
      // 
      this.btnScale1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScale1.Location = new System.Drawing.Point(119, 189);
      this.btnScale1.Name = "btnScale1";
      this.btnScale1.Size = new System.Drawing.Size(25, 23);
      this.btnScale1.TabIndex = 13;
      this.btnScale1.UseVisualStyleBackColor = true;
      this.btnScale1.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // lblScaleBar
      // 
      lblScaleBar.AutoSize = true;
      lblScaleBar.Location = new System.Drawing.Point(10, 194);
      lblScaleBar.Name = "lblScaleBar";
      lblScaleBar.Size = new System.Drawing.Size(74, 13);
      lblScaleBar.TabIndex = 12;
      lblScaleBar.Text = "Map scale bar";
      lblScaleBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnScale2
      // 
      this.btnScale2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScale2.Location = new System.Drawing.Point(150, 189);
      this.btnScale2.Name = "btnScale2";
      this.btnScale2.Size = new System.Drawing.Size(25, 23);
      this.btnScale2.TabIndex = 14;
      this.btnScale2.UseVisualStyleBackColor = true;
      this.btnScale2.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // BoardOptionsForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(289, 351);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.grpColors);
      this.Controls.Add(grpDisplay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "BoardOptionsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Board Options";
      grpDisplay.ResumeLayout(false);
      grpDisplay.PerformLayout();
      this.grpColors.ResumeLayout(false);
      this.grpColors.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox cmbUnitSystem;
    private System.Windows.Forms.CheckBox chkShowAllObservations;
    private System.Windows.Forms.GroupBox grpColors;
    private System.Windows.Forms.Button btnBackground;
    private System.Windows.Forms.ColorDialog colorDialog;
    private System.Windows.Forms.Button btnObservations;
    private System.Windows.Forms.Button btnReference;
    private System.Windows.Forms.Button btnSelected;
    private System.Windows.Forms.Button btnUnselected;
    private System.Windows.Forms.Button btnTMA;
    private System.Windows.Forms.Button btnScale2;
    private System.Windows.Forms.Button btnScale1;
  }
}