using UnityEngine;
using System.Collections;
using System;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;
	public GameObject RoachPartyPrefab;

	public Animator ExterminatorShip;
	public Animator AirlockDoor_Exterminator;
	public Animator AirlockDoor_Main;

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
		SpawnedPlayer.OnDeath += OnPlayerKilled;
		SpawnedPlayer.transform.parent = ExterminatorShip.transform; // Stay with ship...

		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}
	
	public SpaceCharacterController SpawnRoachPlayer()
	{
		GameObject playerObject = Instantiate(RoachPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<SpaceCharacterController>();
		SpawnedPlayer.OnDeath += OnRoachPlayerKilled;
		SpawnedPlayer.transform.parent = ExterminatorShip.transform; // Stay with ship...

		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}

	public void SpawnRoachParty()
	{
		//GameObject playerObject = Instantiate(RoachPartyPrefab, transform.position, Quaternion.identity) as GameObject;
	}

	IEnumerator WalkIn()
	{
		yield return new WaitForSeconds(1.0f);

		ExterminatorShip.SetTrigger("FlyIn");

		yield return new WaitForSeconds(4.0f);

		AirlockDoor_Exterminator.SetBool("Open", true);
		AirlockDoor_Main.SetBool("Open", true);

		SpawnedPlayer.transform.parent = null; // Be freee!

		yield return new WaitForSeconds(0.2f);

		// Consider just manually animating ...

		float walkTime = 0.1f;
		while(walkTime > 0)
		{
			walkTime -= Time.deltaTime;
			SpawnedPlayer.Walk(-Vector2.right);
			yield return 0;
		}

		yield return new WaitForSeconds(0.5f);

		AirlockDoor_Exterminator.SetBool("Open", false);
		AirlockDoor_Main.SetBool("Open", false);

		yield return new WaitForSeconds(1f);

		ExterminatorShip.SetTrigger("FlyOut");

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
