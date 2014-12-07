using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoachSpawner : MonoBehaviour {

	public GameObject RoachPrefab;

	public List<RoachController> SpawnedRoaches;

	public void Start()
	{
		SpawnRoach();
	}

	public void SpawnRoach()
	{
		// Blow open grate if necessary?

		GameObject roach = Instantiate(RoachPrefab, transform.position, Quaternion.identity) as GameObject;
		var SpawnedRoach = roach.GetComponent<RoachController>();
		if (SpawnedRoach)
		{
			SpawnedRoach.OnDeath += OnRoachKilled;
		}
	}

	public void OnRoachKilled()
	{
		SpawnRoach();
	}
}
