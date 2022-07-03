using CameraControl.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace CameraControl.UI;

internal class CameraControlUI : UIState
{
	public UIProgressbar progressBar; // progressbar instance

	private bool drawView; // if true, draws big box representing the screen at a tracking location with 100% zoom
	private bool selectNpcToTrack; // if true, draws green boxes around npcs

	private const float hAlign = 0.35f; // position of the first button
	private const string path = "CameraControl/UI/Assets/"; // common path for all UI Assets

	public CameraControlUI()
	{
		progressBar = new UIProgressbar() {
			Top = new(-150, 1),
			Left = new(0, hAlign),
			Width = new(450, 0),
			Height = new(20, 0)
		};
		Append(progressBar);

		var startStopBtn = new UIMenuButton(
				() => $"{path + (!CameraSystem.IsPlaying() ? "stopBtn" : "playBtn")}",
				() => $"{(CameraSystem.IsPlaying() ? "Stop" : "Start")} Tracking") {
			Top = new(-120, 1),
			Left = new(0, hAlign),
			toggleAction = () => CameraSystem.IsPlaying() // draw frame only while tracking the curve
		};
		startStopBtn.OnClick += StartBtn_OnClick;
		Append(startStopBtn);

		var repeatBtn = new UIMenuButton(path + "repeatBtn", "Repeat") {
			Top = new(-120, 1),
			Left = new(50, hAlign)
		};
		repeatBtn.OnClick += (_, __) => CameraSystem.repeat = !CameraSystem.repeat;
		Append(repeatBtn);

		var bounceBtn = new UIMenuButton(path + "bounceBtn", "Bounce") {
			Top = new(-120, 1),
			Left = new(100, hAlign)
		};
		bounceBtn.OnClick += (_, __) => CameraSystem.bounce = !CameraSystem.bounce;
		Append(bounceBtn);

		var pauseBtn = new UIMenuButton(path + "pauseBtn", "Pause") {
			Top = new(-70, 1),
			Left = new(50, hAlign)
		};
		pauseBtn.OnClick += (_, __) => CameraSystem.TogglePause();
		Append(pauseBtn);

		///////

		var showViewBtn = new UIMenuButton(path + "showViewBtn", "Show View Range") {
			Top = new(-120, 1),
			Left = new(180, hAlign)
		};
		showViewBtn.OnClick += ShowViewBtn_OnClick;
		Append(showViewBtn);

		//////

		var bezierBtn = new UIMenuButton(path + "bezierBtn", "Draw Bezier curve") {
			Top = new(-120, 1),
			Left = new(300, hAlign)
		};
		bezierBtn.OnClick += (_, __) => ToggleCurveType(CurveEditUI.CurveType.Bezier);
		Append(bezierBtn);

		var splineBtn = new UIMenuButton(path + "splineBtn", "Draw Spline curve") {
			Top = new(-120, 1),
			Left = new(350, hAlign)
		};
		splineBtn.OnClick += (_, __) => ToggleCurveType(CurveEditUI.CurveType.Spline);
		Append(splineBtn);

		var entityBtn = new UIMenuButton(path + "entityBtn",
			() => $" {(CameraSystem.trackingEntity == null ? "Start" : "Stop")} Tracking Entity") {
			Top = new(-120, 1),
			Left = new(400, hAlign),
			toggleAction = () => CameraSystem.trackingEntity != null // draw frame only while tracking a npc
		};
		entityBtn.OnClick += EntityBtn_OnClick;
		Append(entityBtn);

		//////

		var eraseBtn = new UIMenuButton(path + "eraseBtn", "Erase curve") {
			Top = new(-70, 1),
			Left = new(320, hAlign)
		};
		eraseBtn.OnClick += EraseBtn_OnClick;
		Append(eraseBtn);

		var deleteAllBtn = new UIMenuButton(path + "deleteAllBtn", "Delete all curves") {
			Top = new(-70, 1),
			Left = new(370, hAlign),
			toggleAction = () => false
		};
		deleteAllBtn.OnClick += DeleteAllBtn_OnClick;
		Append(deleteAllBtn);
	}

	private void StartBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (CameraSystem.IsPlaying()) {
			CameraSystem.StopPlaying();
			EditorCameraSystem.CenterToPosition(Main.LocalPlayer.position);
		}
		else {
			CameraSystem.StartPlaying();
		}
	}

	private void DeleteAllBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		UISystem.CurveEditUI.curves.Clear();
		progressBar.Progress = 0;
		CameraSystem.StopPlaying();
	}

	private void EraseBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		UISystem.CurveEditUI.drawingMode = false;
		UISystem.CurveEditUI.erasing = !UISystem.CurveEditUI.erasing;
	}

	private void ShowViewBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		drawView = !drawView;
	}

	private void EntityBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		// select entity if you aren't tracking one
		if (CameraSystem.trackingEntity == null) {
			selectNpcToTrack = !selectNpcToTrack;
			if (selectNpcToTrack) {
				Main.NewText("Click on the enity you want to track");
			}

			return;
		}

		// unselect entity if you are already tracking one
		CameraSystem.trackingEntity = null;
	}

	private static void ToggleCurveType(CurveEditUI.CurveType curveType)
	{
		var instance = UISystem.CurveEditUI;

		// disable drawing mode if already drawing with the same curve type
		if (instance.drawingMode && instance.curveType == curveType) {
			instance.drawingMode = false;
			return;
		}

		instance.erasing = false;
		instance.drawingMode = true;
		instance.curveType = curveType;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (drawView) {
			int sw = Main.screenWidth;
			int sh = Main.screenHeight;
			float z = EditorCameraSystem.zoom;

			Vector2 center = new Vector2(sw, sh) / 2;
			Vector2 offset = new Vector2(sw - sw * z, sh - sh * z) / 2;

			Vector2 pos = CameraSystem.GetPositionAtPercentage(progressBar.Progress);

			var borderRect = new Rectangle(
					(int)((pos.X - Main.screenPosition.X - center.X) * z + offset.X),
					(int)((pos.Y - Main.screenPosition.Y - center.Y) * z + offset.Y),
					(int)(sw * z),
					(int)(sh * z));

			spriteBatch.DrawRectangleBorder(borderRect, 2, Color.Gray);

			spriteBatch.DrawStraightLine(borderRect.Left, borderRect.Top + borderRect.Height / 2, borderRect.Width, 2, Color.Red);
			spriteBatch.DrawStraightLine(borderRect.Left + borderRect.Width / 2, borderRect.Top, 2, borderRect.Height, Color.Red);
		}

		if (selectNpcToTrack) {
			// all NPCs, Items, and Projectiles
			IEnumerable<Entity> enities = Main.npc.AsEnumerable<Entity>().Concat(Main.item).Concat(Main.projectile);

			// loop through all active entities
			foreach (Entity entity in enities.Where(e => e.active)) {
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, (entity.position - Main.screenPosition), entity.Hitbox, Color.Green * .5f, 0, Vector2.Zero, EditorCameraSystem.zoom, SpriteEffects.None, 0);
			}
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (selectNpcToTrack) {
			// all NPCs, Items, and Projectiles
			IEnumerable<Entity> enities = Main.npc.AsEnumerable<Entity>().Concat(Main.item).Concat(Main.projectile);

			// loop through all active entities
			foreach (Entity entity in enities.Where(e => e.active)) {

				// if mouse clicked inside hitbox of the entity
				if (entity.Hitbox.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft) {
					selectNpcToTrack = false; // stop selecting
					CameraSystem.trackingEntity = entity; // start tracking
				}
			}
		}
		base.Update(gameTime);
	}
}
