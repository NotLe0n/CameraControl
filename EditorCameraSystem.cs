using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace CameraControl;

internal class EditorCameraSystem : ModSystem
{
	public static Vector2 RealMouseWorld => Main.GameViewMatrix.Translation + Main.screenPosition + (Main.MouseScreen / Main.GameViewMatrix.Zoom);

	private static Vector2 position;

	public override void ModifyScreenPosition()
	{
		if (UISystem.EditorVisible && !CameraSystem.Playing && CameraSystem.trackingEntity == null) {
			Main.LocalPlayer.frozen = true;

			if (Main.keyState.IsKeyDown(Keys.W)) {
				position.Y -= 10;
			}
			if (Main.keyState.IsKeyDown(Keys.S)) {
				position.Y += 10;
			}
			if (Main.keyState.IsKeyDown(Keys.A)) {
				position.X -= 10;
			}
			if (Main.keyState.IsKeyDown(Keys.D)) {
				position.X += 10;
			}

			Main.screenPosition = position;
		}
		else {
			position = Main.screenPosition;
		}
	}

	private int lastScrollwheel;
	public static float zoom = 1;
	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
	{
		if (UISystem.EditorVisible) {
			float val = (Mouse.GetState().ScrollWheelValue - lastScrollwheel) / 4000.0f;
			zoom += val;
			zoom = MathHelper.Clamp(zoom, 0.4f, 2f);

			Transform.Zoom = new Vector2(zoom);
		}

		base.ModifyTransformMatrix(ref Transform);
		lastScrollwheel = Mouse.GetState().ScrollWheelValue;
	}

	public static void MoveToPosition(Vector2 pos)
	{
		position = pos;
	}

	public static void CenterToPosition(Vector2 pos)
	{
		position = pos - new Vector2(Main.screenWidth, Main.screenHeight) / 2;
	}
}
