using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UITools;
using TMPro;

public class Randomize : MonoBehaviour {

    
    public Slider gearCountSlider;
    public Toggle autoUpdate;
    public Toggle RemoveOld;

    int gearCount = 10;

    public UIAnimator uIAnimator;

    GearPlacer gearScript;
    CoordinateManager coordScript;
    List<Gear> gearTypes;

    System.Random rand = new System.Random();

    // Use this for initialization
    void Start () {
        uIAnimator = gameObject.AddComponent<UIAnimator>();
        uIAnimator.SetUp(2f);
        uIAnimator.canvas.alpha = 0f;

        gearScript = GameObject.Find("BottomPlate").GetComponent<GearPlacer>();
        coordScript = GameObject.Find("BottomPlate").GetComponent<CoordinateManager>();
        gearTypes = gearScript.gearTypes;
    }
	

    public bool showSettingsPanel = false;
    public void ToggleSettingsPanel()
    {
        showSettingsPanel = !showSettingsPanel;
        uIAnimator.Show(showSettingsPanel);
        GcodeGenerator code = GameObject.Find("BottomPlate").GetComponent<GcodeGenerator>();
		code.showSettingsPanel = false;
		code.uIAnimator.Show(false);
    }

    public void UpdateGearCount()
    {
        gearCount = (int)gearCountSlider.value;
        gearCountSlider.GetComponentInChildren<TextMeshProUGUI>().text ="GearCount[" + gearCount + "]";
    }

    Gear selectedGearBeforeGenerate = new Gear();
    public void GenerateRandomGearSet()
    {
        gearScript.selectedGear.CloneTo(selectedGearBeforeGenerate);
        List<Gear> spawnedGears = new List<Gear>();

        if (RemoveOld.isOn) gearScript.Reset();
        var xScale = Mathf.RoundToInt(gearScript.transform.localScale.x * 100000);
        var zScale = Mathf.RoundToInt(gearScript.transform.localScale.z * 100000);

        if (gearScript.activeGears.Count == 0)
        {
            int selectedGearIndex = 10;
            int count = 0;
            while (true)
            {
                var index = rand.Next(0, gearTypes.Count);
                if (allowedGears[index].allowed)
                {
                    selectedGearIndex = index;
                    break;
                }
                count++;
                if (count > 100)
                {
                    StopAllCoroutines();
                    return;
                }
            }

            
            spawnedGears.Add(gearScript.SpawnGear(rand.Next(-xScale, xScale) / (100000f * 20f), -10, rand.Next(-zScale, zScale) / (100000f * 20f), selectedGearIndex));
            gearScript.showGearToPlace = false;
        }



        for (int i = 0; i < gearCount - 1; i++)
        {
            int selectedGearIndex = 10;
            int count = 0;
            while (true)
            {
                var index = rand.Next(0, gearTypes.Count);
                if (allowedGears[index].allowed)
                {
                    selectedGearIndex = index;
                    break;
                }
                count++;
                if (count > 100)
                {
                    StopAllCoroutines();
                    return;
                }

            }
            if (selectedGearIndex == 10) return;
            var selectedGear = gearScript.gearTypes[selectedGearIndex];

            for(int recalcs = 0; recalcs < 15; recalcs++)
            {
                var mousePosition = new Vector3(rand.Next(-xScale, xScale) / 100000f, gearScript.placeGearHeight, rand.Next(-zScale, zScale) / 100000f);

                var gearToSnap = gearScript.activeGears[rand.Next(0, gearScript.activeGears.Count)];
                var gearToSnapPos = new Vector3(gearToSnap.position.x, mousePosition.y, gearToSnap.position.z);


                var VectorToCloseGear = (gearToSnapPos - mousePosition).normalized * (gearToSnap.radius + selectedGear.radius);
                var gearPlacePoint = gearToSnapPos - VectorToCloseGear;

                bool needRecalc = false;
                foreach (var gear in gearScript.activeGears)
                {
                    var gearPos = new Vector3(gear.position.x, gearPlacePoint.y, gear.position.z);
                    if (Vector3.Magnitude(gearPos - gearPlacePoint) < gear.radius + selectedGear.radius - 0.0001f)
                    {
                        needRecalc = true;
                    }
                    if(Mathf.Abs(gearPlacePoint.x) >= Mathf.Abs(gearScript.transform.lossyScale.x)/2 || Mathf.Abs(gearPlacePoint.z) >= Mathf.Abs(gearScript.transform.lossyScale.z)/2)
                    { needRecalc = true; }
                }
                if (!needRecalc)
                {

                    spawnedGears.Add(gearScript.SpawnGear(gearPlacePoint.x, -10f, gearPlacePoint.z, selectedGearIndex));
                    gearScript.showGearToPlace = false;
                    break;
                }
            }

        }
        RotationBehaviour.instance.Rotate();
        StartCoroutine(AnimateGears(spawnedGears));
    }


    public float gearDropSpeed;
    IEnumerator AnimateGears(List<Gear> gearList)
    {

        coordScript.UpdateDecimals();
        for (int i = 0; i < gearList.Count; i++)
        {
            if (gearList[i].gameObject == null) goto Foo;
            gearList[i].position = new Vector3(gearList[i].position.x, Camera.main.transform.position.y + i * 2 + 5, gearList[i].position.z);
        }


        while (true)
        {
            for (int i = 0; i < gearList.Count; i++)
            {        
                if (gearList[i].gameObject == null) goto Foo;
                var wantedPosition = new Vector3(gearList[i].position.x, gearScript.placeGearHeight, gearList[i].position.z);
//((camY + 3) * (camY + 3) - gearY * gearY) / 20
//((gearY < camY + 2) ? 10 : 25))
                gearList[i].position = Vector3.MoveTowards(gearList[i].position, wantedPosition, Time.deltaTime * gearDropSpeed * 10);
            }
            bool isDone = true;
            foreach (var gear in gearList)
            {
                if (gear.position.y != gearScript.placeGearHeight) isDone = false; 
            }
            if(isDone) break;
            yield return new WaitForEndOfFrame();
        }
        Foo:
        gearScript.SelectGear(selectedGearBeforeGenerate);
        gearScript.canPlaceGear = false;
        gearScript.showGearToPlace = false;
        yield return null;
    }

    #region allowed Gears
    public List<ToggleInfo> allowedGears;

    [System.Serializable]
    public class ToggleInfo
    {
        public bool allowed = true;
        public Toggle toggle;
    }

    public void UpdateAllowedGears()
    {
        for (int i = 0; i < allowedGears.Count; i++)
        {
            allowedGears[i].allowed = allowedGears[i].toggle.isOn;
        }
        if(autoUpdate.isOn && RemoveOld.isOn)
        {
            GenerateRandomGearSet();
        }
    }
    #endregion


}
