using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Maneubo
{
  partial class MapProjectionForm : Form
  {
    public MapProjectionForm()
    {
      InitializeComponent();
      cmbProjection.SelectedIndex = 0;
    }

    public MapProjectionType ProjectionType
    {
      get { return (MapProjectionType)cmbProjection.SelectedIndex; }
    }

    public double Latitude
    {
      get { return ParseLatitude(txtLatitude.Text); }
      set { txtLatitude.Text = ManeuveringBoard.GetLatitudeString(value); }
    }

    public double Longitude
    {
      get { return ParseLongitude(txtLongitude.Text); }
      set { txtLongitude.Text = ManeuveringBoard.GetLongitudeString(value); }
    }

    void btnOK_Click(object sender, EventArgs e)
    {
      if(Validate("latitude", latRe, txtLatitude, 'N') && Validate("longitude", lonRe, txtLongitude, 'W')) DialogResult = DialogResult.OK;
    }

    static double ParseLatitude(string text)
    {
      double angle;
      TryParse(latRe, text, 'N', out angle);
      return angle;
    }

    static double ParseLongitude(string text)
    {
      double angle;
      TryParse(lonRe, text, 'W', out angle);
      return angle;
    }

    static bool TryParse(Regex regex, string text, char negativeChar, out double angle)
    {
      Match m = regex.Match(text);
      if(!m.Success)
      {
        angle = 0;
        return false;
      }
      else
      {
        angle = double.Parse(m.Groups["degree"].Value);
        bool negative = angle < 0;
        if(negative) angle = -angle; // get the absolute value so that when we add minutes and seconds it increases the magnitude
        if(m.Groups["minute"].Success) angle += double.Parse(m.Groups["minute"].Value) / 60;
        if(m.Groups["second"].Success) angle += double.Parse(m.Groups["second"].Value) / 3600;
        if(m.Groups["dir"].Success && char.ToUpperInvariant(m.Groups["dir"].Value[0]) == negativeChar) negative = !negative;
        if(negative) angle = -angle;
        angle = angle/180 * Math.PI;
        return true;
      }
    }

    static bool Validate(string name, Regex regex, TextBox textBox, char negativeChar)
    {
      double angle;
      if(!TryParse(regex, textBox.Text, negativeChar, out angle))
      {
        if(string.IsNullOrEmpty(textBox.Text.Trim()))
        {
          MessageBox.Show("You must specify a longitude and latitude.", "Value required.", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
          MessageBox.Show("Invalid " + name + ": " + textBox.Text, "Invalid " + name + ".", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
      }

      return true;
    }

    const string lonLatReStart = 
      @"^\s*(?<degree>-?(?:\d+(?:[\.,]\d+)?|[\.,]\d+))\s*[\-°]?\s*
        (?:(?<minute>(?:\d+(?:[\.,]\d+)?|[\.,]\d+))\s*[′']?\s*
           (?:(?<second>(?:\d+(?:[\.,]\d+)?|[\.,]\d+))\s*[""″]\s*)?)?
        (?<dir>[";
    const string lonLatReEnd = @"])?\s*$";
    static Regex latRe = new Regex(lonLatReStart + "NSns" + lonLatReEnd, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
    static Regex lonRe = new Regex(lonLatReStart + "EWew" + lonLatReEnd, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
  }
}
