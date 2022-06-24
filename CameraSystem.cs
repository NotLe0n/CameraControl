using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CameraControl;

internal class CameraSystem : ModSystem
{
	public static bool Playing {
		get => _playing;
		set {
			Reset();
			UpdateProgressbar(0, 0);
			_playing = value;
		}
	}

	public static bool repeat;
	public static bool bounce;
	public static Entity trackingEntity = null;

	private static bool _playing;
	private static bool reverse;
	private static float speed = 10;
	private static float t;
	private static int currentCurve;
	private static int segment;

	private static float progress;

	public override void ModifyScreenPosition()
	{
		base.ModifyScreenPosition();

		if (!_playing) {
			// track NPC if not playing
			if (trackingEntity != null)
				CenterCameraTo(trackingEntity.position);

			return;
		}

		var curves = UISystem.CurveEditUI.curves;

		// don't track curve if there are none
		if (curves.Count == 0) {
			_playing = false;

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
			_playing = repeat || bounce; // if repeat or bounce isn't set: stop playing
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
			_playing = repeat || bounce; // if repeat or bounce isn't set: stop playing

			Reset();
		}
	}

	// calculate progress percentage and update progressbar
	private static void UpdateProgressbar(int segments, int curveCount)
	{
		UISystem.CameraControlUI.progressBar.Progress = progress / segments / curveCount;
	}

	public static void Reset()
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
		return UISystem.CurveEditUI.curves[currentCurve].points[segment];
	}

	public static void StopPlaying()
	{
		Reset();
		UpdateProgressbar(0, 0);
		_playing = false;
	}
}
