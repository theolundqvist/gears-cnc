using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;
using UITools;
using System.IO;
using Crosstales.FB;

public class CoordinateManager : MonoBehaviour {


    [HideInInspector]
    public static CoordinateManager instance;

    [Header("Settings")]
    public float messageFadeSpeed = 1;
    public int coordDecimals = 3;

    [Header("Referenses")]
    public GearPlacer gearPlacerScript;
    public GameObject textField;
    public GameObject copied;
    [HideInInspector]
    public UIAnimator copiedAnimator;
    public GameObject pasted;
    [HideInInspector]
    public UIAnimator pastedAnimator;
    public GameObject sliderObject;


    private TMP_InputField textComponent;
    public string text{get{return textComponent.text;} set{textComponent.text = value;}}


    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {
        copiedAnimator = copied.AddComponent<UIAnimator>();
        copiedAnimator.SetUp(messageFadeSpeed);
        pastedAnimator = pasted.AddComponent<UIAnimator>();
        pastedAnimator.SetUp(messageFadeSpeed);


        textComponent = textField.GetComponent<TMP_InputField>();
        UpdateDecimals();
	}
	


    private int gearsPlacedLastUpdate = 0;
	// Update is called once per frame
	void Update () {

        //If the number of gears has changed
		if(gearsPlacedLastUpdate != gearPlacerScript.activeGears.Count)
        {
            gearsPlacedLastUpdate = gearPlacerScript.activeGears.Count;

            UpdateText();
        }
	}


    /// <summary>
    /// Formats string accordingly and assigns it
    /// </summary>
    public void UpdateText()
    {
        string _text = "#[GearPlacementInfo]\n";
        for (int i = gearPlacerScript.activeGears.Count - 1; i >= 0; i--)
        {
            var pos = gearPlacerScript.activeGears[i].position + transform.lossyScale/2;

            var gearType = gearPlacerScript.activeGears[i].prefab;
            int gearTypeIndex = 0;

            for (int x = 0; x < gearPlacerScript.gearTypes.Count; x++)
            {
                if (gearType == gearPlacerScript.gearTypes[x].prefab) gearTypeIndex = x;

            }
            //_text += new Vector2(pos.x, pos.z).ToString() + gearPlacerScript.gearTypes.


            float posX = (float)System.Math.Round(pos.x, coordDecimals);
            float posZ = (float)System.Math.Round(pos.z, coordDecimals);


            _text += new Vector2(posX, posZ).ToString("R")  + " : Type[" + gearTypeIndex.ToString() + "]\n";
             //(gearPlacerScript.activeGears[i].prefab) ;
        }
        text = _text;
    }

    /// <summary>
    /// Update the text according to number of decimals
    /// </summary>
    public void UpdateDecimals()
    {
        int value = (int)sliderObject.GetComponent<Slider>().value;

        var DecimalText = GameObject.Find("DecimalText");
        var _text = DecimalText.GetComponent<TextMeshProUGUI>();

        _text.text = "Decimals[" + value + "]";
        coordDecimals = value;
        UpdateText();
    }

    /// <summary>
    /// Copies coordinates to clipboard
    /// </summary>
    public void SaveToFile()
    {
        var path = FileBrowser.SaveFile("Save", Environment.SpecialFolder.Desktop.ToString(), "GearEdit", "txt");
        if(path != "")
        {
            StreamWriter writer = new StreamWriter(path);
		    writer.Write(text);
		    writer.Close();
            copiedAnimator.Blink();
        }
    }





    /// <summary>
    /// Locates gear positions and types from clipboard text
    /// </summary>
    public void LoadFromFile()
    {
        string[] path = FileBrowser.OpenFiles("Open", Environment.SpecialFolder.Desktop.ToString(), "txt", false);
        if(path.Length == 0) return;

        StreamReader reader = new StreamReader(path[0]);
        string _text = reader.ReadToEnd();
        reader.Close();

        PlaceGearsFromCoordinates(_text);

        pastedAnimator.Blink();
    }


    public void Reload()
    {
        PlaceGearsFromCoordinates(text);
    }

    public void PlaceGearsFromCoordinates(string _text)
    {

        GearPlacer gearScript = transform.GetComponent<GearPlacer>();

        gearScript.Reset();

        if (_text.StartsWith("#[GearPlacementInfo]\n"))
        {

            float xCoord;
            float zCoord;
            int gearType;

            _text = _text.Remove(0, 20);

            string[] lines = _text.Split(']');

            var halfscale = gearScript.gameObject.transform.localScale/2;
            for (int i = 0; i < lines.Length - 1; i++)
            {

                int.TryParse(lines[i][lines[i].Length - 1].ToString(), out gearType);
                lines[i] = lines[i].Remove(lines[i].Length - 10, 10);
                lines[i] = lines[i].Remove(0, 2);


                string[] Coords = lines[i].Split(',', '(', ')');

                float.TryParse(Coords[0], out xCoord);


                float.TryParse(Coords[1], out zCoord);

                gearPlacerScript.SpawnGear(xCoord - halfscale.x, 0, zCoord - halfscale.z, gearType);
                gearPlacerScript.showGearToPlace = false;
            }
            gearPlacerScript.activeGears.Reverse();
        }
        UpdateText();
        RotationBehaviour.instance.Rotate();
    }
}
