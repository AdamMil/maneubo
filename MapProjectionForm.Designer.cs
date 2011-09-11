namespace Maneubo
{
  partial class MapProjectionForm
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
      System.Windows.Forms.Label lblProjection;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Label lblLatitude;
      System.Windows.Forms.Label lblLongitude;
      this.cmbProjection = new System.Windows.Forms.ComboBox();
      this.txtLatitude = new System.Windows.Forms.TextBox();
      this.txtLongitude = new System.Windows.Forms.TextBox();
      lblProjection = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      lblLatitude = new System.Windows.Forms.Label();
      lblLongitude = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblProjection
      // 
      lblProjection.AutoSize = true;
      lblProjection.Location = new System.Drawing.Point(12, 11);
      lblProjection.Name = "lblProjection";
      lblProjection.Size = new System.Drawing.Size(54, 13);
      lblProjection.TabIndex = 0;
      lblProjection.Text = "&Projection";
      lblProjection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnOK.Location = new System.Drawing.Point(61, 91);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 6;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(142, 91);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 7;
      btnCancel.Text = "&Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblLatitude
      // 
      lblLatitude.AutoSize = true;
      lblLatitude.Location = new System.Drawing.Point(12, 38);
      lblLatitude.Name = "lblLatitude";
      lblLatitude.Size = new System.Drawing.Size(45, 13);
      lblLatitude.TabIndex = 2;
      lblLatitude.Text = "L&atitude";
      lblLatitude.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblLongitude
      // 
      lblLongitude.AutoSize = true;
      lblLongitude.Location = new System.Drawing.Point(12, 64);
      lblLongitude.Name = "lblLongitude";
      lblLongitude.Size = new System.Drawing.Size(54, 13);
      lblLongitude.TabIndex = 4;
      lblLongitude.Text = "&Longitude";
      lblLongitude.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cmbProjection
      // 
      this.cmbProjection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbProjection.FormattingEnabled = true;
      this.cmbProjection.Items.AddRange(new object[] {
            "Azimuthal Equidistant"});
      this.cmbProjection.Location = new System.Drawing.Point(72, 7);
      this.cmbProjection.Name = "cmbProjection";
      this.cmbProjection.Size = new System.Drawing.Size(144, 21);
      this.cmbProjection.TabIndex = 1;
      // 
      // txtLatitude
      // 
      this.txtLatitude.Location = new System.Drawing.Point(72, 34);
      this.txtLatitude.Name = "txtLatitude";
      this.txtLatitude.Size = new System.Drawing.Size(145, 20);
      this.txtLatitude.TabIndex = 3;
      // 
      // txtLongitude
      // 
      this.txtLongitude.Location = new System.Drawing.Point(72, 60);
      this.txtLongitude.Name = "txtLongitude";
      this.txtLongitude.Size = new System.Drawing.Size(145, 20);
      this.txtLongitude.TabIndex = 5;
      // 
      // MapProjectionForm
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(229, 122);
      this.Controls.Add(this.txtLongitude);
      this.Controls.Add(lblLongitude);
      this.Controls.Add(this.txtLatitude);
      this.Controls.Add(lblLatitude);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.cmbProjection);
      this.Controls.Add(lblProjection);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "MapProjectionForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Map Projection";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cmbProjection;
    private System.Windows.Forms.TextBox txtLatitude;
    private System.Windows.Forms.TextBox txtLongitude;
  }
}