namespace Maneubo
{
  partial class AdvanceTimeForm
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
      System.Windows.Forms.Label lblTime;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.txtTime = new System.Windows.Forms.TextBox();
      this.chkWaypoints = new System.Windows.Forms.CheckBox();
      lblTime = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblTime
      // 
      lblTime.Location = new System.Drawing.Point(8, 9);
      lblTime.Name = "lblTime";
      lblTime.Size = new System.Drawing.Size(246, 31);
      lblTime.TabIndex = 0;
      lblTime.Text = "Enter the amount of &time to advance. Negative times are valid.";
      // 
      // btnOK
      // 
      btnOK.Location = new System.Drawing.Point(11, 71);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 3;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(92, 71);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 4;
      btnCancel.Text = "&Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // txtTime
      // 
      this.txtTime.Location = new System.Drawing.Point(11, 43);
      this.txtTime.Name = "txtTime";
      this.txtTime.Size = new System.Drawing.Size(71, 20);
      this.txtTime.TabIndex = 1;
      // 
      // chkWaypoints
      // 
      this.chkWaypoints.AutoSize = true;
      this.chkWaypoints.Checked = true;
      this.chkWaypoints.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkWaypoints.Location = new System.Drawing.Point(88, 45);
      this.chkWaypoints.Name = "chkWaypoints";
      this.chkWaypoints.Size = new System.Drawing.Size(166, 17);
      this.chkWaypoints.TabIndex = 2;
      this.chkWaypoints.Text = "Advance units with &waypoints";
      this.chkWaypoints.UseVisualStyleBackColor = true;
      // 
      // AdvanceTimeForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(256, 101);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.chkWaypoints);
      this.Controls.Add(this.txtTime);
      this.Controls.Add(lblTime);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AdvanceTimeForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Advance Time";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtTime;
    private System.Windows.Forms.CheckBox chkWaypoints;
  }
}