using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AdamMil.Mathematics.Geometry;

namespace Maneubo
{
  class DataForm : Form
  {
    protected static void ShowInvalidAngle(string text)
    {
      MessageBox.Show(text + " is not a valid angle.", "Invalid angle", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    protected static void ShowInvalidTime(string text, bool allowRelative)
    {
      string message = text + " is not a valid time. You may specify a time as [hh:]mm[:ss].";
      if(allowRelative)
      {
        message += "If there was a previous time, you may prepend a + sign to indicate that the time should be interpreted relative to " +
                   "the previous one.";
      }
      MessageBox.Show(message, "Invalid time", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static void ShowRequiredMessage(string name)
    {
      MessageBox.Show(name + " is required.", name + " is required", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected static bool TryParseAngle(string text, out double angle)
    {
      if(!double.TryParse(text.Trim(), out angle)) return false;

      angle *= MathConst.DegreesToRadians;
      while(angle < 0) angle += Math.PI*2;
      while(angle >= Math.PI*2) angle -= Math.PI*2;
      return true;
    }

    protected static bool TryParseLength(string text, UnitSystem unitSystem, out double meters)
    {
      Match m = lengthRe.Match(text);
      if(!m.Success || !double.TryParse(m.Groups["number"].Value, out meters))
      {
        meters = 0;
        return false;
      }
      else
      {
        LengthUnit unit;
        switch(m.Groups["unit"].Value.ToLowerInvariant())
        {
          case "ft": unit = LengthUnit.Foot; break;
          case "km": unit = LengthUnit.Kilometer; break;
          case "kyd": unit = LengthUnit.Kiloyard; break;
          case "m": unit = LengthUnit.Meter; break;
          case "mi": unit = LengthUnit.Mile; break;
          case "nm": case "nmi": unit = LengthUnit.NauticalMile; break;
          case "yd": unit = LengthUnit.Yard; break;
          default:
            switch(unitSystem)
            {
              case UnitSystem.Imperial: unit = LengthUnit.Mile; break;
              case UnitSystem.Metric: unit = LengthUnit.Kilometer; break;
              case UnitSystem.NauticalImperial: case UnitSystem.NauticalMetric: unit = LengthUnit.NauticalMile; break;
              default: unit = LengthUnit.Meter; break;
            }
            break;
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
          case "mps": case "m/s": unit = SpeedUnit.MetersPerSecond; break;
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

    protected bool TryParseTime(string text, out TimeSpan time, out bool relative)
    {
      Match m = timeRe.Match(text);
      if(!m.Success)
      {
        time = new TimeSpan();
        relative = false;
        return false;
      }
      else
      {
        int hours, minutes, seconds;
        int.TryParse(m.Groups["hours"].Value, out hours);
        int.TryParse(m.Groups["minutes"].Value, out minutes);
        int.TryParse(m.Groups["seconds"].Value, out seconds);
        time = new TimeSpan(hours, minutes, seconds);
        relative = m.Groups["rel"].Success;
        return true;
      }
    }

    static readonly Regex lengthRe = new Regex(@"^\s*(?<number>\d+|\d*[\.,]\d+)\s*(?<unit>ft|k(?:m|yd)|mi?|nmi?|yd)?\s*$",
                                               RegexOptions.IgnoreCase);
    static readonly Regex speedRe = new Regex(@"^\s*(?<number>\d+|\d*[\.,]\d+)\s*(?<unit>k(?:n|ph|ts?)|m(?:\/s|p[sh]))?\s*$",
                                              RegexOptions.IgnoreCase);
    static readonly Regex timeRe = new Regex(@"^\s*(?<rel>\+)?\s*(?:(?<hours>\d+):)?(?<minutes>\d+)(?::(?<seconds>\d+))?\s*$",
                                             RegexOptions.IgnoreCase);
  }
}