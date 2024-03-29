﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace CameraControl.UI.Elements;

internal class UIMenuButton : UIImageButton
{
	public static bool HoveringOverButton { get; private set; }

	public string hoverText;
	public Func<string> dynamicHoverText;
	public Func<bool> toggleAction; // changes when the "toggled frame" should be drawn

	private readonly Func<string> dynamicTexture;
	private bool _toggled;
	private readonly Texture2D frame = Utils.RequestAsset("CameraControl/Assets/selected").Value;

	public UIMenuButton(string texture, string hoverText) : base(Utils.RequestAsset(texture))
	{
		this.hoverText = hoverText;
		toggleAction ??= () => _toggled; // if no special action is set, the frame should toggle on and off on every click
	}

	public UIMenuButton(string texture, Func<string> dynamicHoverText) : base(Utils.RequestAsset(texture))
	{
		this.dynamicHoverText = dynamicHoverText;
		toggleAction ??= () => _toggled; // if no special action is set, the frame should toggle on and off on every click
	}

	public UIMenuButton(Func<string> dynamicTexture, Func<string> dynamicHoverText) : base(Utils.RequestAsset(dynamicTexture()))
	{
		this.dynamicTexture = dynamicTexture;
		this.dynamicHoverText = dynamicHoverText;
		toggleAction ??= () => _toggled; // if no special action is set, the frame should toggle on and off on every click
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		SoundEngine.PlaySound(SoundID.MenuTick); // tick sound
		_toggled = !_toggled;

		base.LeftClick(evt);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		if (dynamicTexture != null) {
			SetImage(Utils.RequestAsset(dynamicTexture()));
		}

		// draw a white frame
		if (toggleAction()) {
			spriteBatch.Draw(frame, GetDimensions().Position(), Color.White);
		}

		// draw hover text and prevent item use on hover
		if (IsMouseHovering) {
			Main.hoverItemName = dynamicHoverText == null ? hoverText : dynamicHoverText(); // if dynamic hovertext isn't set use the normal hovertext
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		HoveringOverButton = true;
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		HoveringOverButton = false;
	}
}
