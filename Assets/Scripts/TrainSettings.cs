using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trains/Train Settings")]
public class TrainSettings : ScriptableObject
{
	public float perWagonResistance = 1000;
	public float airResistance = 1000;

	public float maxAccelerationForce = 2000;

	//anchor offset from center of wagon
	public float trainAnchorOffset = 9;
	public float trainAnchorMargin = 1;

	public List<Color> playerColors = new List<Color>(4);
	public Color noPlayerColor;

	public Sprite wagonSprite;
	public Sprite locomotiveSprite;
}
