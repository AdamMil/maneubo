namespace Maneubo
{
  partial class ShapeDataForm
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
      System.Windows.Forms.GroupBox grpUnit;
      System.Windows.Forms.Label lblType;
      System.Windows.Forms.Label lblSpeed;
      System.Windows.Forms.Label lblDegrees;
      System.Windows.Forms.GroupBox grpShape;
      System.Windows.Forms.Label lblParentLabel;
      System.Windows.Forms.Label lblName;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Button btnOK;
      this.cmbType = new System.Windows.Forms.ComboBox();
      this.chkRelative = new System.Windows.Forms.CheckBox();
      this.txtSpeed = new System.Windows.Forms.TextBox();
      this.txtDirection = new System.Windows.Forms.TextBox();
      this.lblDirection = new System.Windows.Forms.Label();
      this.lblParent = new System.Windows.Forms.Label();
      this.txtSize = new System.Windows.Forms.TextBox();
      this.lblSize = new System.Windows.Forms.Label();
      this.txtName = new System.Windows.Forms.TextBox();
      grpUnit = new System.Windows.Forms.GroupBox();
      lblType = new System.Windows.Forms.Label();
      lblSpeed = new System.Windows.Forms.Label();
      lblDegrees = new System.Windows.Forms.Label();
      grpShape = new System.Windows.Forms.GroupBox();
      lblParentLabel = new System.Windows.Forms.Label();
      lblName = new System.Windows.Forms.Label();
      btnCancel = new System.Windows.Forms.Button();
      btnOK = new System.Windows.Forms.Button();
      grpUnit.SuspendLayout();
      grpShape.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpUnit
      // 
      grpUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      grpUnit.Controls.Add(lblType);
      grpUnit.Controls.Add(this.cmbType);
      grpUnit.Controls.Add(this.chkRelative);
      grpUnit.Controls.Add(this.txtSpeed);
      grpUnit.Controls.Add(lblSpeed);
      grpUnit.Controls.Add(lblDegrees);
      grpUnit.Controls.Add(this.txtDirection);
      grpUnit.Controls.Add(this.lblDirection);
      grpUnit.Location = new System.Drawing.Point(12, 112);
      grpUnit.Name = "grpUnit";
      grpUnit.Size = new System.Drawing.Size(196, 120);
      grpUnit.TabIndex = 1;
      grpUnit.TabStop = false;
      grpUnit.Text = "Unit Data";
      // 
      // lblType
      // 
      lblType.AutoSize = true;
      lblType.Location = new System.Drawing.Point(8, 72);
      lblType.Name = "lblType";
      lblType.Size = new System.Drawing.Size(31, 13);
      lblType.TabIndex = 8;
      lblType.Text = "&Type";
      lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbType.Items.AddRange(new object[] {
            "Air",
            "Boat",
            "Helicopter",
            "Land",
            "Own ship",
            "Subsurface",
            "Surface",
            "Unknown",
            "Weapon"});
      this.cmbType.Location = new System.Drawing.Point(63, 68);
      this.cmbType.Name = "cmbType";
      this.cmbType.Size = new System.Drawing.Size(122, 21);
      this.cmbType.TabIndex = 9;
      // 
      // chkRelative
      // 
      this.chkRelative.AutoSize = true;
      this.chkRelative.Location = new System.Drawing.Point(11, 95);
      this.chkRelative.Name = "chkRelative";
      this.chkRelative.Size = new System.Drawing.Size(155, 17);
      this.chkRelative.TabIndex = 10;
      this.chkRelative.Text = "Velocity is &relative to parent";
      this.chkRelative.UseVisualStyleBackColor = true;
      // 
      // txtSpeed
      // 
      this.txtSpeed.Location = new System.Drawing.Point(63, 42);
      this.txtSpeed.Name = "txtSpeed";
      this.txtSpeed.Size = new System.Drawing.Size(71, 20);
      this.txtSpeed.TabIndex = 7;
      this.txtSpeed.TextChanged += new System.EventHandler(this.txtSpeed_TextChanged);
      // 
      // lblSpeed
      // 
      lblSpeed.AutoSize = true;
      lblSpeed.Location = new System.Drawing.Point(8, 46);
      lblSpeed.Name = "lblSpeed";
      lblSpeed.Size = new System.Drawing.Size(38, 13);
      lblSpeed.TabIndex = 6;
      lblSpeed.Text = "&Speed";
      lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDegrees
      // 
      lblDegrees.AutoSize = true;
      lblDegrees.Location = new System.Drawing.Point(140, 20);
      lblDegrees.Name = "lblDegrees";
      lblDegrees.Size = new System.Drawing.Size(45, 13);
      lblDegrees.TabIndex = 5;
      lblDegrees.Text = "degrees";
      lblDegrees.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtDirection
      // 
      this.txtDirection.Location = new System.Drawing.Point(63, 16);
      this.txtDirection.Name = "txtDirection";
      this.txtDirection.Size = new System.Drawing.Size(71, 20);
      this.txtDirection.TabIndex = 4;
      this.txtDirection.TextChanged += new System.EventHandler(this.txtDirection_TextChanged);
      // 
      // lblDirection
      // 
      this.lblDirection.AutoSize = true;
      this.lblDirection.Location = new System.Drawing.Point(7, 20);
      this.lblDirection.Name = "lblDirection";
      this.lblDirection.Size = new System.Drawing.Size(49, 13);
      this.lblDirection.TabIndex = 1;
      this.lblDirection.Text = "&Direction";
      this.lblDirection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpShape
      // 
      grpShape.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      grpShape.Controls.Add(this.lblParent);
      grpShape.Controls.Add(lblParentLabel);
      grpShape.Controls.Add(this.txtSize);
      grpShape.Controls.Add(this.lblSize);
      grpShape.Controls.Add(this.txtName);
      grpShape.Controls.Add(lblName);
      grpShape.Location = new System.Drawing.Point(12, 8);
      grpShape.Name = "grpShape";
      grpShape.Size = new System.Drawing.Size(196, 98);
      grpShape.TabIndex = 0;
      grpShape.TabStop = false;
      grpShape.Text = "Shape Data";
      // 
      // lblParent
      // 
      this.lblParent.AutoSize = true;
      this.lblParent.Location = new System.Drawing.Point(63, 72);
      this.lblParent.Name = "lblParent";
      this.lblParent.Size = new System.Drawing.Size(0, 13);
      this.lblParent.TabIndex = 5;
      this.lblParent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblParentLabel
      // 
      lblParentLabel.AutoSize = true;
      lblParentLabel.Location = new System.Drawing.Point(7, 72);
      lblParentLabel.Name = "lblParentLabel";
      lblParentLabel.Size = new System.Drawing.Size(38, 13);
      lblParentLabel.TabIndex = 4;
      lblParentLabel.Text = "Parent";
      lblParentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtSize
      // 
      this.txtSize.Location = new System.Drawing.Point(63, 42);
      this.txtSize.Name = "txtSize";
      this.txtSize.Size = new System.Drawing.Size(71, 20);
      this.txtSize.TabIndex = 3;
      this.txtSize.TextChanged += new System.EventHandler(this.txtSize_TextChanged);
      // 
      // lblSize
      // 
      this.lblSize.AutoSize = true;
      this.lblSize.Location = new System.Drawing.Point(7, 46);
      this.lblSize.Name = "lblSize";
      this.lblSize.Size = new System.Drawing.Size(27, 13);
      this.lblSize.TabIndex = 2;
      this.lblSize.Text = "Size";
      this.lblSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtName
      // 
      this.txtName.Location = new System.Drawing.Point(63, 16);
      this.txtName.Name = "txtName";
      this.txtName.Size = new System.Drawing.Size(122, 20);
      this.txtName.TabIndex = 1;
      // 
      // lblName
      // 
      lblName.AutoSize = true;
      lblName.Location = new System.Drawing.Point(7, 20);
      lblName.Name = "lblName";
      lblName.Size = new System.Drawing.Size(35, 13);
      lblName.TabIndex = 0;
      lblName.Text = "&Name";
      lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(132, 238);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "&Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(51, 238);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 2;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // ShapeDataForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(220, 267);
      this.Controls.Add(btnOK);
      this.Controls.Add(btnCancel);
      this.Controls.Add(grpShape);
      this.Controls.Add(grpUnit);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ShapeDataForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Shape Data";
      grpUnit.ResumeLayout(false);
      grpUnit.PerformLayout();
      grpShape.ResumeLayout(false);
      grpShape.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblParent;
    private System.Windows.Forms.TextBox txtSize;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.TextBox txtDirection;
    private System.Windows.Forms.CheckBox chkRelative;
    private System.Windows.Forms.TextBox txtSpeed;
    private System.Windows.Forms.Label lblDirection;
    private System.Windows.Forms.Label lblSize;
    private System.Windows.Forms.ComboBox cmbType;

  }
}