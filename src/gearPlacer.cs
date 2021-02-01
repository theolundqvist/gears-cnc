using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearPlacer : MonoBehaviour {

    //[TODO]
    //* exampleGear Drops down and places

    public GameObject pausePanel;

    [Header("GearSettings")]
    public float placeGearHeight = 0;   //Y value of gears
    public float pickedupGearHeight = 5;
    public float gearSnapDistance = 1f;
    public List<Gear> gearTypes = new List<Gear>();     //Contains prefabs of gears

    [HideInInspector]
    public List<Gear> activeGears = new List<Gear>();   //All currently active gears
    [HideInInspector]
    public Gear selectedGear;   //The gear to place

    TrashBin trashCan;


    void Awake()
    {
        trashCan = GameObject.Find("BottomPlate").GetComponent<TrashBin>();
    }
    void Start () {
        SelectGear(gearTypes.Count - 1);
        showGearToPlace = false;
        exampleGear.transform.position = new Vector3(0, -10, 0);
        //showGearToPlace = false;
        //exampleGear.transform.position = new Vector3(0, -10, 0);
        Resume();
	}


    [HideInInspector]
    public bool showGearToPlace = false;
    Vector3 placeGearPoint;
    GameObject exampleGear;
    float exampleGearSplitRatio;
    bool showPausePanel = true;
    public bool canPlaceGear = false;

    #region PausePanelMethods
    public void Pause()
    {
        pausePanel.SetActive(true);
        showPausePanel = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        showPausePanel = false;
    }

    public void Exit()
    {
        Application.Quit();
    }
    #endregion

    // Update is called once per frame
    void Update () {

        if(!canPlaceGear && !showGearToPlace)
        {
            exampleGear.transform.position = new Vector3(0, -10, 0);
        }
        //Toggle pausepanel on esc
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (showPausePanel) Resume();
            else Pause();
        }

        //Dont place gears if paused
        if (showPausePanel) return;



        if(Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            if(!Physics.Raycast(exampleGear.transform.position, Vector3.down, out hit)) return;

            if(hit.collider.gameObject.name == "trashbin")
            {
                canPlaceGear = false;
                showGearToPlace = false;
                exampleGear.transform.position = new Vector3(0, -10, 0);
            }

            if(showGearToPlace && canPlaceGear)
            {
                Gear gear = SpawnGear(placeGearPoint);
                activeGears.Add(gear);
                showGearToPlace = false;
                canPlaceGear = false;
                RotationBehaviour.instance.Rotate();
            }
            trashCan.hide();
        }

        if(Input.GetMouseButton(0))
        {

        }
		if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            exampleGear.SetActive(false);
            //Only place gear if mouse is over wood
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                canPlaceGear = false;
                exampleGear.SetActive(true);
                exampleGear.transform.position = new Vector3(0, -10, 0);
                return;
            }

            exampleGear.SetActive(true);

            if(!showGearToPlace)
            {
                for (int i = 0; i < activeGears.Count; i++)
                {
                    if((activeGears[i].position - hit.point).magnitude < activeGears[i].radius)
                    {
                        SelectGear(activeGears[i]);
                        removeGear(i);
                        showGearToPlace = true;
                        trashCan.show();
                    }
                }
                return;
            }

            //If gear can be placed, instantiate new gear
            if(canPlaceGear)
            {
                Gear gear = SpawnGear(placeGearPoint);
                activeGears.Add(gear);
                showGearToPlace = false;
                canPlaceGear = false;
                RotationBehaviour.instance.Rotate();
            }

        }
        CalculateGearPlacePoint();
	}

    void removeGear(int index)
    {
        Destroy(activeGears[index].gameObject);
        activeGears.RemoveAt(index);
    }


    void CalculateGearPlacePoint()
    {
        if (showGearToPlace && exampleGear.gameObject != null)
        {
            RaycastHit hit;
            exampleGear.SetActive(false);
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                exampleGear.transform.position = new Vector3(0, -10, 0);
                exampleGear.SetActive(true);
                return;
            }
            exampleGear.SetActive(true);
            var hitPoint = new Vector3(hit.point.x, placeGearHeight, hit.point.z);

            if (activeGears.Count == 0)
            {
                exampleGear.transform.position = hitPoint;
                placeGearPoint = hitPoint;
                canPlaceGear = true;
                return;
            }

            //Snap exampleGear to closest gear

            Gear closestGear = new Gear();

            float lowestDistance = 1000;
            foreach (var gear in activeGears)
            {
                float distance = Vector3.Magnitude(gear.position - hitPoint) - gear.radius;

                if (distance < lowestDistance)
                {
                    closestGear = gear;
                    lowestDistance = distance;
                }
            }
            var VectorToCloseGear = (closestGear.position - hitPoint).normalized * (closestGear.radius + selectedGear.radius);



            if (Vector3.Magnitude(hitPoint - closestGear.position) < closestGear.radius) return;


            var useHitPoint = true;
            foreach (var gear in activeGears)
            {
                //Physics.ClosestPoint()
                var magnitudeToGear = gear.radius + selectedGear.radius;

                if (Vector3.Magnitude(gear.position - exampleGear.transform.position) < magnitudeToGear - 0.005f)
                {
                    canPlaceGear = false;
                }
                if (Vector3.Magnitude(gear.position - exampleGear.transform.position) < magnitudeToGear - 0.0001f)
                {

                    if ((Vector3.Magnitude(gear.position - hitPoint) < magnitudeToGear - 0.002f))
                        return;
                }

                if(Vector3.Magnitude(gear.position - hitPoint) < magnitudeToGear + gearSnapDistance)
                {
                    useHitPoint = false;
                }
            }
            canPlaceGear = true;

            placeGearPoint = (useHitPoint) ? hitPoint : closestGear.position - VectorToCloseGear;
            
            placeGearPoint.y = placeGearHeight;
            exampleGear.transform.position = placeGearPoint;
            if(!useHitPoint)
            {
                exampleGear.transform.rotation = Quaternion.Euler(CalculateRotation(exampleGear.transform.position, closestGear, exampleGearSplitRatio));   
            }
            //else exampleGear.transform.rotation = Quaternion.Euler(-90, 0, 0);

        }
    }

    public Vector3 CalculateRotation(Vector3 gearPos, Gear referensGear, float splitRatio)
    {
        var closePos = referensGear.position;    closePos.y = 2;    gearPos.y = 2;

        var VectorBetween =  gearPos - closePos;
        var closeGearForward = closePos + referensGear.forward;

        float angle = Vector3.SignedAngle(VectorBetween, closeGearForward - closePos, Vector3.up) + 360;
        if(angle>360)angle-=360;
        Debug.DrawLine(closePos, gearPos);
        Debug.DrawLine(closePos, closeGearForward);
        
        //Debug.Log(angle);
        var gearAngle = (splitRatio / referensGear.splitRatio) * angle + angle;                                        
        //Debug.Log(referensGear.rotation.y - gearAngle);
        gearAngle += splitRatio/2;           //adding 0.5 teeth     //Number of teeth traversed multiplied by splitratio                                            //closestGear is round therefore we must add angle again            
        
        if(gearAngle>360)gearAngle-=(gearAngle-(gearAngle%360));
        //Debug.Log(gearAngle);
        return referensGear.rotation - new Vector3(0, 0, gearAngle + 180);
    }


    #region Spawn functions
    /// <summary>
    /// Instantiates gear
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Gear SpawnGear(Vector3 position)
    {
        Gear gear = new Gear();
        selectedGear.CloneTo(gear);
        gear.gameObject = Instantiate(gear.prefab, position, exampleGear.transform.rotation);
        gear.gameObject.transform.parent = GameObject.Find("ground").transform;

        return gear;
    }

    /// <summary>
    /// Adds a gear to the list and instantiates it. Y value of 0 will be set to gearPlaceHeight
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="gearTypeIndex"></param>
    public Gear SpawnGear(float x, float y, float z, int gearTypeIndex)
    {
        SelectGear(gearTypeIndex);

        var position = new Vector3(x, placeGearHeight, z);
        Gear gear = new Gear();
        selectedGear.CloneTo(gear);
        gear.gameObject = Instantiate(selectedGear.prefab, position, exampleGear.transform.rotation);
        gear.gameObject.transform.parent = GameObject.Find("ground").transform;

        activeGears.Add(gear);
        return gear;
    }
    #endregion


    /// <summary>
    /// Selects indexed gearType and configures exampleGear accoringly
    /// </summary>
    /// <param name="index"></param>

    public void SelectGear(Gear gear)
    {
        Destroy(exampleGear);
        gear.CloneTo(selectedGear);
        exampleGear = Instantiate(selectedGear.prefab);
        exampleGear.transform.parent = GameObject.Find("ground").transform;
        exampleGear.name = "ExampleGear";
        exampleGearSplitRatio = gear.splitRatio;

        if (activeGears.Count == 0) { }

        showGearToPlace = true;
    }

    public void SelectGear(int index)
    {
        if (index >= gearTypes.Count)
            return;

        gearTypes[index].CloneTo(selectedGear);
        SelectGear(selectedGear); 
    }

    /// <summary>
    /// Removes all active gears
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < activeGears.Count; i++)
        {
            Destroy(activeGears[i].gameObject);
        }
        activeGears.Clear();
    }
}