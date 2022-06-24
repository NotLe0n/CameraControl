using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CameraControl.UI.Elements.Curves;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using CameraControl.UI.Elements;

namespace CameraControl.UI;

class CurveEditUI : UIState
{
	public readonly List<Elements.Curves.Curve> curves = new();
	public bool drawingMode;
	public bool erasing;
	private Line _drawingCurve;
	public CurveType curveType;

	public enum CurveType
	{
		Bezier, Spline
	}

	public override void Update(GameTime gameTime)
	{
		if (drawingMode && !UIMenuButton.HoveringOverButton) {
			Main.LocalPlayer.mouseInterface = true;

			Vector2 lastCurveEnd = curves.Count == 0 ? EditorCameraSystem.RealMouseWorld : curves[^1].controls[^1];

			if (Main.mouseLeft && _drawingCurve == null) {
				_drawingCurve = new Line(lastCurveEnd, EditorCameraSystem.RealMouseWorld, Color.Orange);
			}

			if (Main.mouseLeftRelease && _drawingCurve != null) {
				AddCurve(_drawingCurve.startPoint, EditorCameraSystem.RealMouseWorld, curveType);
			}
		}

		if (erasing) {
			Main.LocalPlayer.mouseInterface = true;

			if (Main.mouseLeft) {
				curves.RemoveAll(x => x.IsHovering);
			}
		}

		FixSplineEndings();
		_drawingCurve?.Update(gameTime);
		foreach (var curve in curves) {
			curve.Update(gameTime);
		}

		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Curve Count: " + curves.Count, new Vector2(10, 20), Color.White);
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Mode: " + (drawingMode ? "Draw" : "Select"), new Vector2(10, 50), Color.White);
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Curve Type: " + curveType, new Vector2(10, 80), Color.White);

		_drawingCurve?.Draw(spriteBatch);
		foreach (var curve in curves) {
			curve.Draw(spriteBatch);
		}
	}

	private void AddCurve(Vector2 start, Vector2 end, CurveType type)
	{
		switch (type) {
			case CurveType.Bezier:
				curves.Add(new BezierCurve(start, end));
				break;
			case CurveType.Spline:
				curves.Add(new SplineCurve(start, end));
				break;
			default:
				break;
		}

		_drawingCurve = null;
	}

	private void FixSplineEndings()
	{
		for (int i = 0; i < curves.Count; i++) {
			if (curves[i] is SplineCurve curve) {
				if (i - 1 > 0)
					curve.prevPoint = curves[i - 1].controls[2];
				if (i + 1 < curves.Count)
					curve.nextPoint = curves[i + 1].controls[1];

				curve.PopulatePoints();
			}
		}
	}
}
