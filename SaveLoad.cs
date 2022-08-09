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

namespace CameraControl;

internal class SaveLoad
{
	private class SaveData
	{
		public CurveData[] curves { get; set; }
		public Dictionary<float, float> keyframes { get; set; }
	}

	private struct CurveData
	{
		public string curveType { get; set; }
		public Vector2 c0 { get; set; }
		public Vector2 c1 { get; set; }
		public Vector2 c2 { get; set; }
		public Vector2 c3 { get; set; }
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
		}).Replace("  ", "\t");	// use tabs as indentaion

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
		if (path != null) {
			string json = File.ReadAllText(path, Encoding.Unicode);

			SaveData saveData = JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions() {
				IncludeFields = true	// deserialize the X and Y fields of a Vector2
			});

			// set keyframe data
			UISystem.CameraControlUI.progressBar.keyframes = saveData.keyframes;

			// set curve data
			UISystem.CurveEditUI.curves.Clear();
			foreach (var curve in saveData.curves) {
				switch (curve.curveType) {
					case "Bezier":
						UISystem.CurveEditUI.curves.Add(new BezierCurve(curve.c0, curve.c1, curve.c2, curve.c3));
						break;
					case "Spline":
						UISystem.CurveEditUI.curves.Add(new SplineCurve(curve.c0, curve.c1, curve.c2, curve.c3));
						break;
				}
			}
		}
	}

	private static CurveData[] CurveToDataFormat(List<UI.Elements.Curves.Curve> curves)
	{
		CurveData[] data = curves.Select(curve => new CurveData() {
			curveType = curve is BezierCurve ? "Bezier" : "Spline",
			c0 = curve.controls[0],
			c1 = curve.controls[1],
			c2 = curve.controls[2],
			c3 = curve.controls[3]
		}).ToArray();

		return data;
	}
}
