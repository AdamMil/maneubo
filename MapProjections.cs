using System;
using System.Xml;

namespace Maneubo
{
  enum MapProjectionType
  {
    AzimuthalEquidistant
  }

  // formulas mostly from Map Projections - A Working Manual, by John P. Snyder (http://onlinepubs.er.usgs.gov/djvu/PP/PP_1395.pdf)
  abstract class MapProjection
  {
    protected MapProjection(double centerLongitude, double centerLatitude)
    {
      CenterLongitude = NormalizeAngle(centerLongitude);
      CenterLatitude  = NormalizeAngle(centerLatitude);
    }

    public abstract void Project(double longitude, double latitude, out double x, out double y);
    public abstract void Unproject(double x, double y, out double longitude, out double latitude);

    public readonly double CenterLongitude, CenterLatitude;

    // calculated based on https://secure.wikimedia.org/wikipedia/en/wiki/Earth_radius
    protected const double EquatorialRadius = 6378136.6; // the radius to the equator, designated 'a'
    protected const double PolarRadius = 6356751.9; // the radius to the poles, designated 'b'
    protected const double MeanRadius = (2*EquatorialRadius + PolarRadius) / 3; // the mean radius, equal to (2a+b)/3, designated R1
    // the radius of a sphere having the earth's surface area. equal to sqrt(a^2/2 + b^2/2 * invtanh(e)/e), designated R2. see 'e' below
    protected const double AuthalicRadius = 6371006.776;
    // the radius of a sphere having the earth's volume. equal to (a^2*b)^(1/3). designated R3
    protected const double VolumetricRadius = 6371000.385;
    protected const double EquatorialROC = 6335438.9; // the equatorial radius of curvature in the meridian
    protected const double PolarROC = 6399593.24; // the polar radius of curvature
    protected const double E = 0.081819220609651577; // eccentricity, equal to (a^2-b^2) / a^2, designated 'e'
    protected const double DefaultRadius = MeanRadius; // the default radius used by projections that can use various radii

    protected static double NormalizeAngle(double angle)
    {
      while(angle > Math.PI) angle -= Math.PI*2;
      while(angle <= -Math.PI) angle += Math.PI*2;
      return angle;
    }
  }

  #region AzimuthalEquidistantProjection
  sealed class AzimuthalEquidistantProjection : MapProjection
  {
    public AzimuthalEquidistantProjection(double centerLongitude, double centerLatitude) : base(centerLongitude, centerLatitude)
    {
      centerLatCos = Math.Cos(CenterLatitude);
      centerLatSin = Math.Sin(CenterLatitude);
    }

    public override void Project(double longitude, double latitude, out double x, out double y)
    {
      double latSin = Math.Sin(latitude), latCos = Math.Cos(latitude), lonCos = Math.Cos(longitude-CenterLongitude);
      double c = Math.Acos(centerLatSin*latSin + centerLatCos*latCos*lonCos);
      double kR = c / Math.Sin(c) * DefaultRadius;
      x = kR * latCos * Math.Sin(longitude-CenterLongitude);
      y = kR * (centerLatCos*latSin - centerLatSin*latCos*lonCos);
    }

    public override void Unproject(double x, double y, out double longitude, out double latitude)
    {
      double p = Math.Sqrt(x*x + y*y);
      if(p == 0)
      {
        longitude = CenterLongitude;
        latitude  = CenterLatitude;
      }
      else
      {
        double c = p / DefaultRadius, sinc = Math.Sin(c), cosc = Math.Cos(c);
        latitude = NormalizeAngle(Math.Asin(cosc*centerLatSin + y*sinc*centerLatCos/p));
        if(CenterLatitude == Math.PI/2) longitude = Math.Atan2(x, -y);
        else if(CenterLatitude == Math.PI*3/2) longitude = Math.Atan2(x, y);
        else longitude = Math.Atan2(x*sinc, p*centerLatCos*cosc - y*centerLatSin*sinc);
        longitude = NormalizeAngle(longitude + CenterLongitude);
      }
    }

    readonly double centerLatSin, centerLatCos;
  }
  #endregion
}