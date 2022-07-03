﻿using CameraControl.UI.Elements.Curves;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CameraControl;

internal class CameraSystem : ModSystem
{
	public static bool repeat;
	public static bool bounce;
	public static Entity trackingEntity = null;

	private static bool playing;
	private static bool reverse;
	private static float speed = 10;
	private static float t;
	private static int currentCurve;
	private static int segment;

	private static float progress;

	public override void ModifyScreenPosition()
	{
		base.ModifyScreenPosition();

		if (!playing) {
			// track NPC if not playing
			if (trackingEntity != null)
				CenterCameraTo(trackingEntity.position);

			return;
		}

		var curves = UISystem.CurveEditUI.curves;

		// don't track curve if there are none
		if (curves.Count == 0) {
			playing = false;

			// reset progress to 0
			progress = 0f;
			UpdateProgressbar(0, 0);
			return;
		}

		Vector2 start = curves[currentCurve].points[segment];
		Vector2 end;

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

		// Update progressbar
		UpdateProgressbar(curves[currentCurve].points.Length, curves.Count);
	}

	private static void TrackForwards(List<UI.Elements.Curves.Curve> curves, Vector2 start, Vector2 end) // "UI.Elements.Curves" because of conflict with XNA
	{
		Vector2 middle = new Vector2(Main.screenWidth, Main.screenHeight) / 2; // the middle of the screen

		// increase lerp value
		t += 0.1f / Vector2.Distance(start, end) * speed;

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

	private static void TrackBackwards(List<UI.Elements.Curves.Curve> curves, Vector2 start, Vector2 end)
	{
		// increase lerp value
		t -= 0.1f / Math.Max(Vector2.Distance(start, end), 1) * speed;

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

	// calculate progress percentage and update progressbar
	private static void UpdateProgressbar(int segments, int curveCount)
	{
		UISystem.CameraControlUI.progressBar.Progress = progress / segments / curveCount;
	}

	private static void Reset()
	{
		t = 0;
		segment = 0;
		currentCurve = 0;
		progress = 0;
		reverse = false;
		UpdateProgressbar(0, 0);
	}

	public static void SetSpeed(float speed)
	{
		CameraSystem.speed = speed;
	}

	private static void CenterCameraTo(Vector2 position)
	{
		Main.screenPosition = position - (new Vector2(Main.screenWidth, Main.screenHeight) / 2);
	}

	public static Vector2 GetPositionAtPercentage(float percentage)
	{
		if (!playing)
			return Vector2.Zero;

		int curveCount = UISystem.CurveEditUI.curves.Count;
		var curve = UISystem.CurveEditUI.curves[Math.Min((int)(percentage * curveCount), curveCount - 1)];
		float t = (percentage * curveCount) % 1;

		if (curve is BezierCurve) {
			return BezierCurve.CalculateBezierCurve(t, curve.controls[0], curve.controls[1], curve.controls[2], curve.controls[3]);
		}
		else if (curve is SplineCurve s) {
			if (t <= 0.3333f) {
				return SplineCurve.CalculateCatmullRomSpline(t * 3 % 1, s.prevPoint, s.controls[0], s.controls[1], s.controls[2]);
			}
			else if (t <= 0.6666) {
				return SplineCurve.CalculateCatmullRomSpline(t * 3 % 1, s.controls[0], s.controls[1], s.controls[2], s.controls[3]);
			}
			else {
				return SplineCurve.CalculateCatmullRomSpline(t * 3 % 1, s.controls[1], s.controls[2], s.controls[3], s.nextPoint);
			}
		}
		else {
			return Vector2.Zero;
		}
	}

	public static void StartPlaying()
	{
		playing = true;
	}

	public static void StopPlaying()
	{
		Reset();
		UpdateProgressbar(0, 0);
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
}
