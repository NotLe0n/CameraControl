using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CameraControl.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Graphics;

namespace CameraControl;

internal class UISystem : ModSystem
{
	public static CameraControlUI CameraControlUI { get; private set; }
	public static CurveEditUI CurveEditUI { get; private set; }

	private static UserInterface ccUI;
	private static UserInterface ceUI;

	public static bool EditorVisible { get; private set; }

	public override void Load()
	{
		CurveEditUI = new CurveEditUI();
		CameraControlUI = new CameraControlUI();

		ccUI = new UserInterface();
		ccUI.SetState(CameraControlUI);
		ceUI = new UserInterface();
		ceUI.SetState(CurveEditUI);

		base.Load();
	}

	public override void Unload()
	{
		ccUI = null;
		ceUI = null;
		CurveEditUI = null;
		CameraControlUI = null;

		base.Unload();
	}

	private GameTime _lastUpdateUiGameTime;
	public override void UpdateUI(GameTime gameTime)
	{
		_lastUpdateUiGameTime = gameTime;

		if (EditorVisible)
		{
			ccUI.Update(gameTime);
			ceUI.Update(gameTime);
		}

		base.UpdateUI(gameTime);
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex != -1)
		{
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"CameraControl: UI",
				delegate
				{
					if (ccUI.IsVisible)
						ccUI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.UI));
		}
		if (mouseTextIndex != -1)
		{
			layers.Add(new LegacyGameInterfaceLayer(
				"CameraControl: CurveDrawArea",
				delegate
				{
					if (ceUI.IsVisible)
						ceUI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.Game));
		}

		base.ModifyInterfaceLayers(layers);
	}

	public static void ToggleUI()
	{
		EditorVisible = !EditorVisible;
		ccUI.IsVisible = !ccUI.IsVisible;
		ceUI.IsVisible = !ceUI.IsVisible;
	}
}
