using UnityEngine;
using System.Collections;
using System;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;

	public GameObject PartyRoachPrefab;

	public Animator ExterminatorShip;
	public AirlockDoor AirlockDoor_Exterminator;
	public AirlockDoor AirlockDoor_Main;
	public AirlockDoor AirlockDoor_Inner;

	public SpaceCharacterController SpawnedPlayer;

	public event Action PlayerKilled;
	public event Action RoachPlayerKilled;

	public Sprite HumanShip;
	public Sprite RoachShip;
	public Sprite RoachPartyShip;

	public AudioClip ShipSound;

	public void Start()
	{
	}

	SpriteRenderer ShipRenderer()
	{
		return ExterminatorShip.GetComponent<SpriteRenderer>();
	}

	public SpaceCharacterController SpawnPlayer()
	{
		GameObject playerObject = Instantiate(HumanPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		SpawnedPlayer.OnDeath += OnPlayerKilled;
		SpawnedPlayer.transform.SetParent(ExterminatorShip.transform); // Stay with ship...

		SpawnedPlayer.State = SpaceCharacterController.CharacterState.Walking;
		SpawnedPlayer.Velocity = Vector2.zero;

		ShipRenderer().sprite = HumanShip;

		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}
	
	public SpaceCharacterController SpawnRoachPlayer()
	{
		GameObject playerObject = Instantiate(RoachPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<SpaceCharacterController>();
		SpawnedPlayer.OnDeath += OnRoachPlayerKilled;
		SpawnedPlayer.transform.parent = ExterminatorShip.transform; // Stay with ship...

		ShipRenderer().sprite = RoachShip;

		StartCoroutine(WalkIn());
		return SpawnedPlayer;
	}

	public void SpawnRoachParty()
	{
		ShipRenderer().sprite = RoachPartyShip;
		ShipRenderer().sortingLayerName = "PartyBusFG";
		StartCoroutine(PartyDown());
	}

	public float PartySpawnInterval = 0.5f;
	public int MaxPartyRoaches = 50;

	IEnumerator PartyDown()
	{
		yield return new WaitForSeconds(1.0f);

		ExterminatorShip.SetTrigger("FlyIn");
		AudioSource.PlayClipAtPoint(ShipSound, transform.position);

		yield return new WaitForSeconds(4.0f);

		AirlockDoor_Exterminator.Open();
		AirlockDoor_Exterminator.GetComponent<Collider2D>().enabled = false;
		AirlockDoor_Main.Open();
		AirlockDoor_Main.GetComponent<Collider2D>().enabled = false;
		AirlockDoor_Inner.Open();
		AirlockDoor_Inner.GetComponent<Collider2D>().enabled = false;

		int partyCount = 0;

		while (partyCount < MaxPartyRoaches) // YEEEEEEE
		{
			Instantiate(PartyRoachPrefab, transform.position, Quaternion.identity);
			yield return new WaitForSeconds(PartySpawnInterval);
			partyCount++;
		}
	}

	public Transform AirlockCenter;

	IEnumerator WalkIn()
	{
		yield return new WaitForSeconds(1.0f);

		SpawnedPlayer.Velocity = Vector2.zero;

		ExterminatorShip.SetTrigger("FlyIn");

		AudioSource.PlayClipAtPoint(ShipSound, transform.position);

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

		SpawnedPlayer.WalkingIn = false;
		SpawnedPlayer.HumanControlled = true;

		yield return new WaitForSeconds(1f);

		ExterminatorShip.SetTrigger("FlyOut");
		AudioSource.PlayClipAtPoint(ShipSound, transform.position);
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
