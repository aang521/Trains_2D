using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float[] playerScores = new float[4];

	public bool playing = true;

	public float matchDuration;
	[HideInInspector]
	public float startTime;

	public static GameManager instance;

	public void Awake()
	{
		instance = this;

		StartGame();
	}

	public void StartGame()
	{
		startTime = Time.time;
	}

	public void Update()
	{
		float timePlaying = Time.time - startTime;
		float remainingTime = matchDuration - timePlaying;
		if(remainingTime < 0)
		{
			playing = false;
		}
	}
}
