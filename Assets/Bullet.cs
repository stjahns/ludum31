using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	public float Speed = 10f;

	void Start()
	{
		rigidbody2D.velocity = transform.rotation * Vector2.up * Speed;
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		// TODO spawn some kind of burst / spark effect
		var roach = coll.gameObject.GetComponent<RoachController>();
		if (roach)
		{
			// TODO call a 'kill' method on the roach
			Destroy(roach.gameObject);
		}

		Destroy(gameObject);
	}
}
