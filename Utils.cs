using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CameraControl;

public static class Utils
{
	public static Asset<Texture2D> RequestAsset(string path)
	{
		return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
	}

	public static float GetAngle(Vector2 A, Vector2 B)
	{
		return MathF.Atan2(B.Y - A.Y, B.X - A.X);
	}

	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, int width, Color color)
	{
		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				(int)start.X,
				(int)start.Y,
				(int)Vector2.Distance(start, end) + 1, // "+ 1" to fix tiny lines from not drawing and clean up line segments
				width)
			, null, color,
			GetAngle(start, end), default, SpriteEffects.None, 0);
	}

	public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, int width, Color color, float rotation = default, Vector2 anchor = default, SpriteEffects effects = SpriteEffects.None)
	{
		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				(int)start.X,
				(int)start.Y,
				(int)Vector2.Distance(start, end) + 1, // "+ 1" to fix tiny lines from not drawing and clean up line segments
				width)
			, null, color,
			GetAngle(start, end) + rotation, anchor, effects, 0);
	}

	public static void DrawRectangleBorder(this SpriteBatch spriteBatch, Rectangle rect, int borderSize, Color color)
	{
		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				rect.X,
				rect.Y,
				rect.Width,
				borderSize),
			color);

		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				rect.X,
				rect.Y,
				borderSize,
				rect.Height),
			color);

		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				rect.X,
				rect.Bottom,
				rect.Width,
				borderSize),
			color);

		spriteBatch.Draw(TextureAssets.MagicPixel.Value,
			new Rectangle(
				rect.Right,
				rect.Y,
				borderSize,
				rect.Height),
			color);
	}
}
