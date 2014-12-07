using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	public float Speed = 10f;

	public AudioClip HitSound;
	public GameObject RoachHitEffect;
	public GameObject HitEffect;
	public float HitEffectTime;

	void Start()
	{
		rigidbody2D.velocity = transform.rotation * Vector2.up * Speed;
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		GameObject effectPrefab = HitEffect;

		var dude = coll.gameObject.GetComponent<SpaceCharacterController>();
		if (dude)
		{
			// Bullets shouldn't hit player UNLESS player is actually a roach..
			if (dude != Game.Player || Game.PlayerIsRoach)
			{
				AudioSource.PlayClipAtPoint(HitSound, transform.position);
				effectPrefab = RoachHitEffect;
				dude.AddDamage();
			}
		}

		Vector2 normal = coll.contacts[0].normal;
		Object effect = Instantiate(effectPrefab, coll.contacts[0].point, Quaternion.FromToRotation(Vector2.up, normal));
		Destroy(effect, HitEffectTime);

		Destroy(gameObject);
	}
}
