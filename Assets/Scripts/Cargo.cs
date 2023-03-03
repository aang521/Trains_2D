using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{
	public CargoDefinition definition;
	public new Rigidbody2D rigidbody;
	public SpriteRenderer spriteRenderer;

	public bool attached = true;
	public float detachTime = 0;

	public float fadeOutTime = 1.5f;

	public float GetMass()
	{
		return definition.mass;
	}

	public void Detach(Vector2 velocity)
	{
		attached = false;
		detachTime = Time.time;

		transform.parent = null;
		rigidbody.velocity = velocity;
		rigidbody.simulated = true;
	}

	public void Update()
	{
		if (attached) return;
		float fadeOutFraction = (fadeOutTime - (Time.time - detachTime)) / fadeOutTime;
		if(fadeOutFraction < 0)
		{
			Destroy(gameObject);
		}
		Color col = spriteRenderer.color;
		col.a = fadeOutFraction * fadeOutFraction;
		spriteRenderer.color = col;
	}
}