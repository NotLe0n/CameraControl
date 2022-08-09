using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace CameraControl.UI.Elements.Curves;

public abstract class Curve
{
	public readonly Vector2[] controls;
	public Vector2[] points;
	protected int clickedControl = -1;

	private bool _isHovering;
	public bool IsHovering => _isHovering;

	private bool _selected;
	public bool Selected => _selected;

	public const int NumSteps = (int)(1.0f / Factor);
	public const float Factor = 0.00666f;

	protected Curve(Vector2[] controls)
	{
		this.controls = controls;
	}

	protected Curve(Vector2 start, Vector2 end)
	{
		// add control points at start, 30%, 70% and end
		controls = new[] {
				start,
				0.7f * start + 0.3f * end,
				0.3f * start + 0.7f * end,
				end
			};
	}

	public void Update(GameTime gameTime)
	{
		// if the curve is clicked select it
		if (Main.mouseLeft) {
			_selected = IsHovering;
		}

		for (int i = 0; i < controls.Length; i++) {
			var rect = new Rectangle((int)controls[i].X - 5, (int)controls[i].Y - 5, 10, 10);

			// if the mouse is hovering over a control point...
			if (rect.Contains(EditorCameraSystem.RealMouseWorld.ToPoint())) {
				// ... and it is clicked ...
				if (Main.mouseLeft) {
					// ... flag the control point
					// we don't move the control point here because you can lose it if you move your mouse too fast
					clickedControl = i;
				}
			}

			// move flagged control point
			if (clickedControl == i) {
				controls[i] = EditorCameraSystem.RealMouseWorld;
				_selected = true;

				PopulatePoints();
			}
		}

		// if the mouse is released no control point should be moved
		if (Main.mouseLeftRelease) {
			clickedControl = -1;
		}

		var min = Vector2.Min(Vector2.Min(controls[0], controls[1]), Vector2.Min(controls[2], controls[3]));
		var max = Vector2.Max(Vector2.Max(controls[0], controls[1]), Vector2.Max(controls[2], controls[3]));
		bool inside = EditorCameraSystem.RealMouseWorld.X >= min.X &&
					  EditorCameraSystem.RealMouseWorld.X <= min.X + max.X &&
					  EditorCameraSystem.RealMouseWorld.Y >= min.Y &&
					  EditorCameraSystem.RealMouseWorld.Y <= min.Y + max.Y;

		if (!inside) {
			_isHovering = false;
			return;
		}

		// detect if mouse is hovering over curve
		_isHovering = false;
		Vector2 closestPoint = default;
		float bestDistance = float.MaxValue;
		float currentDistance;
		for (int i = 0; i < points.Length; i++) {
			// find closest point on the curve to the mouse
			currentDistance = Vector2.DistanceSquared(points[i], EditorCameraSystem.RealMouseWorld);
			if (currentDistance < bestDistance) {
				bestDistance = currentDistance;
				closestPoint = points[i];
			}
		}

		// mouse is hovering if it's within 50 pixels of the closest point to the mouse
		float distance = Vector2.Distance(closestPoint, EditorCameraSystem.RealMouseWorld);
		const float detectionRange = 50;
		if (distance > -detectionRange && distance < detectionRange) {
			_isHovering = true;
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		// draw control points
		if (IsHovering || Selected) {
			foreach (Vector2 control in controls) {
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(control.X - 5 - Main.screenPosition.X), (int)(control.Y - 5 - Main.screenPosition.Y), 10, 10), Color.Red);
			}
		}

		// draw points
		for (int i = 0; i + 1 < points.Length; i++) {
			Vector2 pt = points[i] - Main.screenPosition;
			Vector2 npt = points[i + 1] - Main.screenPosition;

			// draw line
			spriteBatch.DrawLine(pt, npt, 2,
				IsHovering || Selected ? Color.Orange : Color.White); // if the curve is hovered or selected draw the points in orange, otherwise in white
		}
	}

	public abstract void PopulatePoints();
}
