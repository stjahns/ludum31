using UnityEngine;
using System.Collections;
using System;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;

	public PlayerController SpawnedPlayer;

	public event Action PlayerKilled;


	public void Start()
	{
	}

	public void SpawnPlayer()
	{
		GameObject playerObject = Instantiate(HumanPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		SpawnedPlayer.OnDeath += OnPlayerKilled;
		StartCoroutine(WalkIn());
	}
	
	public void SpawnRoachPlayer()
	{
		GameObject playerObject = Instantiate(RoachPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		//SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		//SpawnedPlayer.OnDeath += OnPlayerKilled;
		//StartCoroutine(WalkIn());
	}

	IEnumerator WalkIn()
	{
		yield return new WaitForSeconds(1.0f);

		// TODO open Door (play a sound)

		float walkTime = 1f;
		while(walkTime > 0)
		{
			walkTime -= Time.deltaTime;
			SpawnedPlayer.Walk(-Vector2.right);
			yield return 0;
		}

		// TODO close door

		SpawnedPlayer.WalkingIn = false;
		SpawnedPlayer.HumanControlled = true;
	}

	public void OnPlayerKilled(GameObject player)
	{
		if (PlayerKilled != null)
		{
			PlayerKilled();
		}
	}
}
