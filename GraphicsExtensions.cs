using System;
using System.Drawing;
using AdamMil.Mathematics.Geometry;

namespace Maneubo
{
  static class GraphicsExtensions
  {
    public static void DrawArrow(this Graphics graphics, Pen pen, PointF start, PointF end)
    {
      graphics.DrawArrow(pen, start.X, start.Y, end.X, end.Y);
    }

    public static void DrawArrow(this Graphics graphics, Pen pen, float x, float y, float x2, float y2)
    {
      // draw the main line
      graphics.DrawLine(pen, x, y, x2, y2);

      // draw the end cap
      const double Angle = 60 * MathConst.DegreesToRadians;
      Vector2 vector = new Vector2(x-x2, y-y2).GetNormal(8); // the length of the end cap segments

      Vector2 rotated = vector.Rotate(Angle/2);
      graphics.DrawLine(pen, x2, y2, x2+(float)rotated.X, y2+(float)rotated.Y);

      rotated = vector.Rotate(-Angle/2);
      graphics.DrawLine(pen, x2, y2, x2+(float)rotated.X, y2+(float)rotated.Y);
    }

    public static void DrawCircle(this Graphics graphics, Pen pen, float x, float y, float radius)
    {
      float diameter = radius*2;
      graphics.DrawEllipse(pen, x-radius, y-radius, diameter, diameter);
    }

    public static void DrawCircle(this Graphics graphics, Pen pen, PointF center, float radius)
    {
      float diameter = radius*2;
      graphics.DrawEllipse(pen, center.X-radius, center.Y-radius, diameter, diameter);
    }

    public static void DrawHLine(this Graphics graphics, Pen pen, int x, int y, int x2)
    {
      graphics.DrawLine(pen, x, y, x2, y);
    }

    public static void DrawVLine(this Graphics graphics, Pen pen, int x, int y, int y2)
    {
      graphics.DrawLine(pen, x, y, x, y2);
    }

    public static void DrawRectangle(this Graphics graphics, Pen pen, RectangleF rectangle)
    {
      graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Height, rectangle.Width);
    }
  }
}