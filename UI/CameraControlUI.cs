using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using CameraControl.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;

namespace CameraControl.UI;

internal class CameraControlUI : UIState
{
	public static bool HoveringOverButton { get; private set; }
	public UIProgressbar progressBar; // progressbar instance

	private bool drawView; // if true, draws big box representing the screen at a tracking location with 100% zoom
	private bool selectNpcToTrack; // if true, draws green boxes around npcs

	private const float hAlign = 0.35f; // position of the first button
	private const string path = "CameraControl/UI/Assets/"; // common path for all UI Assets

	public CameraControlUI()
	{
		progressBar = new UIProgressbar()
		{
			Top = new(-150, 1),
			Left = new(0, hAlign),
			Width = new(450, 0),
			Height = new(20, 0)
		};
		Append(progressBar);

		var startBtn = new UIMenuButton(
			() => $"{path + (CameraSystem.Playing ? "stopBtn" : "playBtn")}",
			() => $"{(CameraSystem.Playing ? "Stop" : "Start")} Tracking")
		{
			Top = new(-120, 1),
			Left = new(0, hAlign)
		};
		startBtn.toggleAction = () => CameraSystem.Playing; // draw frame only while tracking the curve
		startBtn.OnClick += (_, __) => CameraSystem.Playing = !CameraSystem.Playing;
		startBtn.OnMouseOver += StartHoveringButton;
		startBtn.OnMouseOut += StopHoveringButton;
		Append(startBtn);

		var repeatBtn = new UIMenuButton(path + "repeatBtn", "Repeat")
		{
			Top = new(-120, 1),
			Left = new(50, hAlign)
		};
		repeatBtn.OnClick += (_, __) => CameraSystem.repeat = !CameraSystem.repeat;
		repeatBtn.OnMouseOver += StartHoveringButton;
		repeatBtn.OnMouseOut += StopHoveringButton;
		Append(repeatBtn);

		var bounceBtn = new UIMenuButton(path + "bounceBtn", "Bounce")
		{
			Top = new(-120, 1),
			Left = new(100, hAlign)
		};
		bounceBtn.OnClick += (_, __) => CameraSystem.bounce = !CameraSystem.bounce;
		bounceBtn.OnMouseOver += StartHoveringButton;
		bounceBtn.OnMouseOut += StopHoveringButton;
		Append(bounceBtn);

		///////

		var showViewBtn = new UIMenuButton(path + "showViewBtn", "Show View Range")
		{
			Top = new(-120, 1),
			Left = new(180, hAlign)
		};
		showViewBtn.OnClick += ShowViewBtn_OnClick;
		showViewBtn.OnMouseOver += StartHoveringButton;
		showViewBtn.OnMouseOut += StopHoveringButton;
		Append(showViewBtn);

		//////

		var bezierBtn = new UIMenuButton(path + "bezierBtn", "Draw Bezier curve")
		{
			Top = new(-120, 1),
			Left = new(300, hAlign)
		};
		bezierBtn.OnClick += (_, __) => ToggleCurveType(CurveEditUI.CurveType.Bezier);
		bezierBtn.OnMouseOver += StartHoveringButton;
		bezierBtn.OnMouseOut += StopHoveringButton;
		Append(bezierBtn);

		var splineBtn = new UIMenuButton(path + "splineBtn", "Draw Spline curve")
		{
			Top = new(-120, 1),
			Left = new(350, hAlign)
		};
		splineBtn.OnClick += (_, __) => ToggleCurveType(CurveEditUI.CurveType.Spline);
		splineBtn.OnMouseOver += StartHoveringButton;
		splineBtn.OnMouseOut += StopHoveringButton;
		Append(splineBtn);

		var entityBtn = new UIMenuButton(path + "entityBtn",
			() => $" {(CameraSystem.trackingEntity == null ? "Start" : "Stop")} Tracking Entity")
		{
			Top = new(-120, 1),
			Left = new(400, hAlign)
		};
		entityBtn.toggleAction = () => CameraSystem.trackingEntity != null; // draw frame only while tracking a npc
		entityBtn.OnClick += EntityBtn_OnClick;
		entityBtn.OnMouseOver += StartHoveringButton;
		entityBtn.OnMouseOut += StopHoveringButton;
		Append(entityBtn);

		//////

		var eraseBtn = new UIMenuButton(path + "eraseBtn", "Erase curve")
		{
			Top = new(-70, 1),
			Left = new(320, hAlign)
		};
		eraseBtn.OnClick += EraseBtn_OnClick;
		eraseBtn.OnMouseOver += StartHoveringButton;
		eraseBtn.OnMouseOut += StopHoveringButton;
		Append(eraseBtn);

		var deleteAllBtn = new UIMenuButton(path + "deleteAllBtn", "Delete all curves")
		{
			Top = new(-70, 1),
			Left = new(370, hAlign)
		};
		deleteAllBtn.toggleAction = () => false;
		deleteAllBtn.OnClick += DeleteAllBtn_OnClick;
		deleteAllBtn.OnMouseOver += StartHoveringButton;
		deleteAllBtn.OnMouseOut += StopHoveringButton;
		Append(deleteAllBtn);
	}

	private void StopHoveringButton(UIMouseEvent evt, UIElement listeningElement) => HoveringOverButton = false;
	private void StartHoveringButton(UIMouseEvent evt, UIElement listeningElement) => HoveringOverButton = true;

	private void DeleteAllBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		UISystem.CurveEditUI.curves.Clear();
		progressBar.Progress = 0;
		CameraSystem.Playing = false;
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
		if (CameraSystem.trackingEntity == null)
		{
			Main.NewText("Click on the enity you want to track");
			selectNpcToTrack = true;

			return;
		}

		// unselect entity if you are already tracking one
		CameraSystem.trackingEntity = null;
	}

	private static void ToggleCurveType(CurveEditUI.CurveType curveType)
	{
		var instance = UISystem.CurveEditUI;

		// disable drawing mode if already drawing with the same curve type
		if (instance.drawingMode && instance.curveType == curveType)
		{
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

		if (drawView)
		{
			// TODO: make preview border

			Vector2 center = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
			Vector2 pos = CameraSystem.GetPositionAtPercentage(progressBar.Progress) - center * EditorCameraSystem.zoom;

			spriteBatch.DrawRectangleBorder(new Rectangle(
					(int)(pos.X - Main.screenPosition.X),
					(int)(pos.Y - Main.screenPosition.Y),
					(int)(Main.screenWidth * EditorCameraSystem.zoom),
					(int)(Main.screenHeight * EditorCameraSystem.zoom)),
				2, Color.Gray);
		}

		if (selectNpcToTrack)
		{
			var enities = Main.npc.AsEnumerable<Entity>().Concat(Main.item).Concat(Main.projectile);
			foreach (Entity entity in enities.Where(e => e.active))
			{
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, (entity.position - Main.screenPosition), entity.Hitbox, Color.Green * .5f, 0, Vector2.Zero, EditorCameraSystem.zoom, SpriteEffects.None, 0);
			}
		}
	}

	public override void Update(GameTime gameTime)
	{
		if (selectNpcToTrack)
		{
			var enities = Main.npc.AsEnumerable<Entity>().Concat(Main.item).Concat(Main.projectile);
			foreach (Entity entity in enities.Where(e => e.active))
			{
				if (entity.Hitbox.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft)
				{
					selectNpcToTrack = false;
					CameraSystem.trackingEntity = entity;
				}
			}
		}
		base.Update(gameTime);
	}
}
