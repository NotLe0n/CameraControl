using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;

namespace CameraControl.UI.Elements.Curves
{
	public class Line
	{
		public Vector2 startPoint;
		public Vector2 endPoint;
		public readonly Color color;
		public float Angle => MathF.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);

		private int clickedControl = -1;

		public Line(Vector2 start, Vector2 end, Color color)
		{
			startPoint = start;
			endPoint = end;
			this.color = color;
		}

		public void Update(GameTime gameTime)
		{
			var startPointRect = new Rectangle((int)startPoint.X, (int)startPoint.Y, 10, 10);
			var endPointRect = new Rectangle((int)endPoint.X, (int)endPoint.Y, 10, 10);

			if (Main.mouseLeft)
			{
				if (startPointRect.Contains(EditorCameraSystem.RealMouseWorld.ToPoint()))
				{
					// we don't move the control point here because you can lose it if you move your mouse too fast
					clickedControl = 0;
				}
				if (endPointRect.Contains(EditorCameraSystem.RealMouseWorld.ToPoint()))
				{
					// we don't move the control point here because you can lose it if you move your mouse too fast
					clickedControl = 1;
				}

				// move flagged control point
				if (clickedControl == 0)
				{
					startPoint = EditorCameraSystem.RealMouseWorld;
				}
				if (clickedControl == 1)
				{
					endPoint = EditorCameraSystem.RealMouseWorld;
				}
			}
			else
			{
				// if the mouse is released no control point should be moved
				clickedControl = -1;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			// draw line
			spriteBatch.DrawLine(startPoint - Main.screenPosition, endPoint - Main.screenPosition, 2, color);

			// draw control points
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(startPoint.X - 5 - Main.screenPosition.X), (int)(startPoint.Y - 5 - Main.screenPosition.Y), 10, 10), Color.Red);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(endPoint.X - 5 - Main.screenPosition.X), (int)(endPoint.Y - 5 - Main.screenPosition.Y), 10, 10), Color.Red);
		}
	}
}
