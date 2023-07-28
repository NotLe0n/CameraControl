using CameraControl.UI.Elements.Curves;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Terraria;
using Terraria.Utilities.FileBrowser;
using Curve = CameraControl.UI.Elements.Curves.Curve;

namespace CameraControl;

internal class SaveLoad
{
	private struct SaveData
	{
		public IEnumerable<CurveData> curves;
		public Dictionary<float, float> keyframes;
	}

	private struct CurveData
	{
		public string curveType;
		public Vector2 c0;
		public Vector2 c1;
		public Vector2 c2;
		public Vector2 c3;
	}

	public static void SaveCurveData()
	{
		var curves = UISystem.CurveEditUI.curves;
		var keyframes = UISystem.CameraControlUI.progressBar.keyframes;

		// create SaveData object
		var data = new SaveData() {
			curves = CurveToDataFormat(curves),
			keyframes = keyframes
		};

		// serialize data to json string
		string json = JsonSerializer.Serialize(data, new JsonSerializerOptions() { 
			WriteIndented = true,	// make it pretty
			IncludeFields = true	// to serialize the X and Y fields of a Vector2
		}).Replace("  ", "\t");	// use tabs as indentation

		// create CameraControlData directory
		var directory = Directory.CreateDirectory(Path.Combine(Main.SavePath, "CameraControlData"));

		// create file
		using FileStream stream = File.Create(Path.Combine(directory.FullName, DateTime.Now.ToString("ddMMyy-HHmmss")) + ".json");

		// write to file
		stream.Write(json.ToByteArray());

		// close file
		stream.Close();
	}

	public static void LoadCurveData()
	{
		string path = FileBrowser.OpenFilePanel("Select Data file", "json");
		if (string.IsNullOrEmpty(path)) {
			return;
		}

		string json = File.ReadAllText(path, Encoding.Unicode);

		SaveData saveData = JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions() {
			IncludeFields = true  // deserialize the X and Y fields of a Vector2
		});

		// set keyframe data
		UISystem.CameraControlUI.progressBar.keyframes = saveData.keyframes;
		
		// set curve data
		UISystem.CurveEditUI.curves.Clear();
		foreach (var curve in saveData.curves) {
			var curvePoints = new[] { curve.c0, curve.c1, curve.c2, curve.c3 };
			
			Curve newCurve = null;
			switch (curve.curveType) {
				case "Bezier":
					newCurve = new SplineCurve(curvePoints);
					break;
				case "Spline":
					newCurve = new SplineCurve(curvePoints);
					break;
			}

			UISystem.CurveEditUI.curves.Add(newCurve);
		}
	}

	private static IEnumerable<CurveData> CurveToDataFormat(IEnumerable<Curve> curves)
	{
		var data = curves.Select(curve => new CurveData {
			curveType = curve is BezierCurve ? "Bezier" : "Spline",
			c0 = curve.controls[0],
			c1 = curve.controls[1],
			c2 = curve.controls[2],
			c3 = curve.controls[3]
		});

		return data;
	}
}
