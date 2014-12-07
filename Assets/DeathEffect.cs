using UnityEngine;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
	public float MinForce = 5f;
	public float MaxForce = 10f;

	public float MinTorque = 5f;
	public float MaxTorque = 10f;

	public float FadeTime = 10f;
	public bool FadeAfterDelay = false;

	void Start()
	{
		var childBodies = transform.GetComponentsInChildren<Rigidbody2D>();
		foreach (var body in childBodies)
		{
			Vector2 direction = body.transform.position - transform.position;
			float force = Random.Range(MinForce, MaxForce);
			body.AddForce(direction * force, ForceMode2D.Impulse);

			float torque = Random.Range(MinTorque, MaxTorque);
			body.AddTorque(torque);

			// TODO - fade effect after delay?
			if (FadeAfterDelay)
				Destroy(body.gameObject, FadeTime);
		}

		if (FadeAfterDelay)
			Destroy(gameObject, FadeTime);
	}

}
