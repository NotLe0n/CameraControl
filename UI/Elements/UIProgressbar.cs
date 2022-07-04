using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace CameraControl.UI.Elements;

public class UIProgressbar : UIElement
{
	//public event Action<float> OnHold;
	public float Progress { get; set; }
	private bool holding;
	private readonly Asset<Texture2D> _texture;
	private readonly Asset<Texture2D> _innerTexture;

	public UIProgressbar()
	{
		Progress = 0;
		_texture = Main.Assets.Request<Texture2D>("Images/UI/Scrollbar");
		_innerTexture = Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner");
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		// prevents the Progress from being NaN or Infinity
		if (!float.IsFinite(Progress)) {
			Progress = 0;
		}

		if (holding && UISystem.CurveEditUI.curves.Count > 0) {
			var dim = GetDimensions().ToRectangle();
			var p = MathHelper.Clamp((Main.MouseScreen.X - dim.X) / dim.Width, 0, 1);
			Progress = p;

			CameraSystem.SetProgress(p);
			EditorCameraSystem.CenterToPosition(CameraSystem.GetPositionAtPercentage(p));
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
			Main.hoverItemName = $"{Progress:P}"; // formatted as Percent
			Main.LocalPlayer.mouseInterface = true;
		}
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
