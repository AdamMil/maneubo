using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Maneubo
{
  class DataForm : Form
  {
    protected static void ShowInvalidDirection(string text)
    {
      MessageBox.Show(text + " is not a valid number of degrees.", "Invalid direction", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static void ShowInvalidLength(string text)
    {
      MessageBox.Show(text + " is not a valid length. Enter a number followed by a unit such as ft, km, kyd, m, mi, nm, " +
                      "nmi, or yd.", "Invalid size", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static void ShowInvalidSpeed(string text)
    {
      MessageBox.Show(text + " is not a valid speed. Enter a number followed by a unit such as kn, kph, kt, kts, m/s, or "+
                      "mph.", "Invalid speed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static void ShowRequiredMessage(string name)
    {
      MessageBox.Show(name + " is required.", name + " is required", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static bool TryParseLength(string text, out double meters)
    {
      Match m = lengthRe.Match(text);
      if(!m.Success || !double.TryParse(m.Groups["number"].Value, out meters))
      {
        meters = 0;
        return false;
      }
      else
      {
        LengthUnit unit = LengthUnit.Meter;
        switch(m.Groups["unit"].Value.ToLowerInvariant())
        {
          case "ft": unit = LengthUnit.Foot; break;
          case "km": unit = LengthUnit.Kilometer; break;
          case "kyd": unit = LengthUnit.Kiloyard; break;
          case "mi": unit = LengthUnit.Mile; break;
          case "nm": case "nmi": unit = LengthUnit.NauticalMile; break;
          case "yd": unit = LengthUnit.Yard; break;
        }
        meters = ManeuveringBoard.ConvertFromUnit(meters, unit);
        return true;
      }
    }

    protected static bool TryParseSpeed(string text, UnitSystem unitSystem, out double metersPerSecond)
    {
      Match m = speedRe.Match(text);
      if(!m.Success || !double.TryParse(m.Groups["number"].Value, out metersPerSecond))
      {
        metersPerSecond = 0;
        return false;
      }
      else
      {
        SpeedUnit unit;
        switch(m.Groups["unit"].Value.ToLowerInvariant())
        {
          case "kn": case "kt": case "kts": unit = SpeedUnit.Knots; break;
          case "kph": unit = SpeedUnit.KilometersPerHour; break;
          case "mph": unit = SpeedUnit.MilesPerHour; break;
          case "m/s": unit = SpeedUnit.MetersPerSecond; break;
          default:
            switch(unitSystem)
            {
              case UnitSystem.Imperial: unit = SpeedUnit.MilesPerHour; break;
              case UnitSystem.Metric: unit = SpeedUnit.KilometersPerHour; break;
              case UnitSystem.NauticalImperial:
              case UnitSystem.NauticalMetric: unit = SpeedUnit.Knots; break;
              default: unit = SpeedUnit.MetersPerSecond; break;
            }
            break;
        }
        metersPerSecond = ManeuveringBoard.ConvertFromUnit(metersPerSecond, unit);
        return true;
      }
    }

    static readonly Regex lengthRe = new Regex(@"^\s*(?<number>\d+|\d*[\.,]\d+)\s*(?<unit>ft|k(?:m|yd)|mi?|nmi?|yd)\s*$",
                                               RegexOptions.IgnoreCase);
    static readonly Regex speedRe = new Regex(@"^\s*(?<number>\d+|\d*[\.,]\d+)\s*(?<unit>k(?:n|ph|ts?)|m(?:\/s|ph))?\s*$",
                                              RegexOptions.IgnoreCase);
  }
}