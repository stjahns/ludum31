﻿using UnityEngine;
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

	public bool PlayerIsRoach = false;

	public SpaceCharacterController CurrentHuman;
	
	void Start()
	{
		playerSpawner.PlayerKilled += OnPlayerKilled;
		playerSpawner.RoachPlayerKilled += OnRoachPlayerKilled;

		liveRoaches.ForEach(r => r.OnDeath += OnRoachKilled);

		StartCoroutine(ResetPlayer());
	}

	void Update()
	{
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

		// Stop controlling human
		if (playerSpawner.SpawnedPlayer)
		{
			playerSpawner.SpawnedPlayer.HumanControlled = false;
		}

		// TODO - If no human... ??? immediately jump to roach party?

		// Spawn player controlled roach 
		playerSpawner.SpawnRoachPlayer();

		PlayerIsRoach = true;

		yield return new WaitForSeconds(1f);

		StartCoroutine(FlashText(SecondTitleText));
	}

	void OnPlayerKilled()
	{
		if (PlayerIsRoach)
		{
			// Party time!
			StartCoroutine(RoachParty());

		}
		else
		{
			StartCoroutine(ResetPlayer());
		}
	}

	IEnumerator RoachParty()
	{
		playerSpawner.SpawnRoachPlayer();

		yield return new WaitForSeconds(2.0f);

		// TODO start party musics!

		StartCoroutine(FlashText(PartyText, int.MaxValue));
	}

	void OnRoachPlayerKilled()
	{
		StartCoroutine(ResetPlayer());
	}

	IEnumerator ResetPlayer()
	{
		if (PlayerIsRoach)
		{
			playerSpawner.SpawnRoachPlayer();

			yield return new WaitForSeconds(2.0f);

			StartCoroutine(FlashText(SecondTitleText));
		}
		else
		{
			// Respawn and flash title again..
			CurrentHuman = playerSpawner.SpawnPlayer();

			yield return new WaitForSeconds(2.0f);

			StartCoroutine(FlashText(TitleText));
		}
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
