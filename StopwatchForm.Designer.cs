namespace Maneubo
{
  partial class StopwatchForm
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
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.Label lblCurrentLabel;
      System.Windows.Forms.Label lblTotalLabel;
      System.Windows.Forms.Button btnSave;
      this.lblCurrent = new System.Windows.Forms.Label();
      this.lblTotal = new System.Windows.Forms.Label();
      this.btnStartStop = new System.Windows.Forms.Button();
      this.btnCopy = new System.Windows.Forms.Button();
      this.lstTimes = new System.Windows.Forms.ListBox();
      this.btnClear = new System.Windows.Forms.Button();
      this.btnReset = new System.Windows.Forms.Button();
      this.timer = new System.Windows.Forms.Timer(this.components);
      lblCurrentLabel = new System.Windows.Forms.Label();
      lblTotalLabel = new System.Windows.Forms.Label();
      btnSave = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblCurrentLabel
      // 
      lblCurrentLabel.AutoSize = true;
      lblCurrentLabel.Location = new System.Drawing.Point(10, 15);
      lblCurrentLabel.Name = "lblCurrentLabel";
      lblCurrentLabel.Size = new System.Drawing.Size(41, 13);
      lblCurrentLabel.TabIndex = 0;
      lblCurrentLabel.Text = "Current";
      lblCurrentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTotalLabel
      // 
      lblTotalLabel.AutoSize = true;
      lblTotalLabel.Location = new System.Drawing.Point(10, 34);
      lblTotalLabel.Name = "lblTotalLabel";
      lblTotalLabel.Size = new System.Drawing.Size(31, 13);
      lblTotalLabel.TabIndex = 2;
      lblTotalLabel.Text = "Total";
      lblTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnSave
      // 
      btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnSave.Location = new System.Drawing.Point(8, 129);
      btnSave.Name = "btnSave";
      btnSave.Size = new System.Drawing.Size(48, 23);
      btnSave.TabIndex = 7;
      btnSave.Text = "S&ave";
      btnSave.UseVisualStyleBackColor = true;
      btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // lblCurrent
      // 
      this.lblCurrent.AutoSize = true;
      this.lblCurrent.Location = new System.Drawing.Point(58, 15);
      this.lblCurrent.Name = "lblCurrent";
      this.lblCurrent.Size = new System.Drawing.Size(43, 13);
      this.lblCurrent.TabIndex = 1;
      this.lblCurrent.Text = "0:00:00";
      this.lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTotal
      // 
      this.lblTotal.AutoSize = true;
      this.lblTotal.Location = new System.Drawing.Point(58, 34);
      this.lblTotal.Name = "lblTotal";
      this.lblTotal.Size = new System.Drawing.Size(43, 13);
      this.lblTotal.TabIndex = 3;
      this.lblTotal.Text = "0:00:00";
      this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnStartStop
      // 
      this.btnStartStop.Location = new System.Drawing.Point(116, 7);
      this.btnStartStop.Name = "btnStartStop";
      this.btnStartStop.Size = new System.Drawing.Size(48, 23);
      this.btnStartStop.TabIndex = 4;
      this.btnStartStop.Text = "&Start";
      this.btnStartStop.UseVisualStyleBackColor = true;
      this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
      // 
      // btnCopy
      // 
      this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnCopy.Enabled = false;
      this.btnCopy.Location = new System.Drawing.Point(62, 129);
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.Size = new System.Drawing.Size(48, 23);
      this.btnCopy.TabIndex = 8;
      this.btnCopy.Text = "&Copy";
      this.btnCopy.UseVisualStyleBackColor = true;
      this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
      // 
      // lstTimes
      // 
      this.lstTimes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lstTimes.FormattingEnabled = true;
      this.lstTimes.IntegralHeight = false;
      this.lstTimes.Location = new System.Drawing.Point(8, 65);
      this.lstTimes.Name = "lstTimes";
      this.lstTimes.Size = new System.Drawing.Size(156, 58);
      this.lstTimes.TabIndex = 6;
      this.lstTimes.SelectedIndexChanged += new System.EventHandler(this.lstTimes_SelectedIndexChanged);
      this.lstTimes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstTimes_MouseDoubleClick);
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnClear.Enabled = false;
      this.btnClear.Location = new System.Drawing.Point(116, 129);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(48, 23);
      this.btnClear.TabIndex = 9;
      this.btnClear.Text = "C&lear";
      this.btnClear.UseVisualStyleBackColor = true;
      this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
      // 
      // btnReset
      // 
      this.btnReset.Location = new System.Drawing.Point(116, 36);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(48, 23);
      this.btnReset.TabIndex = 5;
      this.btnReset.Text = "&Reset";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // timer
      // 
      this.timer.Interval = 200;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // StopwatchForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(172, 158);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(btnSave);
      this.Controls.Add(this.btnClear);
      this.Controls.Add(this.lstTimes);
      this.Controls.Add(this.btnCopy);
      this.Controls.Add(this.btnStartStop);
      this.Controls.Add(this.lblTotal);
      this.Controls.Add(this.lblCurrent);
      this.Controls.Add(lblTotalLabel);
      this.Controls.Add(lblCurrentLabel);
      this.Location = new System.Drawing.Point(180, 161);
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(180, 9999);
      this.Name = "StopwatchForm";
      this.Text = "Stopwatch";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblCurrent;
    private System.Windows.Forms.Label lblTotal;
    private System.Windows.Forms.Button btnStartStop;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.ListBox lstTimes;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Timer timer;
  }
}