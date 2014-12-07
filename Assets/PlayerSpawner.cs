using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{

	public GameObject HumanPlayerPrefab;
	public GameObject RoachPlayerPrefab;

	private PlayerController SpawnedPlayer;

	public void Start()
	{
		SpawnPlayer();
	}

	public void SpawnPlayer()
	{
		GameObject playerObject = Instantiate(HumanPlayerPrefab, transform.position, Quaternion.identity) as GameObject;
		SpawnedPlayer = playerObject.GetComponent<PlayerController>();
		SpawnedPlayer.OnDeath += OnPlayerKilled;
		StartCoroutine(WalkIn());
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

	public void OnPlayerKilled()
	{
		// Tell game to show title again...?
		SpawnPlayer();
	}
}
