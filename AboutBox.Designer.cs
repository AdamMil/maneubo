namespace Maneubo
{
  partial class AboutBox
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
      System.Windows.Forms.Label lblDescription;
      System.Windows.Forms.PictureBox logoPictureBox;
      this.lblUrl = new System.Windows.Forms.LinkLabel();
      this.labelProductName = new System.Windows.Forms.Label();
      this.labelCopyright = new System.Windows.Forms.Label();
      this.okButton = new System.Windows.Forms.Button();
      lblDescription = new System.Windows.Forms.Label();
      logoPictureBox = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // lblDescription
      // 
      lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      lblDescription.Location = new System.Drawing.Point(181, 54);
      lblDescription.Name = "lblDescription";
      lblDescription.Size = new System.Drawing.Size(280, 97);
      lblDescription.TabIndex = 26;
      lblDescription.Text = "Maneubo a simple maneuvering board application, intended to be used either alone " +
    "or with a simulation program.\r\n\r\nIt is released under the terms of the GNU GPL v" +
    "ersion 2.";
      // 
      // logoPictureBox
      // 
      logoPictureBox.Image = global::Maneubo.Properties.Resources.About;
      logoPictureBox.Location = new System.Drawing.Point(12, 12);
      logoPictureBox.Name = "logoPictureBox";
      logoPictureBox.Size = new System.Drawing.Size(160, 160);
      logoPictureBox.TabIndex = 13;
      logoPictureBox.TabStop = false;
      // 
      // lblUrl
      // 
      this.lblUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblUrl.Location = new System.Drawing.Point(181, 129);
      this.lblUrl.Name = "lblUrl";
      this.lblUrl.Size = new System.Drawing.Size(197, 17);
      this.lblUrl.TabIndex = 27;
      this.lblUrl.TabStop = true;
      this.lblUrl.Text = "http://www.adammil.net/Maneubo";
      this.lblUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblUrl_LinkClicked);
      // 
      // labelProductName
      // 
      this.labelProductName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelProductName.Location = new System.Drawing.Point(181, 12);
      this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
      this.labelProductName.Name = "labelProductName";
      this.labelProductName.Size = new System.Drawing.Size(280, 17);
      this.labelProductName.TabIndex = 20;
      this.labelProductName.Text = "Product Name";
      this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelCopyright
      // 
      this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelCopyright.Location = new System.Drawing.Point(181, 29);
      this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
      this.labelCopyright.Name = "labelCopyright";
      this.labelCopyright.Size = new System.Drawing.Size(280, 17);
      this.labelCopyright.TabIndex = 22;
      this.labelCopyright.Text = "Copyright";
      this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.okButton.Location = new System.Drawing.Point(385, 154);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 25;
      this.okButton.Text = "&OK";
      // 
      // AboutBox
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.okButton;
      this.ClientSize = new System.Drawing.Size(467, 185);
      this.Controls.Add(this.lblUrl);
      this.Controls.Add(lblDescription);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.labelCopyright);
      this.Controls.Add(this.labelProductName);
      this.Controls.Add(logoPictureBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutBox";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label labelProductName;
    private System.Windows.Forms.Label labelCopyright;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.LinkLabel lblUrl;
  }
}
