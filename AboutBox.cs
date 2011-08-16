using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;

namespace Maneubo
{
  partial class AboutBox : Form
  {
    public AboutBox()
    {
      InitializeComponent();
      this.Text = String.Format("About {0}", AssemblyTitle);
      this.labelProductName.Text = AssemblyTitle + ", version " + AssemblyVersion;
      this.labelCopyright.Text = AssemblyCopyright;
    }

    #region Assembly Attribute Accessors
    string AssemblyCopyright
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
      }
    }

    string AssemblyTitle
    {
      get
      {
        object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if(attributes.Length != 0)
        {
          AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
          if(!string.IsNullOrEmpty(titleAttribute.Title)) return titleAttribute.Title;
        }
        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    string AssemblyVersion
    {
      get
      {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
      }
    }
    #endregion

    void lblUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(lblUrl.Text);
    }
  }
}
