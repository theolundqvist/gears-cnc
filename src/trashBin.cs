using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrashBin : MonoBehaviour {

    [Header("trashBin")]
    public Transform trashCan;

    [HideInInspector]
    public Vector3 startPosition;
    public Vector3 trashBinOffset;
    public float trashBinSpeed;
	// Use this for initialization
	void Start () {
		startPosition = trashCan.position;
		trashCan.gameObject.SetActive(false);
		Setup();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void show(){StartCoroutine(Show());}
	IEnumerator Show()
    {
		trashCan.gameObject.SetActive(true);
        while(trashCan.position != startPosition + trashBinOffset)
        {
            trashCan.position = Vector3.MoveTowards(trashCan.position, startPosition + trashBinOffset, Time.deltaTime * trashBinSpeed);
            yield return new WaitForEndOfFrame();
        }
    }

	public void hide(){StopAllCoroutines(); StartCoroutine(Hide());}
    IEnumerator Hide()
    {
		while(trashCan.position != startPosition)
		{
			trashCan.position = Vector3.MoveTowards(trashCan.position, startPosition, Time.deltaTime * trashBinSpeed);
			yield return new WaitForEndOfFrame();
		}
		trashCan.gameObject.SetActive(false);
    }

	public void Setup()
	{
		startPosition = new Vector3(transform.lossyScale.x/2, startPosition.y, transform.lossyScale.z/2);

	}
}
