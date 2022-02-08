using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace CameraControl;

internal class HotkeySystem : ModSystem
{
	public static ModKeybind OpenUIHotkey { get; private set; }

	public override void Load()
	{
		OpenUIHotkey = KeybindLoader.RegisterKeybind(Mod, "open UI", Keys.N);

		base.Load();
	}

	public override void Unload()
	{
		OpenUIHotkey = null;

		base.Unload();
	}
}

internal class HotkeyPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (HotkeySystem.OpenUIHotkey.JustPressed)
		{
			UISystem.ToggleUI();
		}

		base.ProcessTriggers(triggersSet);
	}
}
