using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UITools;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Crosstales.FB;
using AntColony;
using Unity.Jobs;

public class GcodeGenerator : MonoBehaviour {


#region UI

	[Header("UIReferences")]
	public GameObject GCodeSettings;
	public Slider YOffsetSlider;
	public TextMeshProUGUI startheightText;
	public Slider HoleDepthSlider;
	public TextMeshProUGUI HoleDepthtext;
	public TMP_InputField outputField;
	GearPlacer gearScript;
	DimensionManager dimensionManager;
	public UIAnimator uIAnimator;

	public GameObject LoadingPanel;

	public LineRenderer line;
	public TextMeshProUGUI bestPathText;
	public TextMeshProUGUI logText;

	public void UpdateUI()
	{
		drillDepth = HoleDepthSlider.value;
		HoleDepthtext.text = "[" + String.Format("{0,0:F1}", drillDepth) + "mm]";

		YOffset = YOffsetSlider.value;
		startheightText.text = "[" + String.Format("{0,0:F1}", YOffset) + "mm]";
	}

	public bool showSettingsPanel = false;
    public void ToggleSettingsPanel()
    {
        showSettingsPanel = !showSettingsPanel;
        uIAnimator.Show(showSettingsPanel);
		Randomize rand = GameObject.Find("randomize_Settings").GetComponent<Randomize>();
		rand.showSettingsPanel = false;
		rand.uIAnimator.Show(false);
		ShowPath(showSettingsPanel);
    }
	public void ShowSettingsPanel()
	{
		showSettingsPanel = true;
        uIAnimator.Show(true);
		Randomize rand = GameObject.Find("randomize_Settings").GetComponent<Randomize>();
		rand.showSettingsPanel = false;
		rand.uIAnimator.Show(false);
		ShowPath(true);
	}





#endregion
	
	[Header("Variables")]
	public float drillDepth;
	public float YOffset;

	void Awake()
	{
		gearScript = transform.GetComponent<GearPlacer>();
		dimensionManager = transform.GetComponent<DimensionManager>();
		uIAnimator = GCodeSettings.AddComponent<UIAnimator>();
		colonyJob = gameObject.AddComponent<AntColonyProgram>();
	}
	void Start () {
		uIAnimator.SetUp(3);
		UpdateUI();
		LoadingPanel.SetActive(false);

		colonyJob.LoadReferenses(bestPathText, logText, line);
	}


	private void ShowPath(bool show)
	{
		List<Gear> gears = gearScript.activeGears;
		Vector3[] positions = new Vector3[gears.Count];

		float length = 0;
		for (int i = 0; i < gears.Count; i++)
		{
			positions[i] = gears[i].position;
			positions[i].y = 1.3f;
			if(i != 0) length += (positions[i] - positions[i - 1]).magnitude;
		}
		line.positionCount = gears.Count;
		line.SetPositions(positions);
		line.gameObject.SetActive(show);
		bestPathText.text = "Path: " + Mathf.RoundToInt(length).ToString() + " cm";
	}
	public void TogglePath()
	{
		ShowPath(!line.gameObject.activeInHierarchy);
	}

	string Gcode;
	public void GenerateGCode()
	{
		StopAllCoroutines();
		StartCoroutine(GenerateCode());
	}


	AntColonyProgram colonyJob;
	public IEnumerator GenerateCode()
	{
		List<Gear> gears = gearScript.activeGears;
		int count = gears.Count;
		if(count == 0) yield break;;
		if(count < 4) {
			Vector3 zero = new Vector3(dimensionManager.size.x, gears[0].position.y, dimensionManager.size.y);
			gears.OrderByDescending(x => (x.position - zero).magnitude);
		}
		else
		{
			LoadingPanel.SetActive(true);
			
			colonyJob.startCalculation(gears);
			while(!colonyJob.calcDone)
			{
				yield return new WaitForEndOfFrame();
			}
			int[] trail = colonyJob.trail;
			List<Gear> temp = new List<Gear>();

			for (int i = 0; i < trail.Length; i++)	
				temp.Add(gears[trail[i]]);

			gearScript.activeGears = gears = temp;
			CoordinateManager.instance.UpdateText();
			LoadingPanel.SetActive(false);
		}

		

		string newLine = Environment.NewLine;
		Gcode = string.Format("%{0}O001{0}", newLine); 											//Start Program
		Gcode += string.Format("G90{0}", newLine);												//Absolute coordinate system
		Gcode += string.Format("G21{0}", newLine);												//Millimeter input
		Gcode += string.Format("G0X{0}Y{1}Z{2}{3}",gears[0].position.x, gears[0].position.y, YOffset ,newLine);								//Move to origo + Yoffset
		Gcode += string.Format("F300.0S6000M03{0}{0}", newLine);								//Start spindle 6000rpm and 300mm/min

		
		int gearCount = 0;
		foreach (var gear in gears)
		{
			var position = (gear.position + transform.lossyScale/2) * 10; //To millimeter
			Gcode += string.Format("(Gear: " + gearCount++ + "){0}", newLine);
			Gcode += string.Format("G0X" + String.Format("{0,0:F4}", position.x) + "Y" + String.Format("{0,0:F4}", position.z) + "{0}", newLine); //Move to gear.position
			Gcode += string.Format("G01Z" + String.Format("{0,0:F1}", (-drillDepth)) + "{0}G01Z" + String.Format("{0,0:F1}", YOffset) + "{0}{0}", newLine); //Drill hole
		}
		Gcode += string.Format("G0X152.50Y305.00Z81.51{0}M05{0}M02{0}%{0}", newLine); //Move to viewPosition / Stop spindle / End of program

		outputField.text = Gcode;
	}

	public void CopyCode()
	{
		var path = FileBrowser.SaveFile("Save", Environment.SpecialFolder.Desktop.ToString(), "GearEdit", "txt");
        if(path != "")
        {
            StreamWriter writer = new StreamWriter(path);
		    writer.Write(Gcode);
		    writer.Close();
			transform.GetComponent<CoordinateManager>().copiedAnimator.Blink();
        }
	}

	public void WriteToDesktop()
	{
		string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + @"\GCode.txt";

		StreamWriter writer = new StreamWriter(path);
		writer.Write(Gcode);
		writer.Close();
		transform.GetComponent<CoordinateManager>().copiedAnimator.Blink();
	}
}
