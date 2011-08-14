namespace Maneubo
{
  partial class BackgroundScaleForm
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
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.lblPixels = new System.Windows.Forms.Label();
      this.txtLength = new Maneubo.ChangeTrackingTextBox();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(15, 59);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 2;
      btnOK.Text = "OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(96, 59);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblPixels
      // 
      this.lblPixels.AutoSize = true;
      this.lblPixels.Location = new System.Drawing.Point(13, 9);
      this.lblPixels.Name = "lblPixels";
      this.lblPixels.Size = new System.Drawing.Size(125, 13);
      this.lblPixels.TabIndex = 0;
      this.lblPixels.Text = "XXXX pixels represents...";
      // 
      // txtLength
      // 
      this.txtLength.Location = new System.Drawing.Point(16, 30);
      this.txtLength.Name = "txtLength";
      this.txtLength.Size = new System.Drawing.Size(155, 20);
      this.txtLength.TabIndex = 1;
      this.txtLength.WasChanged = false;
      // 
      // BackgroundScaleForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(183, 94);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.txtLength);
      this.Controls.Add(this.lblPixels);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "BackgroundScaleForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Set Background Scale";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblPixels;
    private Maneubo.ChangeTrackingTextBox txtLength;
  }
}