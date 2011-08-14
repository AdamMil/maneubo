namespace Maneubo
{
  partial class TMAForm
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
      System.Windows.Forms.Label lblCourse;
      System.Windows.Forms.Label lblSpeed;
      this.txtCourse = new Maneubo.ChangeTrackingTextBox();
      this.txtSpeed = new Maneubo.ChangeTrackingTextBox();
      this.btnAuto = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.chkLockCourse = new System.Windows.Forms.CheckBox();
      this.chkLockSpeed = new System.Windows.Forms.CheckBox();
      lblCourse = new System.Windows.Forms.Label();
      lblSpeed = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblCourse
      // 
      lblCourse.AutoSize = true;
      lblCourse.Location = new System.Drawing.Point(9, 13);
      lblCourse.Name = "lblCourse";
      lblCourse.Size = new System.Drawing.Size(40, 13);
      lblCourse.TabIndex = 0;
      lblCourse.Text = "&Course";
      lblCourse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblSpeed
      // 
      lblSpeed.AutoSize = true;
      lblSpeed.Location = new System.Drawing.Point(9, 38);
      lblSpeed.Name = "lblSpeed";
      lblSpeed.Size = new System.Drawing.Size(38, 13);
      lblSpeed.TabIndex = 3;
      lblSpeed.Text = "&Speed";
      lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtCourse
      // 
      this.txtCourse.Location = new System.Drawing.Point(56, 9);
      this.txtCourse.Name = "txtCourse";
      this.txtCourse.Size = new System.Drawing.Size(80, 20);
      this.txtCourse.TabIndex = 1;
      this.txtCourse.WasChanged = false;
      this.txtCourse.Validating += new System.ComponentModel.CancelEventHandler(this.txtCourse_Validating);
      // 
      // txtSpeed
      // 
      this.txtSpeed.Location = new System.Drawing.Point(56, 35);
      this.txtSpeed.Name = "txtSpeed";
      this.txtSpeed.Size = new System.Drawing.Size(80, 20);
      this.txtSpeed.TabIndex = 4;
      this.txtSpeed.WasChanged = false;
      this.txtSpeed.Validating += new System.ComponentModel.CancelEventHandler(this.txtSpeed_Validating);
      // 
      // btnAuto
      // 
      this.btnAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnAuto.Location = new System.Drawing.Point(8, 62);
      this.btnAuto.Name = "btnAuto";
      this.btnAuto.Size = new System.Drawing.Size(77, 23);
      this.btnAuto.TabIndex = 12;
      this.btnAuto.Text = "Auto &TMA";
      this.btnAuto.UseVisualStyleBackColor = true;
      this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnApply.Location = new System.Drawing.Point(91, 62);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(65, 23);
      this.btnApply.TabIndex = 13;
      this.btnApply.Text = "&Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // chkLockCourse
      // 
      this.chkLockCourse.AutoSize = true;
      this.chkLockCourse.Location = new System.Drawing.Point(141, 12);
      this.chkLockCourse.Name = "chkLockCourse";
      this.chkLockCourse.Size = new System.Drawing.Size(15, 14);
      this.chkLockCourse.TabIndex = 2;
      this.chkLockCourse.UseVisualStyleBackColor = true;
      // 
      // chkLockSpeed
      // 
      this.chkLockSpeed.AutoSize = true;
      this.chkLockSpeed.Location = new System.Drawing.Point(141, 38);
      this.chkLockSpeed.Name = "chkLockSpeed";
      this.chkLockSpeed.Size = new System.Drawing.Size(15, 14);
      this.chkLockSpeed.TabIndex = 5;
      this.chkLockSpeed.UseVisualStyleBackColor = true;
      // 
      // TMAForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(163, 91);
      this.Controls.Add(this.chkLockSpeed);
      this.Controls.Add(this.chkLockCourse);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.btnAuto);
      this.Controls.Add(this.txtSpeed);
      this.Controls.Add(lblSpeed);
      this.Controls.Add(this.txtCourse);
      this.Controls.Add(lblCourse);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "TMAForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Target Motion Analysis";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Maneubo.ChangeTrackingTextBox txtCourse;
    private Maneubo.ChangeTrackingTextBox txtSpeed;
    private System.Windows.Forms.Button btnAuto;
    private System.Windows.Forms.Button btnApply;
    private System.Windows.Forms.CheckBox chkLockCourse;
    private System.Windows.Forms.CheckBox chkLockSpeed;

  }
}