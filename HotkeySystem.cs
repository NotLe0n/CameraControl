using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace CameraControl;

internal class HotkeySystem : ModSystem
{
	public static ModKeybind OpenUIHotkey { get; private set; }
	public static ModKeybind PlayPauseHotkey { get; private set; }
	public static ModKeybind BounceHotkey { get; private set; }
	public static ModKeybind RepeatHotkey { get; private set; }
	public static ModKeybind LockScreenHotkey { get; private set; }

	public override void Load()
	{
		OpenUIHotkey = KeybindLoader.RegisterKeybind(Mod, "open UI", Keys.N);
		PlayPauseHotkey = KeybindLoader.RegisterKeybind(Mod, "Play/Pause", Keys.None);
		BounceHotkey = KeybindLoader.RegisterKeybind(Mod, "Toggle Bounce", Keys.None);
		RepeatHotkey = KeybindLoader.RegisterKeybind(Mod, "Toggle Repeat", Keys.None);
		LockScreenHotkey = KeybindLoader.RegisterKeybind(Mod, "Toggle Lock Screen", Keys.None);

		base.Load();
	}

	public override void Unload()
	{
		OpenUIHotkey = PlayPauseHotkey = BounceHotkey = RepeatHotkey = LockScreenHotkey = null;

		base.Unload();
	}
}

internal class HotkeyPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (HotkeySystem.OpenUIHotkey.JustPressed) {
			UISystem.ToggleUI();
		}
		else if (HotkeySystem.PlayPauseHotkey.JustPressed) {
			CameraSystem.TogglePause();
		}
		else if (HotkeySystem.BounceHotkey.JustPressed) {
			CameraSystem.bounce = !CameraSystem.bounce;
		}
		else if (HotkeySystem.RepeatHotkey.JustPressed) {
			CameraSystem.repeat = !CameraSystem.repeat;
		}
		else if (HotkeySystem.LockScreenHotkey.JustPressed) {
			CameraSystem.ToggleLock();
		}

		base.ProcessTriggers(triggersSet);
	}
}
