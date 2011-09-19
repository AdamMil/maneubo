using System;
using System.Windows.Forms;
using AdamMil.Mathematics.Geometry;
using AdamMil.Utilities;
using MB = Maneubo.ManeuveringBoard;
using AdamMil.Mathematics.Optimization;
using AdamMil.Mathematics;

namespace Maneubo
{
  partial class InterceptForm : DataForm
  {
    public InterceptForm()
    {
      InitializeComponent();
    }

    public InterceptForm(UnitShape unit, UnitShape target, UnitSystem unitSystem) : this()
    {
      if(unit == null || target == null) throw new ArgumentNullException();
      this.unit       = unit;
      this.target     = target;
      this.unitSystem = unitSystem;
      UpdateSolution();
    }

    public bool CreateWaypoints
    {
      get { return radWaypoint.Checked; }
    }

    public double Course
    {
      get { return MB.SwapBearing(Solution.Angle); }
    }

    public Point2 InterceptPoint { get; private set; }
    public Vector2 Solution { get; private set; }

    public double Speed
    {
      get { return Solution.Length; }
    }

    public double Time
    {
      get { return (InterceptPoint-unit.Position).Length / Speed; }
    }

    bool UpdateSolution()
    {
      double? speed = null, time = null, aob = null, radius = null;

      if(!string.IsNullOrEmpty(txtSpeed.Text.Trim()))
      {
        double value;
        if(!TryParseSpeed(txtSpeed.Text, unitSystem, out value))
        {
          ShowInvalidSpeed(txtSpeed.Text);
          goto invalidData;
        }
        speed = value;
      }

      if(!string.IsNullOrEmpty(txtTime.Text.Trim()))
      {
        TimeSpan timeSpan;
        bool relative;
        if(!TryParseTime(txtTime.Text, out timeSpan, out relative))
        {
          ShowInvalidTime(txtTime.Text, false);
          goto invalidData;
        }
        time = timeSpan.TotalSeconds;
      }

      if(!string.IsNullOrEmpty(txtAoB.Text.Trim()))
      {
        double value;
        if(!double.TryParse(txtAoB.Text.Trim(), out value))
        {
          MessageBox.Show(txtAoB.Text + " is not a valid angle.", "Invalid angle", MessageBoxButtons.OK, MessageBoxIcon.Error);
          goto invalidData;
        }
        aob = value * MathConst.DegreesToRadians;
      }

      if(!string.IsNullOrEmpty(txtRadius.Text.Trim()))
      {
        double value;
        if(!TryParseLength(txtRadius.Text, unitSystem, out value))
        {
          ShowInvalidLength(txtRadius.Text);
          goto invalidData;
        }
        if(value != 0) radius = value;
      }

      if(speed.HasValue && time.HasValue)
      {
        lblSolution.Text = "Remove speed or time.";
        return false;
      }

      if(aob.HasValue && !radius.HasValue)
      {
        lblSolution.Text = "Using AoB requires a radius.";
        return false;
      }

      if(radius.HasValue && !aob.HasValue)
      {
        lblSolution.Text = "Using a radius requires AoB.";
        return false;
      }

      Vector2 v = target.GetEffectiveVelocity();

      // if AoB was specified, then we're actually trying to intercept a single point on the radius circle, so make that are target point
      // and use the standard point intercept algorithm
      Point2 targetPt = !aob.HasValue ?
        target.Position : target.Position + new Vector2(0, radius.Value).Rotated(-(target.Direction + aob.Value));

      // if we've already satisfied the intercept criteria...
      Vector2 o = targetPt - unit.Position;
      if(o.LengthSqr <= (radius.HasValue && !aob.HasValue ? radius.Value*radius.Value : 0))
      {
        lblSolution.Text = "You are already there.";
        return false;
      }

      // if the target is not moving, any speed will work, so we'll just arbitrarily head there at 10 units of speed
      if(v.LengthSqr == 0)
      {
        Solution       = o.Normalized(MB.ConvertFromUnit(10, MB.GetAppropriateSpeedUnit(unitSystem)));
        InterceptPoint = targetPt;
        lblSolution.Text = MB.GetRoundedString(MB.SwapBearing(o.Angle) * MathConst.RadiansToDegrees) + "° (target stationary)";
        return true;
      }

      // if we have a single target point (i.e. the target itself, or one point on the radius circle), the intercept formula basically
      // consists in solving a quadratic formula. if we're at P and the target is at T with velocity V, then we know that the interception
      // point is at T+V*t, where t is time. if we take the vector from P to the intersection point (T+V*t-P), then the distance is
      // |T+V*t-P|. dividing by the speed s gives us the time: t = |(T+V*t-P)|/s. so s*t = |T+V*t-P|. squaring both sides, replacing T-P
      // with the helper O (i.e. translating P to the origin), and expanding the vector, we get (s*t)^2 = (Ox+Vx*t)^2 + (Oy+Vy*t)^2
      if(!time.HasValue) // if the user didn't specify the time of intercept... (if they did, the problem's solved already)
      {
        if(!speed.HasValue)
        {
          // if the user specified no information, there are an infinite number of solutions. we'll choose the one that requires
          // approximately the lowest intercept speed. if we solve the quadratic equation for speed instead of time, we get
          // s = sqrt((Ox + Vx*t)^2 + (Oy + Vy*t)^2) / t. we need to minimize this equation where s >= 0 and t >= 0. there's probably
          // some way to do this analytically, but it seems complicated, so we'll just minimize it numerically. we'll represent the time
          // TODO: check out the inverse of this function. it may be more sloped and easier to optimize?
          // TODO: consider a better method of rescaling (to get all the numbers about the same magnitude)
          Func<double, double> speedFunc = t =>
          {
            if(t <= 0) return double.NaN;
            t *= 3600; // we'll scale the time from seconds to hours because the minimizer is a bit more stable when params are around 1.0
            double x = o.X + v.X*t, y = o.Y + v.Y*t;
            return Math.Sqrt(x*x + y*y) / t;
          };
          Func<double, double> derivative = t =>
          {
            if(t <= 0) return double.NaN;
            t *= 3600;
            double x = o.X + v.X*t, y = o.Y + v.Y*t;
            return -(o.X*x + o.Y*y) / (t*t*Math.Sqrt(x*x + y*y));
          };
          ConstrainedMinimizer minimizer = new ConstrainedMinimizer(new DifferentiableMDFunction(speedFunc, derivative));
          // the function tends to be very flat near the minimum, so it's hard to find it exactly. increase the gradient tolerance to
          // prevent it from failing as often. we're going to be rounding the answer anyway, so it needn't be exact
          minimizer.GradientTolerance = 1e-6;
          // constrain the solution to be non-negative
          minimizer.AddConstraint(new DifferentiableMDFunction(t => -speedFunc(t), t => -derivative(t)));
          minimizer.SetBounds(0, 0, double.MaxValue); // constrain t to be non-negative
          try
          {
            double[] point = new double[] { 1 };
            double minSpeed = minimizer.Minimize(point);
            // now we want to round the speed up to the next unit, to make it look nicer. we also want to increase the speed by a small
            // amount because the time seems to increase without bound as the speed nears the minimum. for instance, a difference of
            // 0.01 kn near the minimum might increase the time by 5 hours. speeds near the minimum also render the math below very
            // inaccurate due to mismatching precision. so we'll add about a knot to the speed as well as round it up
            SpeedUnit speedUnit = MB.GetAppropriateSpeedUnit(unitSystem);
            speed = MB.ConvertFromUnit(Math.Ceiling(MB.ConvertToUnit(minSpeed+0.5, speedUnit)), speedUnit); // 0.5 m/s ~= 1 kn
          }
          catch(MinimumNotFoundException)
          {
            lblSolution.Text = "Try setting parameters.";
            return false;
          }
        }

        // if know the intercept speed, we take the above equation and factor out time. we end up with:
        // t^2(Vx^2 + Vy^2 - s^2) + t*(2Ox*Vx + 2Oy*Vy) + Ox^2 + Oy^2 = 0. if we take A=(Vx^2 + Vy^2 - s^2), B=2(Ox*Vx + Oy*Vy), and
        // C=Ox^2 + Oy^2, then we have the quadratic A*t^2 + B*t + C = 0 which we can solve with the quadratic formula.
        // t = (-B +/- sqrt(B^2 - 4AC)) / 2A (and we can remove a factor of 2). if the discriminant is negative, there is no solution.
        // otherwise, we take whichever solution gives the smallest non-negative time
        double A = v.X*v.X + v.Y*v.Y - speed.Value*speed.Value, B = o.X*v.X + o.Y*v.Y, C = o.X*o.X + o.Y*o.Y;

        // if A = 0, then the speeds are identical, and we get division by zero solving the quadratic. but if A = 0 then we just have
        // B*t + C = 0 or t = -C/B. we know B can't be zero because we checked the relevant cases above
        if(A == 0)
        {
          double t = -C / (2*B); // we have to multiply B by 2 because we removed a factor of two above
          if(t >= 0) time = t;
          else goto noSolution;
        }
        else
        {
          double discriminant = B*B - A*C;
          if(discriminant < 0) goto noSolution;
          double root = Math.Sqrt(discriminant), time1 = (-B + root) / A, time2 = (-B - root) / A;
          if(time1 >= 0 && time1 <= time2) time = time1;
          else if(time2 >= 0) time = time2;
          else goto noSolution;
        }
      }

      // now that we know the time of intercept, we can calculate the intercept point and get the velocity we'd need to get there.
      // the intercept point is T+V*t. the intercept vector is T+V*t-P = O+V*t. this vector has a length equal to the distance, but we
      // want a length equal to the speed, so divide by time to get speed (e.g. 10 km in 2 hour = 5km/hour). but (O+V*t)/t = O/t+V
      Solution       = o/time.Value + v;
      InterceptPoint = targetPt + v*time.Value;
      haveSolution   = true;

      lblSolution.Text = MB.GetRoundedString(MB.SwapBearing(Solution.Angle) * MathConst.RadiansToDegrees) + "° at " +
                         MB.GetSpeedString(Solution.Length, unitSystem) + " for " + GetTimeString(time.Value);
      return true;

      noSolution:
      lblSolution.Text = "No intercept is possible.";
      return false;

      invalidData:
      lblSolution.Text = "Invalid data.";
      return false;
    }

    void btnOK_Click(object sender, System.EventArgs e)
    {
      if(!haveSolution && !UpdateSolution())
      {
        MessageBox.Show(lblSolution.Text, "Can't provide solution", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      DialogResult = DialogResult.OK;
    }

    void txt_Leave(object sender, System.EventArgs e)
    {
      if(!haveSolution) UpdateSolution();
    }

    void txt_TextChanged(object sender, EventArgs e)
    {
      haveSolution = false;
    }

    readonly UnitSystem unitSystem;
    readonly UnitShape unit, target;
    bool haveSolution;

    static string GetTimeString(double seconds)
    {
      int secs = (int)Math.Ceiling(seconds);
      if(secs < 60) return secs.ToInvariantString() + "s";
      int totalMinutes = (secs+30)/60, minutes = (secs%3600+59)/60;
      return secs < 3600 ? totalMinutes.ToInvariantString() + "m" :
             (secs/3600).ToInvariantString() + "h " + minutes.ToInvariantString() + "m";
    }
  }
}
