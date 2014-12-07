using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	public float Speed = 10f;

	public AudioClip HitSound;

	void Start()
	{
		rigidbody2D.velocity = transform.rotation * Vector2.up * Speed;
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		// TODO spawn some kind of burst / spark effect

		AudioSource.PlayClipAtPoint(HitSound, transform.position);

		var roach = coll.gameObject.GetComponent<RoachController>();
		if (roach)
		{
			roach.Kill();
		}

		Destroy(gameObject);
	}
}
