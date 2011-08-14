using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Maneubo
{
  public partial class BackgroundImageForm : Form
  {
    public BackgroundImageForm()
    {
      InitializeComponent();

      if(Clipboard.ContainsImage()) radClipboard.Checked = true;
      else radFile.Checked = true;
    }

    public Image Image { get; private set; }

    void btnBrowse_Click(object sender, System.EventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.FileName = txtFile.Text;
      ofd.Filter   = "Image Files (*.bmp; *.gif; *.jpg; *.jpeg; *.png; *.tif; *.tiff)|*.bmp; *.gif; *.jpg; *.jpeg; *.png; *.tif; *.tiff|"+
                     "All Files (*.*)|*.*";
      ofd.Title    = "Select an image file to load.";
      if(!string.IsNullOrEmpty(txtFile.Text.Trim()))
      {
        try
        {
          string directory = Path.GetDirectoryName(txtFile.Text.Trim());
          if(Directory.Exists(directory)) ofd.InitialDirectory = directory;
        }
        catch { }
      }

      if(ofd.ShowDialog() == DialogResult.OK)
      {
        txtFile.Text = ofd.FileName;
        radFile.Checked = true;
      }
    }

    void btnOK_Click(object sender, System.EventArgs e)
    {
      if(radClipboard.Checked)
      {
        try
        {
          if(!Clipboard.ContainsImage())
          {
            MessageBox.Show("The clipboard does not contain a usable image.", "No clipboard image", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return;
          }

          Image = Clipboard.GetImage();
          if(Image == null) throw new Exception("Unable to read image from clipboard.");
        }
        catch(Exception ex)
        {
          MessageBox.Show("An error occurred while reading data from the clipboard. (" + ex.Message + ") Try copying the data into the " +
                          "clipboard again.", "Error reading clipboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
      }
      else
      {
        string file = txtFile.Text.Trim();
        if(string.IsNullOrEmpty(file))
        {
          MessageBox.Show("You must select a file.", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if(!File.Exists(file))
        {
          MessageBox.Show(file + " does not exist or cannot be accessed.", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        try
        {
          Image = Image.FromFile(file);
        }
        catch(Exception ex)
        {
          MessageBox.Show("An error occurred while reading " + file + ". (" + ex.Message + ")", "Error reading file", MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
          return;
        }
      }

      DialogResult = DialogResult.OK;
    }
  }
}
