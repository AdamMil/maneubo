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
      this.txtSpeed = new System.Windows.Forms.TextBox();
      this.txtRadius = new System.Windows.Forms.TextBox();
      this.txtTime = new System.Windows.Forms.TextBox();
      this.radVector = new System.Windows.Forms.RadioButton();
      this.radWaypoint = new System.Windows.Forms.RadioButton();
      this.lblSolution = new System.Windows.Forms.Label();
      this.txtAoB = new System.Windows.Forms.TextBox();
      lblSpeed = new System.Windows.Forms.Label();
      lblRadius = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      lblTime = new System.Windows.Forms.Label();
      lblAoB = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblSpeed
      // 
      lblSpeed.AutoSize = true;
      lblSpeed.Location = new System.Drawing.Point(11, 14);
      lblSpeed.Name = "lblSpeed";
      lblSpeed.Size = new System.Drawing.Size(81, 13);
      lblSpeed.TabIndex = 0;
      lblSpeed.Text = "Intercept &speed";
      lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblRadius
      // 
      lblRadius.AutoSize = true;
      lblRadius.Location = new System.Drawing.Point(11, 92);
      lblRadius.Name = "lblRadius";
      lblRadius.Size = new System.Drawing.Size(80, 13);
      lblRadius.TabIndex = 6;
      lblRadius.Text = "Intercept &radius";
      lblRadius.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(14, 164);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 11;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(95, 164);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 12;
      btnCancel.Text = "&Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblTime
      // 
      lblTime.AutoSize = true;
      lblTime.Location = new System.Drawing.Point(11, 40);
      lblTime.Name = "lblTime";
      lblTime.Size = new System.Drawing.Size(71, 13);
      lblTime.TabIndex = 2;
      lblTime.Text = "Intercept &time";
      lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblAoB
      // 
      lblAoB.AutoSize = true;
      lblAoB.Location = new System.Drawing.Point(11, 66);
      lblAoB.Name = "lblAoB";
      lblAoB.Size = new System.Drawing.Size(72, 13);
      lblAoB.TabIndex = 4;
      lblAoB.Text = "Intercept &AoB";
      lblAoB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtSpeed
      // 
      this.txtSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtSpeed.Location = new System.Drawing.Point(100, 10);
      this.txtSpeed.Name = "txtSpeed";
      this.txtSpeed.Size = new System.Drawing.Size(69, 20);
      this.txtSpeed.TabIndex = 1;
      this.txtSpeed.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtSpeed.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtRadius
      // 
      this.txtRadius.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtRadius.Location = new System.Drawing.Point(100, 88);
      this.txtRadius.Name = "txtRadius";
      this.txtRadius.Size = new System.Drawing.Size(69, 20);
      this.txtRadius.TabIndex = 7;
      this.txtRadius.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtRadius.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // txtTime
      // 
      this.txtTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTime.Location = new System.Drawing.Point(100, 36);
      this.txtTime.Name = "txtTime";
      this.txtTime.Size = new System.Drawing.Size(69, 20);
      this.txtTime.TabIndex = 3;
      this.txtTime.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtTime.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // radVector
      // 
      this.radVector.AutoSize = true;
      this.radVector.Checked = true;
      this.radVector.Location = new System.Drawing.Point(11, 141);
      this.radVector.Name = "radVector";
      this.radVector.Size = new System.Drawing.Size(74, 17);
      this.radVector.TabIndex = 9;
      this.radVector.TabStop = true;
      this.radVector.Text = "Set &vector";
      this.radVector.UseVisualStyleBackColor = true;
      // 
      // radWaypoint
      // 
      this.radWaypoint.AutoSize = true;
      this.radWaypoint.Location = new System.Drawing.Point(87, 141);
      this.radWaypoint.Name = "radWaypoint";
      this.radWaypoint.Size = new System.Drawing.Size(86, 17);
      this.radWaypoint.TabIndex = 10;
      this.radWaypoint.Text = "Set &waypoint";
      this.radWaypoint.UseVisualStyleBackColor = true;
      // 
      // lblSolution
      // 
      this.lblSolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblSolution.Location = new System.Drawing.Point(11, 116);
      this.lblSolution.Name = "lblSolution";
      this.lblSolution.Size = new System.Drawing.Size(162, 16);
      this.lblSolution.TabIndex = 8;
      this.lblSolution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtAoB
      // 
      this.txtAoB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtAoB.Location = new System.Drawing.Point(100, 62);
      this.txtAoB.Name = "txtAoB";
      this.txtAoB.Size = new System.Drawing.Size(69, 20);
      this.txtAoB.TabIndex = 5;
      this.txtAoB.TextChanged += new System.EventHandler(this.txt_TextChanged);
      this.txtAoB.Leave += new System.EventHandler(this.txt_Leave);
      // 
      // InterceptForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(182, 195);
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
  }
}