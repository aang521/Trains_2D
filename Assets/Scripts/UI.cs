using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public List<TMP_Text> playerScoreText = new List<TMP_Text>(4);

    public TMP_Text timeRemainingText;

	public void Update()
	{
		for(int i = 0; i < 4; i++)
		{
			playerScoreText[i].text = GameManager.instance.playerScores[i].ToString("N",new CultureInfo("nl-NL"));
		}

		float timePlaying = Time.time - GameManager.instance.startTime;
		float remainingTime = GameManager.instance.matchDuration - timePlaying;

		timeRemainingText.text = Mathf.Floor(remainingTime).ToString() + " seconden resterend";
	}
}
