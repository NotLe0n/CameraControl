using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria;
using Terraria.UI;

namespace CameraControl.UI.Elements;

public class UIProgressbar : UIElement
{
	public float Progress { get; set; }

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var dim = GetDimensions().ToRectangle();

		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(dim.X, dim.Y, (int)(dim.Width * Progress), dim.Height), Color.Red); // draw progressbar
		spriteBatch.DrawRectangleBorder(dim, 2, Color.Black); // draw black border

		// draw percentage and prevent item use on hover
		if (IsMouseHovering) {
			Main.hoverItemName = Progress + "%";
			Main.LocalPlayer.mouseInterface = true;
		}
	}
}
