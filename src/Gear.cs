using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Gears
//{
	[System.Serializable]
	public class Gear
	{
		public GameObject prefab;
		public float radius;
		public float splitRatio;

		
		[HideInInspector]
		public GameObject gameObject;

		public Vector3 forward{get{return gameObject.transform.up;}}

		public Vector3 position { 
			get { return gameObject.transform.position; } 
			set { gameObject.transform.position = value; }
		}  
		public Vector3 rotation{
			get{return gameObject.transform.rotation.eulerAngles;}
			set{gameObject.transform.rotation = Quaternion.Euler(value);}
		}

		[HideInInspector]
		public Color color{
			get{return gameObject.GetComponentInChildren<MeshRenderer>().material.color;}
			set{gameObject.GetComponentInChildren<MeshRenderer>().material.color = value;}
		}

		[HideInInspector]
		public RotationBehaviour.Directions direction = RotationBehaviour.Directions.none;
	

		public void SetVisible(bool status)
		{
			var renderers = gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = status;
			}
		}

		public void CloneTo(Gear copyToThis)
		{
			copyToThis.prefab = this.prefab;
			copyToThis.radius = this.radius;
			copyToThis.splitRatio = this.splitRatio;
		}
	}
//}
