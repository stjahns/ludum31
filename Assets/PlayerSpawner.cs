using UnityEngine;
using System.Collections;
using System;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;
	public GameObject RoachPartyPrefab;

	public Animator ExterminatorShip;
	public AirlockDoor AirlockDoor_Exterminator;
	public AirlockDoor AirlockDoor_Main;
	public AirlockDoor AirlockDoor_Inner;

	public SpaceCharacterController SpawnedPlayer;

	public event Action PlayerKilled;
	public event Action RoachPlayerKilled;

	public Sprite AirlockTallOpen;
	public Sprite AirlockTallClosed;

	public void Start()
	{
	}

	public SpaceCharacterController SpawnPlayer()
	{
		GameObject playerObject = Instantiate(HumanPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		SpawnedPlayer.OnDeath += OnPlayerKilled;
		SpawnedPlayer.transform.parent = ExterminatorShip.transform; // Stay with ship...

		SpawnedPlayer.State = SpaceCharacterController.CharacterState.Walking;
		SpawnedPlayer.Velocity = Vector2.zero;

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

	public Transform AirlockCenter;

	IEnumerator WalkIn()
	{
		yield return new WaitForSeconds(1.0f);

		SpawnedPlayer.Velocity = Vector2.zero;

		ExterminatorShip.SetTrigger("FlyIn");

		yield return new WaitForSeconds(4.0f);

		AirlockDoor_Exterminator.Open();
		AirlockDoor_Main.Open();

		SpawnedPlayer.transform.parent = null; // Be freee!

		yield return new WaitForSeconds(0.2f);


		Vector2 startPosition = SpawnedPlayer.transform.position;
		Vector2 endPostion = AirlockCenter.position;

		SpawnedPlayer.Velocity = -Vector2.right;
		float totalWalkTime = 1.5f;
		float walkTime = 0.0f;
		while(walkTime <= totalWalkTime)
		{
			SpawnedPlayer.transform.position = Vector2.Lerp(startPosition, endPostion, walkTime / totalWalkTime);
			walkTime += Time.deltaTime;
			yield return 0;
		}

		SpawnedPlayer.Velocity = Vector2.zero;

		yield return new WaitForSeconds(0.5f);

		AirlockDoor_Exterminator.Close();
		AirlockDoor_Main.Close();

		yield return new WaitForSeconds(0.5f);

		AirlockDoor_Inner.Open();

		yield return new WaitForSeconds(1f);

		SpawnedPlayer.WalkingIn = false;
		SpawnedPlayer.HumanControlled = true;

		ExterminatorShip.SetTrigger("FlyOut");
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
