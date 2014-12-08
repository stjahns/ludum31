using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoachSpawner : MonoBehaviour {

	public GameObject RoachPrefab;

	public Sprite OpenedSprite;

	public List<RoachController> SpawnedRoaches;

	public AudioClip PopSound;

	public bool Opened = false;

	public void Start()
	{
		if (Opened)
		{
			var renderer = base.renderer as SpriteRenderer;
			renderer.sprite = OpenedSprite;
		}
	}


	public RoachController SpawnRoach()
	{
		if (!Opened)
		{
			AudioSource.PlayClipAtPoint(PopSound, transform.position);
		}

		Opened = true;
		var renderer = base.renderer as SpriteRenderer;
		renderer.sprite = OpenedSprite;

		GameObject roach = Instantiate(RoachPrefab, transform.position, Quaternion.identity) as GameObject;
		var SpawnedRoach = roach.GetComponent<RoachController>();
		if (SpawnedRoach)
		{
			SpawnedRoach.OnDeath += OnRoachKilled;
		}

		// Give it a random direction?

		return SpawnedRoach;
	}

	public void OnRoachKilled(GameObject roach)
	{
		//SpawnRoach();
	}
}
