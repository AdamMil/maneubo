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
      System.Windows.Forms.Button btnAuto;
      System.Windows.Forms.GroupBox grpAutoTMA;
      System.Windows.Forms.Label lblDash2;
      System.Windows.Forms.Label lblAutoSpeed;
      System.Windows.Forms.Label lblDash1;
      System.Windows.Forms.Label lblAutoCourse;
      System.Windows.Forms.GroupBox grpManualTMA;
      System.Windows.Forms.Button btnOptimize;
      System.Windows.Forms.Button btnApply;
      System.Windows.Forms.Label lblSpeed;
      System.Windows.Forms.Label lblCourse;
      this.txtMaxSpeed = new System.Windows.Forms.TextBox();
      this.txtMinSpeed = new System.Windows.Forms.TextBox();
      this.txtMaxCourse = new System.Windows.Forms.TextBox();
      this.txtMinCourse = new System.Windows.Forms.TextBox();
      this.chkLockSpeed = new System.Windows.Forms.CheckBox();
      this.chkLockCourse = new System.Windows.Forms.CheckBox();
      this.txtSpeed = new Maneubo.ChangeTrackingTextBox();
      this.txtCourse = new Maneubo.ChangeTrackingTextBox();
      btnAuto = new System.Windows.Forms.Button();
      grpAutoTMA = new System.Windows.Forms.GroupBox();
      lblDash2 = new System.Windows.Forms.Label();
      lblAutoSpeed = new System.Windows.Forms.Label();
      lblDash1 = new System.Windows.Forms.Label();
      lblAutoCourse = new System.Windows.Forms.Label();
      grpManualTMA = new System.Windows.Forms.GroupBox();
      btnOptimize = new System.Windows.Forms.Button();
      btnApply = new System.Windows.Forms.Button();
      lblSpeed = new System.Windows.Forms.Label();
      lblCourse = new System.Windows.Forms.Label();
      grpAutoTMA.SuspendLayout();
      grpManualTMA.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnAuto
      // 
      btnAuto.Location = new System.Drawing.Point(10, 70);
      btnAuto.Name = "btnAuto";
      btnAuto.Size = new System.Drawing.Size(77, 23);
      btnAuto.TabIndex = 8;
      btnAuto.Text = "Auto &TMA";
      btnAuto.UseVisualStyleBackColor = true;
      btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
      // 
      // grpAutoTMA
      // 
      grpAutoTMA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      grpAutoTMA.Controls.Add(lblDash2);
      grpAutoTMA.Controls.Add(this.txtMaxSpeed);
      grpAutoTMA.Controls.Add(this.txtMinSpeed);
      grpAutoTMA.Controls.Add(lblAutoSpeed);
      grpAutoTMA.Controls.Add(lblDash1);
      grpAutoTMA.Controls.Add(this.txtMaxCourse);
      grpAutoTMA.Controls.Add(this.txtMinCourse);
      grpAutoTMA.Controls.Add(lblAutoCourse);
      grpAutoTMA.Controls.Add(btnAuto);
      grpAutoTMA.Location = new System.Drawing.Point(9, 117);
      grpAutoTMA.Name = "grpAutoTMA";
      grpAutoTMA.Size = new System.Drawing.Size(209, 101);
      grpAutoTMA.TabIndex = 1;
      grpAutoTMA.TabStop = false;
      grpAutoTMA.Text = "Auto TMA";
      // 
      // lblDash2
      // 
      lblDash2.AutoSize = true;
      lblDash2.Location = new System.Drawing.Point(121, 46);
      lblDash2.Name = "lblDash2";
      lblDash2.Size = new System.Drawing.Size(10, 13);
      lblDash2.TabIndex = 6;
      lblDash2.Text = "-";
      lblDash2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // txtMaxSpeed
      // 
      this.txtMaxSpeed.Location = new System.Drawing.Point(134, 42);
      this.txtMaxSpeed.Name = "txtMaxSpeed";
      this.txtMaxSpeed.Size = new System.Drawing.Size(64, 20);
      this.txtMaxSpeed.TabIndex = 7;
      // 
      // txtMinSpeed
      // 
      this.txtMinSpeed.Location = new System.Drawing.Point(54, 42);
      this.txtMinSpeed.Name = "txtMinSpeed";
      this.txtMinSpeed.Size = new System.Drawing.Size(64, 20);
      this.txtMinSpeed.TabIndex = 5;
      // 
      // lblAutoSpeed
      // 
      lblAutoSpeed.AutoSize = true;
      lblAutoSpeed.Location = new System.Drawing.Point(8, 46);
      lblAutoSpeed.Name = "lblAutoSpeed";
      lblAutoSpeed.Size = new System.Drawing.Size(38, 13);
      lblAutoSpeed.TabIndex = 4;
      lblAutoSpeed.Text = "&Speed";
      lblAutoSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDash1
      // 
      lblDash1.AutoSize = true;
      lblDash1.Location = new System.Drawing.Point(121, 20);
      lblDash1.Name = "lblDash1";
      lblDash1.Size = new System.Drawing.Size(10, 13);
      lblDash1.TabIndex = 2;
      lblDash1.Text = "-";
      lblDash1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // txtMaxCourse
      // 
      this.txtMaxCourse.Location = new System.Drawing.Point(134, 16);
      this.txtMaxCourse.Name = "txtMaxCourse";
      this.txtMaxCourse.Size = new System.Drawing.Size(64, 20);
      this.txtMaxCourse.TabIndex = 3;
      // 
      // txtMinCourse
      // 
      this.txtMinCourse.Location = new System.Drawing.Point(54, 16);
      this.txtMinCourse.Name = "txtMinCourse";
      this.txtMinCourse.Size = new System.Drawing.Size(64, 20);
      this.txtMinCourse.TabIndex = 1;
      // 
      // lblAutoCourse
      // 
      lblAutoCourse.AutoSize = true;
      lblAutoCourse.Location = new System.Drawing.Point(8, 20);
      lblAutoCourse.Name = "lblAutoCourse";
      lblAutoCourse.Size = new System.Drawing.Size(40, 13);
      lblAutoCourse.TabIndex = 0;
      lblAutoCourse.Text = "&Course";
      lblAutoCourse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpManualTMA
      // 
      grpManualTMA.Controls.Add(btnOptimize);
      grpManualTMA.Controls.Add(this.chkLockSpeed);
      grpManualTMA.Controls.Add(this.chkLockCourse);
      grpManualTMA.Controls.Add(btnApply);
      grpManualTMA.Controls.Add(this.txtSpeed);
      grpManualTMA.Controls.Add(lblSpeed);
      grpManualTMA.Controls.Add(this.txtCourse);
      grpManualTMA.Controls.Add(lblCourse);
      grpManualTMA.Location = new System.Drawing.Point(9, 10);
      grpManualTMA.Name = "grpManualTMA";
      grpManualTMA.Size = new System.Drawing.Size(209, 101);
      grpManualTMA.TabIndex = 0;
      grpManualTMA.TabStop = false;
      grpManualTMA.Text = "Manual TMA";
      // 
      // btnOptimize
      // 
      btnOptimize.Location = new System.Drawing.Point(10, 70);
      btnOptimize.Name = "btnOptimize";
      btnOptimize.Size = new System.Drawing.Size(77, 23);
      btnOptimize.TabIndex = 6;
      btnOptimize.Text = "&Optimize";
      btnOptimize.UseVisualStyleBackColor = true;
      btnOptimize.Click += new System.EventHandler(this.btnOptimize_Click);
      // 
      // chkLockSpeed
      // 
      this.chkLockSpeed.AutoSize = true;
      this.chkLockSpeed.Location = new System.Drawing.Point(181, 45);
      this.chkLockSpeed.Name = "chkLockSpeed";
      this.chkLockSpeed.Size = new System.Drawing.Size(15, 14);
      this.chkLockSpeed.TabIndex = 5;
      this.chkLockSpeed.UseVisualStyleBackColor = true;
      // 
      // chkLockCourse
      // 
      this.chkLockCourse.AutoSize = true;
      this.chkLockCourse.Location = new System.Drawing.Point(181, 19);
      this.chkLockCourse.Name = "chkLockCourse";
      this.chkLockCourse.Size = new System.Drawing.Size(15, 14);
      this.chkLockCourse.TabIndex = 2;
      this.chkLockCourse.UseVisualStyleBackColor = true;
      // 
      // btnApply
      // 
      btnApply.Location = new System.Drawing.Point(93, 70);
      btnApply.Name = "btnApply";
      btnApply.Size = new System.Drawing.Size(77, 23);
      btnApply.TabIndex = 7;
      btnApply.Text = "&Apply";
      btnApply.UseVisualStyleBackColor = true;
      btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // txtSpeed
      // 
      this.txtSpeed.Location = new System.Drawing.Point(55, 42);
      this.txtSpeed.Name = "txtSpeed";
      this.txtSpeed.Size = new System.Drawing.Size(115, 20);
      this.txtSpeed.TabIndex = 4;
      this.txtSpeed.Validating += new System.ComponentModel.CancelEventHandler(this.txtSpeed_Validating);
      // 
      // lblSpeed
      // 
      lblSpeed.AutoSize = true;
      lblSpeed.Location = new System.Drawing.Point(8, 45);
      lblSpeed.Name = "lblSpeed";
      lblSpeed.Size = new System.Drawing.Size(38, 13);
      lblSpeed.TabIndex = 3;
      lblSpeed.Text = "&Speed";
      lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtCourse
      // 
      this.txtCourse.Location = new System.Drawing.Point(55, 16);
      this.txtCourse.Name = "txtCourse";
      this.txtCourse.Size = new System.Drawing.Size(115, 20);
      this.txtCourse.TabIndex = 1;
      this.txtCourse.Validating += new System.ComponentModel.CancelEventHandler(this.txtCourse_Validating);
      // 
      // lblCourse
      // 
      lblCourse.AutoSize = true;
      lblCourse.Location = new System.Drawing.Point(8, 20);
      lblCourse.Name = "lblCourse";
      lblCourse.Size = new System.Drawing.Size(40, 13);
      lblCourse.TabIndex = 0;
      lblCourse.Text = "&Course";
      lblCourse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // TMAForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(226, 227);
      this.Controls.Add(grpManualTMA);
      this.Controls.Add(grpAutoTMA);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "TMAForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Target Motion Analysis";
      grpAutoTMA.ResumeLayout(false);
      grpAutoTMA.PerformLayout();
      grpManualTMA.ResumeLayout(false);
      grpManualTMA.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TextBox txtMaxSpeed;
    private System.Windows.Forms.TextBox txtMinSpeed;
    private System.Windows.Forms.TextBox txtMaxCourse;
    private System.Windows.Forms.TextBox txtMinCourse;
    private System.Windows.Forms.CheckBox chkLockSpeed;
    private System.Windows.Forms.CheckBox chkLockCourse;
    private ChangeTrackingTextBox txtSpeed;
    private ChangeTrackingTextBox txtCourse;


  }
}