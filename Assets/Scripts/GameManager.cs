using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float[] playerScores = new float[4];

	public bool playing = true;

	public GameObject endScreen;

	public float matchDuration;
	[HideInInspector]
	public float startTime;

	public static GameManager instance;

	public AudioSource muziek;

	public void Awake()
	{
		instance = this;

		StartGame();
	}

	public void StartGame()
	{
		startTime = Time.time;
		muziek.Play();
	}

	public void Update()
	{
		float timePlaying = Time.time - startTime;
		float remainingTime = matchDuration - timePlaying;
		if(remainingTime < 0)
		{
			playing = false;
			endScreen.SetActive(true);
		}
	}
}
