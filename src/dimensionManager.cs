using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class DimensionManager : MonoBehaviour
{

    public TMP_InputField xValue;
    public TMP_InputField zValue;
    public Toggle isCircleToggle;
    public GameObject CircleMesh;

    Mesh cubeMesh;

    public Vector2 size {
        get{
            Vector2 Size = new Vector2();
            float.TryParse(xValue.text, out Size.x);
            float.TryParse(zValue.text, out Size.y);
            return Size;
        }
    }
    public float dimensionChangeSpeed;

    GearPlacer gearScript;
    CoordinateManager coordScript;
    TrashBin trashBin;


    private void Start()
    {
        cubeMesh = gameObject.GetComponent<MeshFilter>().mesh;
        trashBin = GameObject.Find("BottomPlate").GetComponent<TrashBin>();
        gearScript = GameObject.Find("BottomPlate").GetComponent<GearPlacer>();
        coordScript = GameObject.Find("BottomPlate").GetComponent<CoordinateManager>();
    }


    string storedCoordinates;
    /// <summary>
    /// Queues a change of dimensions
    /// </summary>
    public void SetDimension()
    {
        if (isChanging)
        {
            queueChange = true;
            return;
        }
        isChanging = true;
        int x;
        int z;
        int.TryParse(xValue.text, out x);
        int.TryParse(zValue.text, out z);
        if (x < 5 || z < 5) { isChanging = false; return; }

        storedCoordinates = coordScript.text;
        gearScript.Reset();
        StartCoroutine(changeScale(new Vector3(Mathf.Clamp(x, 5, 100), 1, Mathf.Clamp(z, 5, 100))));
    }

    private void Update()
    {
        if (queueChange && !isChanging)
        {
            SetDimension();
            queueChange = false;
        }
    }

    bool isChanging = false;
    bool queueChange = false;


    /// <summary>
    /// Smooth interpolation of scale and camera position
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    IEnumerator changeScale(Vector3 scale)
    {
        var selectedGearAtStart = gearScript.selectedGear;
        //Camera
        var y = (scale.x > scale.z) ? scale.x : scale.z;
        var speed = Mathf.Abs(Vector3.Magnitude(scale - transform.localScale));

        while(true)
        {
            var wantedPosition = new Vector3(0, Mathf.Clamp(y, 2, 100), 0);
            var position = Camera.main.transform.position;
            Camera.main.transform.position = Vector3.MoveTowards(position, wantedPosition, Time.deltaTime * dimensionChangeSpeed * speed);

            //Scale
            transform.localScale = Vector3.MoveTowards(transform.localScale, scale, Time.deltaTime * dimensionChangeSpeed * speed);



            if (transform.localScale == scale) break;
            yield return new WaitForEndOfFrame();
        }
        
        isChanging = false;
        gearScript.SelectGear(selectedGearAtStart);
        gearScript.showGearToPlace = false;

        coordScript.text = storedCoordinates;
        coordScript.PlaceGearsFromCoordinates(storedCoordinates);

        trashBin.Setup();
    }

    public void SetMesh()
    {
        if(isCircleToggle.isOn) changeToCircle();
        else changeToCube();
        trashBin.hide();
    }

    void changeToCube()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh = cubeMesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.position -= Vector3.down * 0.5f;
    }
    void changeToCircle()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh = CircleMesh.GetComponent<MeshFilter>().mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.position += Vector3.down * 0.5f;
    }
}
