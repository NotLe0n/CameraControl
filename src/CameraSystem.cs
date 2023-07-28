using CameraControl.UI.Elements.Curves;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using System.Linq;
using Curve = CameraControl.UI.Elements.Curves.Curve;

namespace CameraControl;

internal class CameraSystem : ModSystem
{
	public static bool repeat;
	public static bool bounce;
	public static Entity trackingEntity = null;

	private static Vector2? lockPosition;
	private static bool playing;
	private static bool reverse;
	private static float currentSpeedMult = 1;
	private static float t;
	private static int currentCurve;
	private static int segment;

	private static float progress;

	public override void ModifyScreenPosition()
	{
		base.ModifyScreenPosition();

		if (!playing) {
			// track NPC if not playing
			if (lockPosition.HasValue) {
				Main.screenPosition =  lockPosition.Value;
			}
			else if (trackingEntity != null) {
				CenterCameraTo(trackingEntity.position);
			}

			return;
		}

		var curves = UISystem.CurveEditUI.curves;

		// don't track curve if there are none
		if (curves.Count == 0) {
			playing = false;

			// reset progress to 0
			progress = 0f;
			UpdateProgressbar();
			return;
		}

		Vector2 start = curves[currentCurve].points[segment];
		Vector2 end;

		// change speed
		float p = progress / Curve.NumSteps / UISystem.CurveEditUI.curves.Count;
		foreach (var keyframe in UISystem.CameraControlUI.progressBar.keyframes) {
			if (Math.Abs(keyframe.Key - p) < 0.005f) {
				currentSpeedMult = keyframe.Value;
			}
		}

		if (!reverse) {
			bool segmentEnd = segment + 1 >= curves[currentCurve].points.Length; // is the last segment reached?
			end = segmentEnd ? start : curves[currentCurve].points[segment + 1]; // if yes, go to start, if no go to next segment

			CenterCameraTo(Vector2.Lerp(start, end, t)); // move screen
			TrackForwards(curves, start, end); // update variables
		}
		else {
			bool segmentEnd = segment - 1 < 0; // is the last segment reached?
			end = segmentEnd ? start : curves[currentCurve].points[segment - 1]; // if yes, go to start, if no go to next segment

			CenterCameraTo(Vector2.Lerp(end, start, t)); // move screen
			TrackBackwards(curves, start, end); // update variables
		}

		UpdateProgressbar();
	}

	private static void TrackForwards(List<Curve> curves, Vector2 start, Vector2 end) // "UI.Elements.Curves" because of conflict with XNA
	{
		Vector2 middle = new Vector2(Main.screenWidth, Main.screenHeight) / 2; // the middle of the screen

		// increase lerp value
		t += 0.1f / (Vector2.Distance(start, end) / (currentSpeedMult * 10f));

		// if we reached the end of a line segment, reset t and go to the next segment
		if (t > 1 || Main.screenPosition == end - middle) {
			segment++;
			t = 0;
			progress++;
		}

		// if we reached the end of the last segment, reset segment and go the the next curve
		if (segment >= curves[currentCurve].points.Length) {
			segment = 0;
			currentCurve++;
			progress++;
		}

		// if we reached the end of the last curve, reset currentCurve and stop playing
		if (currentCurve >= curves.Count) {
			currentCurve = 0;
			playing = repeat || bounce; // if repeat or bounce isn't set: stop playing
			progress = 0;

			// if bounce is set: reverse tracking
			if (bounce) {
				reverse = true;
				t = 1;
				segment = curves[currentCurve].points.Length - 1;
				currentCurve = curves.Count - 1;
				progress = curves[currentCurve].points.Length * curves.Count;
			}
		}
	}

	private static void TrackBackwards(List<Curve> curves, Vector2 start, Vector2 end)
	{
		// increase lerp value
		t -= 0.1f / (Math.Max(Vector2.Distance(start, end), 1) / (currentSpeedMult * 10f));

		// if we reached the end of a line segment, reset t and go to the next segment
		if (t <= 0) {
			segment--;
			t = 1;
			progress--;
		}

		// if we reached the end of the last segment, reset segment and go the the next curve
		if (segment < 0) {
			segment = curves[currentCurve].points.Length - 1;
			currentCurve--;
			progress--;
		}

		// if we reached the end of the last curve, reset currentCurve and stop playing
		if (currentCurve < 0) {
			currentCurve = curves.Count - 1;
			playing = repeat || bounce; // if repeat or bounce isn't set: stop playing

			Reset();
		}
	}

	public static void SetProgress(float newProgress)
	{
		int curveCount = UISystem.CurveEditUI.curves.Count;
		currentCurve = Math.Min((int)(newProgress * curveCount), curveCount - 1);
		segment = (int)(newProgress * Curve.NumSteps * curveCount) % (Curve.NumSteps + 1);
		progress = newProgress * Curve.NumSteps * curveCount;
	}

	// calculate progress percentage and update progressbar
	private static void UpdateProgressbar()
	{
		UISystem.CameraControlUI.progressBar.Progress = progress / Curve.NumSteps / UISystem.CurveEditUI.curves.Count;
	}

	private static void Reset()
	{
		t = 0;
		segment = 0;
		currentCurve = 0;
		progress = 0;
		reverse = false;
		currentSpeedMult = 1;
		UpdateProgressbar();
	}

	private static void CenterCameraTo(Vector2 position)
	{
		Main.screenPosition = new Vector2((int)position.X, (int)position.Y) - (new Vector2(Main.screenWidth, Main.screenHeight) / 2);
	}

	public static Vector2 GetPositionAtPercentage(float percentage)
	{
		int curveCount = UISystem.CurveEditUI.curves.Count;
		var curve = UISystem.CurveEditUI.curves[Math.Min((int)(percentage * curveCount), curveCount - 1)];
		float t0 = (Math.Abs(percentage - 1) < 0.001f) ? 1 : (percentage * curveCount) % 1;

		if (curve is BezierCurve) {
			return BezierCurve.CalculateBezierCurve(t0, curve.controls[0], curve.controls[1], curve.controls[2], curve.controls[3]);
		}

		if (curve is SplineCurve s) {
			if (t0 <= 0.3333f) {
				return SplineCurve.CalculateCatmullRomSpline(t0 * 3 % 1, s.prevPoint, s.controls[0], s.controls[1], s.controls[2]);
			}

			if (t0 <= 0.6666) {
				return SplineCurve.CalculateCatmullRomSpline(t0 * 3 % 1, s.controls[0], s.controls[1], s.controls[2], s.controls[3]);
			}
			return SplineCurve.CalculateCatmullRomSpline(t0 * 3 % 1, s.controls[1], s.controls[2], s.controls[3], s.nextPoint);
		}
		return Vector2.Zero;
	}

	public static void StartPlaying()
	{
		playing = true;
	}

	public static void StopPlaying()
	{
		Reset();
		playing = false;
	}

	public static void TogglePause()
	{
		playing = !playing;
	}

	public static bool IsPlaying()
	{
		return playing;
	}

	public static void ToggleLock()
	{
		lockPosition = lockPosition is null ? Main.screenPosition : null;
	}

	public static bool IsLocked()
	{
		return lockPosition is not null;
	}

	public static void ChangeSpeed(float amount)
	{
		var keyframes = UISystem.CameraControlUI.progressBar.keyframes;

		float p = progress / Curve.NumSteps / UISystem.CurveEditUI.curves.Count;

		foreach (var keyframe in keyframes) {
			if (keyframe.Key > p) {
				int prevIndex = keyframes.ToList().IndexOf(keyframe) - 1;
				var previous = keyframes.ElementAt(prevIndex);
				keyframes[previous.Key] *= amount;

				return;
			}		
		}

		keyframes[keyframes.Last().Key] *= amount;
	}
}
