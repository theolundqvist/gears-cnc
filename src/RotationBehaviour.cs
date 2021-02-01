using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class RotationBehaviour : MonoBehaviour {
	[HideInInspector]
	public static RotationBehaviour instance;

	public float RPM = 1;
	public GearPlacer gearPlacer;

	public enum Directions{left, right, none};


	// Use this for initialization
	void Awake () {
		instance = this;
	}
	

	Gear StartingGear;

	List<Gear> gears = new List<Gear>();

	public void Rotate()
	{
		gears = gearPlacer.activeGears;
		for (int i = 0; i < gears.Count; i++)
		{
			gears[i].direction = Directions.none;
			gears[i].color = Color.white;
		}
		StartingGear = gears[0];
		StartingGear.direction = Directions.right;

		CalculationSuccessful = true;
		recursions = 0;
		
		CalculateRotationPointers(0);
		StopAllCoroutines();
		if(CalculationSuccessful) StartCoroutine(RotateGears());
	}

	public bool CalculationSuccessful = true;
	int recursions = 0;
	void CalculateRotationPointers(int refIndex)
	{
		if(recursions++ > gears.Count * 5) return;
		if(refIndex >= gears.Count) return;
		for (int i = 0; i < gears.Count; i++)
		{
			if(i == refIndex) i++; if(i >= gears.Count) return;

			if((gears[refIndex].position - gears[i].position).magnitude < gears[refIndex].radius + gears[i].radius + 0.25f)
			{
				if(gears[i].direction != Directions.none)
				{
					var dir = gears[i].direction;
					gears[i].direction = TransformDirection(gears[refIndex]);

					if(dir != gears[i].direction)
					{
						gears[refIndex].gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
						gears[i].gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
						gears[i].direction = Directions.none;
						gears[refIndex].direction = Directions.none;
						CalculationSuccessful = false; return;
					}
				}
				else{
					gears[i].direction = TransformDirection(gears[refIndex]);

					gears[i].rotation = gearPlacer.CalculateRotation(gears[i].position, gears[refIndex], gears[i].splitRatio);
					CalculateRotationPointers(i);
				}
			}
		}
	}

	Directions TransformDirection(Gear refGear)
	{
		switch(refGear.direction)
		{
			case Directions.left:
				return Directions.right;
			case Directions.right:
				return Directions.left;
		}
		return Directions.none;
	}

	IEnumerator RotateGears()
	{
		while(true)
		{
			for (int i = 0; i < gears.Count; i++)
			{
				RotateGear(gears[i]);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	void RotateGear(Gear gear)
	{
		if(gear.direction == Directions.none) return;

		var r = (gear.splitRatio/StartingGear.splitRatio) + gear.splitRatio/2;
		r *= (gear.direction == StartingGear.direction) ? RPM : -RPM;

		r += gear.rotation.y;
		if(r > 360) r -= (r - (r % 360));

		gear.rotation = new Vector3(-90, 0, r);
	}
}
