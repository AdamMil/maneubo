namespace Maneubo
{
  partial class BackgroundImageForm
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
      System.Windows.Forms.Label lblTakeBackground;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.radClipboard = new System.Windows.Forms.RadioButton();
      this.radFile = new System.Windows.Forms.RadioButton();
      this.txtFile = new System.Windows.Forms.TextBox();
      this.btnBrowse = new System.Windows.Forms.Button();
      lblTakeBackground = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblTakeBackground
      // 
      lblTakeBackground.AutoSize = true;
      lblTakeBackground.Location = new System.Drawing.Point(13, 11);
      lblTakeBackground.Name = "lblTakeBackground";
      lblTakeBackground.Size = new System.Drawing.Size(132, 13);
      lblTakeBackground.TabIndex = 0;
      lblTakeBackground.Text = "Take background image...";
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(124, 107);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 5;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(205, 107);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 6;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // radClipboard
      // 
      this.radClipboard.AutoSize = true;
      this.radClipboard.Location = new System.Drawing.Point(16, 28);
      this.radClipboard.Name = "radClipboard";
      this.radClipboard.Size = new System.Drawing.Size(112, 17);
      this.radClipboard.TabIndex = 1;
      this.radClipboard.TabStop = true;
      this.radClipboard.Text = "From the &clipboard";
      this.radClipboard.UseVisualStyleBackColor = true;
      // 
      // radFile
      // 
      this.radFile.AutoSize = true;
      this.radFile.Location = new System.Drawing.Point(16, 51);
      this.radFile.Name = "radFile";
      this.radFile.Size = new System.Drawing.Size(73, 17);
      this.radFile.TabIndex = 2;
      this.radFile.TabStop = true;
      this.radFile.Text = "From a &file";
      this.radFile.UseVisualStyleBackColor = true;
      // 
      // txtFile
      // 
      this.txtFile.Location = new System.Drawing.Point(36, 71);
      this.txtFile.Name = "txtFile";
      this.txtFile.Size = new System.Drawing.Size(163, 20);
      this.txtFile.TabIndex = 3;
      // 
      // btnBrowse
      // 
      this.btnBrowse.Location = new System.Drawing.Point(205, 70);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(75, 23);
      this.btnBrowse.TabIndex = 4;
      this.btnBrowse.Text = "&Browse";
      this.btnBrowse.UseVisualStyleBackColor = true;
      this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
      // 
      // BackgroundImageForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(292, 142);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.btnBrowse);
      this.Controls.Add(this.txtFile);
      this.Controls.Add(this.radFile);
      this.Controls.Add(this.radClipboard);
      this.Controls.Add(lblTakeBackground);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "BackgroundImageForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Set Background Image";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton radClipboard;
    private System.Windows.Forms.RadioButton radFile;
    private System.Windows.Forms.TextBox txtFile;
    private System.Windows.Forms.Button btnBrowse;
  }
}