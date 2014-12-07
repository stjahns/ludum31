using UnityEngine;
using System.Collections;
using System;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;
	public GameObject RoachPartyPrefab;

	public SpaceCharacterController SpawnedPlayer;

	public event Action PlayerKilled;
	public event Action RoachPlayerKilled;

	public void Start()
	{
	}

	public SpaceCharacterController SpawnPlayer()
	{
		GameObject playerObject = Instantiate(HumanPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		SpawnedPlayer.OnDeath += OnPlayerKilled; // TODO - must differentiate between human/roach!
		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}
	
	public SpaceCharacterController SpawnRoachPlayer()
	{
		GameObject playerObject = Instantiate(RoachPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<SpaceCharacterController>();
		SpawnedPlayer.OnDeath += OnRoachPlayerKilled;
		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}

	public void SpawnRoachParty()
	{
		GameObject playerObject = Instantiate(RoachPartyPrefab, transform.position, Quaternion.identity) as GameObject;
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

	public void OnRoachPlayerKilled(GameObject player)
	{
		if (RoachPlayerKilled != null)
		{
			RoachPlayerKilled();
		}
	}

	public void OnPlayerKilled(GameObject player)
	{
		if (PlayerKilled != null)
		{
			PlayerKilled();
		}
	}
}
