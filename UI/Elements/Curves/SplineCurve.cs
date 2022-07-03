using Microsoft.Xna.Framework;

namespace CameraControl.UI.Elements.Curves;

class SplineCurve : Curve
{
	public Vector2 prevPoint;
	public Vector2 nextPoint;

	public SplineCurve(Vector2 start, Vector2 end) : base(start, end)
	{
		prevPoint = controls[0];
		nextPoint = controls[3];

		points = new Vector2[NumSteps + 1];
		PopulatePoints();
	}

	public override void PopulatePoints()
	{
		// calculation only goes from point 1-2 with points 0 and 3 being "control points"
		// to still be able to see a connection from 0-1 and 2-3 we calculate 2 more times with the control points from another curve (gets updated in CurveDrawArea.FixSplineEndings())
		for (int i = 0; i <= NumSteps / 3; i++) {
			float t = i * Factor * 3;

			//points[i] = CalculateCatmullRomSpline(t, controls[0], controls[1], controls[2], controls[3]);
			points[i] = CalculateCatmullRomSpline(t, prevPoint, controls[0], controls[1], controls[2]);
			points[i + NumSteps / 3] = CalculateCatmullRomSpline(t, controls[0], controls[1], controls[2], controls[3]);
			points[i + NumSteps / 3 * 2] = CalculateCatmullRomSpline(t, controls[1], controls[2], controls[3], nextPoint);
		}
	}

	// Calculates the point at t on a Catmull-Rom spline
	public static Vector2 CalculateCatmullRomSpline(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return 0.5f * ((2 * p1) +
		(-p0 + p2) * t +
		(2 * p0 - 5 * p1 + 4 * p2 - p3) * (t * t) +
		(-p0 + 3 * p1 - 3 * p2 + p3) * (t * t * t));
	}
}
