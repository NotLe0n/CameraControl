using CameraControl.UI.Elements;
using CameraControl.UI.Elements.Curves;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

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
			Main.LocalPlayer.mouseInterface = true;  // don't use items

			// last curve end is the last point of a curve, or the mouse position if there are no curves
			Vector2 lastCurveEnd = curves.Count == 0 ? EditorCameraSystem.RealMouseWorld : curves[^1].controls[^1];

			if (Main.mouseLeft && _drawingCurve == null) {
				_drawingCurve = new Line(lastCurveEnd, EditorCameraSystem.RealMouseWorld, Color.Orange);
			}

			if (Main.mouseLeftRelease && _drawingCurve != null) {
				AddCurve(_drawingCurve.startPoint, EditorCameraSystem.RealMouseWorld, curveType);
				_drawingCurve = null; // remove temporary line
			}
		}

		if (erasing) {
			Main.LocalPlayer.mouseInterface = true; // don't use items

			if (Main.mouseLeft) {
				curves.RemoveAll(x => x.IsHovering); // remove all curves, the mouse is hovering over
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
		// Debug draw
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Curve Count: " + curves.Count, new Vector2(10, 20), Color.White);
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Mode: " + (drawingMode ? "Draw" : "Select"), new Vector2(10, 50), Color.White);
		spriteBatch.DrawString(FontAssets.MouseText.Value, "Curve Type: " + curveType, new Vector2(10, 80), Color.White);

		// draw all curves
		_drawingCurve?.Draw(spriteBatch);
		foreach (var curve in curves) {
			curve.Draw(spriteBatch);
		}
	}

	private void AddCurve(Vector2 start, Vector2 end, CurveType type)
	{
		if (type is CurveType.Bezier) {
			curves.Add(new BezierCurve(start, end));
		}
		else if (type is CurveType.Spline) {
			curves.Add(new SplineCurve(start, end));
		}
	}

	// makes spine curves connect to eachother nicely
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
