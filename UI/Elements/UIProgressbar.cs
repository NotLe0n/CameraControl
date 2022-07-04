using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using System.Linq;

namespace CameraControl.UI.Elements;

public class UIProgressbar : UIElement
{
	public float Progress { get; set; }
	private bool holding;
	private readonly Asset<Texture2D> _texture;
	private readonly Asset<Texture2D> _innerTexture;

	public Dictionary<float, float> keyframes; // [percentage] = speed
	private int hoveringOverKeyframe;

	public UIProgressbar()
	{
		Progress = 0;
		keyframes = new() { { 0, 1 } };
		_texture = Main.Assets.Request<Texture2D>("Images/UI/Scrollbar");
		_innerTexture = Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner");

		hoveringOverKeyframe = -1;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		var dim = GetDimensions().ToRectangle();

		// prevents the Progress from being NaN or Infinity
		if (!float.IsFinite(Progress)) {
			Progress = 0;
		}

		if (holding && UISystem.CurveEditUI.curves.Count > 0) {
			var p = MathHelper.Clamp((Main.MouseScreen.X - dim.X) / dim.Width, 0, 1);
			Progress = p;

			CameraSystem.SetProgress(p);
			EditorCameraSystem.CenterToPosition(CameraSystem.GetPositionAtPercentage(p));
		}

		if (IsMouseHovering) {
			int i = 0;
			foreach (var keyframe in keyframes) {			
				var p = MathHelper.Clamp((Main.MouseScreen.X - dim.X) / dim.Width, 0, 1);

				if ((keyframe.Key - p) is < 0.005f and > -0.005f) {
					hoveringOverKeyframe = i;
					break;
				}
				else {
					hoveringOverKeyframe = -1;
				}
				i++;
			}
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var dim = GetDimensions().ToRectangle();

		DrawBar(spriteBatch, dim);
		DrawHandle(spriteBatch, new Rectangle(dim.X, dim.Y, (int)(dim.Width * Progress), dim.Height));

		// draw percentage and prevent item use on hover
		if (IsMouseHovering) {
			if (hoveringOverKeyframe == -1) {
				Main.hoverItemName = $"{Progress:P}"; // formatted as Percent
			}
			else {
				Main.hoverItemName = $"Keyframe #{hoveringOverKeyframe + 1}\nSpeed: {keyframes.ElementAt(hoveringOverKeyframe).Value}";
			}
			Main.LocalPlayer.mouseInterface = true;
		}

		foreach (var keyframe in keyframes) {
			var x = (int)(keyframe.Key * dim.Width) + dim.X;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x - 2, dim.Y + 5, 5, 10), Color.Red);
		}
	}

	public override void RightClick(UIMouseEvent evt)
	{
		base.RightClick(evt);

		var dim = GetDimensions().ToRectangle();
		var p = MathHelper.Clamp((Main.MouseScreen.X - dim.X) / dim.Width, 0, 1);

		foreach (var keyframe in keyframes) {
			if ((keyframe.Key - p) is < 0.005f and > -0.005f && keyframe.Key != 0) {
				keyframes.Remove(keyframe.Key, out _);
				return;
			}
		}
		keyframes.Add(p, 1);
		keyframes = new Dictionary<float, float>(keyframes.OrderBy(x => x.Key));
	}

	public override void MouseDown(UIMouseEvent evt)
	{
		base.MouseDown(evt);
		holding = true;
	}

	public override void MouseUp(UIMouseEvent evt)
	{
		base.MouseUp(evt);
		holding = false;
	}

	private void DrawBar(SpriteBatch spriteBatch, Rectangle dimensions)
	{
		Texture2D texture = _texture.Value;

		// draw top
		spriteBatch.Draw(texture, new Rectangle(dimensions.X - 6, dimensions.Y, 6, dimensions.Height), new Rectangle(0, 0, 6, texture.Height), Color.White);
		// draw middle
		spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle(8, 0, 4, texture.Height), Color.White);
		// draw bottom
		spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width, dimensions.Y, 6, dimensions.Height), new Rectangle(texture.Width - 6, 0, 6, texture.Height), Color.White);
	}

	private void DrawHandle(SpriteBatch spriteBatch, Rectangle dimensions)
	{
		Texture2D texture = _innerTexture.Value;
		Color color = Color.White * (holding ? 1 : 0.8f);

		// draw top
		spriteBatch.Draw(texture, new Rectangle(dimensions.X - 8, dimensions.Y + 3, 8, dimensions.Height - 6), new Rectangle(0, 0, 8, texture.Height), color);
		// draw middle
		spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y + 3, dimensions.Width, dimensions.Height - 6), new Rectangle(8, 0, 4, texture.Height), color);
		// draw bottom
		spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width, dimensions.Y + 3, 8, dimensions.Height - 6), new Rectangle(texture.Width - 8, 0, 8, texture.Height), color);
	}
}
