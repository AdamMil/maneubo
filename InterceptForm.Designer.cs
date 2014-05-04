namespace Maneubo
{
  partial class InterceptForm
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
      System.Windows.Forms.Label lblSpeed;
      System.Windows.Forms.Label lblRadius;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Label lblTime;
      System.Windows.Forms.Label lblAoB;
      System.Windows.Forms.Label lblBearing;
      System.Windows.Forms.Label lblRange;
      System.Windows.Forms.Label lblTargetSpeed;
      System.Windows.Forms.Label lblCourse;
      this.txtSpeed = new System.Windows.Forms.TextBox();
      this.txtRadius = new System.Windows.Forms.TextBox();
      this.txtTime = new System.Windows.Forms.TextBox();
      this.radVector = new System.Windows.Forms.RadioButton();
      this.radWaypoint = new System.Windows.Forms.RadioButton();
      this.lblSolution = new System.Windows.Forms.Label();
      this.txtAoB = new System.Windows.Forms.TextBox();
      this.txtBearing = new System.Windows.Forms.TextBox();
      this.txtRange = new System.Windows.Forms.TextBox();
      this.txtTargetSpeed = new System.Windows.Forms.TextBox();
      this.txtCourse = new System.Windows.Forms.TextBox();
      lblSpeed = new System.Windows.Forms.Label();
      lblRadius = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      lblTime = new System.Windows.Forms.Label();
      lblAoB = new System.Windows.Forms.Label();
      lblBearing = new System.Windows.Forms.Label();
      lblRange = new System.Windows.Forms.Label();
      lblTargetSpeed = new System.Windows.Forms.Label();
      lblCourse = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblSpeed
      // 
      lblSpeed.AutoSize = true;
      lblSpeed.Location = new System.Drawing.Point(183, 11);
      lblSpeed.Name = "lblSpeed";
      lblSpeed.Size = new System.Drawing.Size(81, 13);
      lblSpeed.TabIndex = 8;
      lblSpeed.Text = "Intercept &speed";
      lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblRadius
      // 
      lblRadius.AutoSize = true;
      lblRadius.Location = new System.Drawing.Point(183, 89);
      lblRadius.Name = "lblRadius";
      lblRadius.Size = new System.Drawing.Size(80, 13);
      lblRadius.TabIndex = 14;
      lblRadius.Text = "Intercept &radius";
      lblRadius.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnOK.Location = new System.Drawing.Point(9, 135);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 19;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(90, 135);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 20;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblTime
      // 
      lblTime.AutoSize = true;
      lblTime.Location = new System.Drawing.Point(183, 37);
      lblTime.Name = "lblTime";
      lblTime.Size = new System.Drawing.Size(71, 13);
      lblTime.TabIndex = 10;
      lblTime.Text = "Intercept t&ime";
      lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblAoB
      // 
      lblAoB.AutoSize = true;
      lblAoB.Location = new System.Drawing.Point(183, 63);
      lblAoB.Name = "lblAoB";
      lblAoB.Size = new System.Drawing.Size(72, 13);
      lblAoB.TabIndex = 12;
      lblAoB.Text = "Intercept &AoB";
      lblAoB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblBearing
      // 
      lblBearing.AutoSize = true;
      lblBearing.Location = new System.Drawing.Point(8, 11);
      lblBearing.Name = "lblBearing";
      lblBearing.Size = new System.Drawing.Size(76, 13);
      lblBearing.TabIndex = 0;
      lblBearing.Text = "Target &bearing";
      lblBearing.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblRange
      // 
      lblRange.AutoSize = true;
      lblRange.Location = new System.Drawing.Point(8, 37);
      lblRange.Name = "lblRange";
      lblRange.Size = new System.Drawing.Size(81, 13);
      lblRange.TabIndex = 2;
      lblRange.Text = "Target &distance";
      lblRange.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTargetSpeed
      // 
      lblTargetSpeed.AutoSize = true;
      lblTargetSpeed.Location = new System.Drawing.Point(8, 89);
      lblTargetSpeed.Name = "lblTargetSpeed";
      lblTargetSpeed.Size = new System.Drawing.Size(70, 13);
      lblTargetSpeed.TabIndex = 6;
      lblTargetSpeed.Text = "Target s&peed";
      lblTargetSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblCourse
      // 
      lblCourse.AutoSize = true;
      lblCourse.Location = new System.Drawing.Point(8, 63);
      lblCourse.Name = "lblCourse";
      lblCourse.Size = new System.Drawing.Size(73, 13);
      lblCourse.TabIndex = 4;
      lblCourse.Text = "Target &course";
      lblCourse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtSpeed
      // 
      this.txtSpeed.Location = new System.Drawing.Point(272, 7);
      this.txtSpeed.Name = "txtSpeed";
      this.txtSpeed.Size = new System.Drawing.Size(69, 20);
      this.txtSpeed.TabIndex = 9;
      this.txtSpeed.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtSpeed.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtRadius
      // 
      this.txtRadius.Location = new System.Drawing.Point(272, 85);
      this.txtRadius.Name = "txtRadius";
      this.txtRadius.Size = new System.Drawing.Size(69, 20);
      this.txtRadius.TabIndex = 15;
      this.txtRadius.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtRadius.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtTime
      // 
      this.txtTime.Location = new System.Drawing.Point(272, 33);
      this.txtTime.Name = "txtTime";
      this.txtTime.Size = new System.Drawing.Size(69, 20);
      this.txtTime.TabIndex = 11;
      this.txtTime.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtTime.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // radVector
      // 
      this.radVector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.radVector.AutoSize = true;
      this.radVector.Checked = true;
      this.radVector.Location = new System.Drawing.Point(180, 138);
      this.radVector.Name = "radVector";
      this.radVector.Size = new System.Drawing.Size(74, 17);
      this.radVector.TabIndex = 17;
      this.radVector.TabStop = true;
      this.radVector.Text = "Set &vector";
      this.radVector.UseVisualStyleBackColor = true;
      // 
      // radWaypoint
      // 
      this.radWaypoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.radWaypoint.AutoSize = true;
      this.radWaypoint.Location = new System.Drawing.Point(256, 138);
      this.radWaypoint.Name = "radWaypoint";
      this.radWaypoint.Size = new System.Drawing.Size(86, 17);
      this.radWaypoint.TabIndex = 18;
      this.radWaypoint.Text = "Set &waypoint";
      this.radWaypoint.UseVisualStyleBackColor = true;
      // 
      // lblSolution
      // 
      this.lblSolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblSolution.Location = new System.Drawing.Point(11, 112);
      this.lblSolution.Name = "lblSolution";
      this.lblSolution.Size = new System.Drawing.Size(333, 16);
      this.lblSolution.TabIndex = 16;
      this.lblSolution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtAoB
      // 
      this.txtAoB.Location = new System.Drawing.Point(272, 59);
      this.txtAoB.Name = "txtAoB";
      this.txtAoB.Size = new System.Drawing.Size(69, 20);
      this.txtAoB.TabIndex = 13;
      this.txtAoB.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtAoB.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtBearing
      // 
      this.txtBearing.Location = new System.Drawing.Point(97, 7);
      this.txtBearing.Name = "txtBearing";
      this.txtBearing.Size = new System.Drawing.Size(69, 20);
      this.txtBearing.TabIndex = 1;
      this.txtBearing.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtBearing.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtRange
      // 
      this.txtRange.Location = new System.Drawing.Point(97, 33);
      this.txtRange.Name = "txtRange";
      this.txtRange.Size = new System.Drawing.Size(69, 20);
      this.txtRange.TabIndex = 3;
      this.txtRange.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtRange.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtTargetSpeed
      // 
      this.txtTargetSpeed.Location = new System.Drawing.Point(97, 85);
      this.txtTargetSpeed.Name = "txtTargetSpeed";
      this.txtTargetSpeed.Size = new System.Drawing.Size(69, 20);
      this.txtTargetSpeed.TabIndex = 7;
      this.txtTargetSpeed.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtTargetSpeed.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtCourse
      // 
      this.txtCourse.Location = new System.Drawing.Point(97, 59);
      this.txtCourse.Name = "txtCourse";
      this.txtCourse.Size = new System.Drawing.Size(69, 20);
      this.txtCourse.TabIndex = 5;
      this.txtCourse.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtCourse.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // InterceptForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(353, 166);
      this.Controls.Add(this.txtTargetSpeed);
      this.Controls.Add(lblTargetSpeed);
      this.Controls.Add(this.txtCourse);
      this.Controls.Add(lblCourse);
      this.Controls.Add(this.txtBearing);
      this.Controls.Add(lblBearing);
      this.Controls.Add(this.txtRange);
      this.Controls.Add(lblRange);
      this.Controls.Add(this.txtAoB);
      this.Controls.Add(lblAoB);
      this.Controls.Add(this.lblSolution);
      this.Controls.Add(this.radWaypoint);
      this.Controls.Add(this.radVector);
      this.Controls.Add(this.txtTime);
      this.Controls.Add(lblTime);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.txtRadius);
      this.Controls.Add(lblRadius);
      this.Controls.Add(this.txtSpeed);
      this.Controls.Add(lblSpeed);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InterceptForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Calculate Intercept";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtSpeed;
    private System.Windows.Forms.TextBox txtRadius;
    private System.Windows.Forms.TextBox txtTime;
    private System.Windows.Forms.RadioButton radVector;
    private System.Windows.Forms.RadioButton radWaypoint;
    private System.Windows.Forms.Label lblSolution;
    private System.Windows.Forms.TextBox txtAoB;
    private System.Windows.Forms.TextBox txtBearing;
    private System.Windows.Forms.TextBox txtRange;
    private System.Windows.Forms.TextBox txtTargetSpeed;
    private System.Windows.Forms.TextBox txtCourse;
  }
}