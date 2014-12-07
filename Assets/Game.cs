using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{

	public Text TitleText;
	public Text SecondTitleText;
	public Text PartyText;

	public PlayerSpawner playerSpawner;
	public List<RoachSpawner> roachSpawners;

	public List<RoachController> liveRoaches;

	public int WaveNumber = 1;
	
	void Start()
	{
		playerSpawner.PlayerKilled += OnPlayerKilled;

		liveRoaches.ForEach(r => r.OnDeath += OnRoachKilled);

		// Flash title text
		StartCoroutine(ResetPlayer());
	}

	void Update()
	{
		// When human killed
		// Bring in roach party shipt
		// Flash party text	

		//StartCoroutine(FlashText(PartyText), int.MaxValue);
	}

	IEnumerator FlashText(Text text, int numFlashes = 3)
	{
		for (int i = 0; i < numFlashes; i++)
		{
			text.enabled = true;
			yield return new WaitForSeconds(0.5f);
			text.enabled = false;
			yield return new WaitForSeconds(0.5f);
		}
	}

	void NextWave()
	{
		WaveNumber += 1;
		switch (WaveNumber)
		{
			case 2:
				Wave5();
				//Wave2();
				break;
			case 3:
				Wave3();
				break;
			case 4:
				Wave4();
				break;
			case 5:
				Wave5();
				break;
		}
	}


	public List<RoachSpawner> Wave2Spawners;
	void Wave2()
	{
		Debug.Log("WAVE 2");
		foreach (var spawner in Wave2Spawners)
		{
			var roach = spawner.SpawnRoach();
			roach.OnDeath += OnRoachKilled;
			liveRoaches.Add(roach);
		}
	}

	void Wave3()
	{
		Debug.Log("WAVE 3");
		foreach (var spawner in roachSpawners)
		{
			var roach = spawner.SpawnRoach();
			roach.OnDeath += OnRoachKilled;
			liveRoaches.Add(roach);
		}
	}

	void Wave4()
	{
		Debug.Log("WAVE 4");
		foreach (var spawner in roachSpawners)
		{
			var roach = spawner.SpawnRoach();
			roach.OnDeath += OnRoachKilled;
			liveRoaches.Add(roach);

			roach = spawner.SpawnRoach();
			roach.OnDeath += OnRoachKilled;
			liveRoaches.Add(roach);
		}
	}

	void Wave5()
	{
		Debug.Log("WAVE 5 -- Reversal");
		StartCoroutine(RoachVengeance());
	}

	IEnumerator RoachVengeance()
	{
		yield return new WaitForSeconds(1f);

		// Spawn player controlled roach 
		playerSpawner.SpawnedPlayer.HumanControlled = false;
		playerSpawner.SpawnRoachPlayer();

		yield return new WaitForSeconds(1f);

		StartCoroutine(FlashText(SecondTitleText));
	}

	void OnPlayerKilled()
	{
		StartCoroutine(ResetPlayer());
	}

	IEnumerator ResetPlayer()
	{
		// Respawn and flash title again..
		playerSpawner.SpawnPlayer();

		yield return new WaitForSeconds(2.0f);

		StartCoroutine(FlashText(TitleText));
	}

	void OnRoachKilled(GameObject roach)
	{
		var roachController = roach.GetComponent<RoachController>();
		liveRoaches.Remove(roachController);

		if (liveRoaches.Count == 0)
		{
			NextWave();
		}
	}
}
