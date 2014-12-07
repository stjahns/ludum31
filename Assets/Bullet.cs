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

		var roach = coll.gameObject.GetComponent<RoachController>();
		if (roach)
		{
			AudioSource.PlayClipAtPoint(HitSound, transform.position);
			effectPrefab = RoachHitEffect;
			roach.AddDamage();
		}

		Vector2 normal = coll.contacts[0].normal;
		Object effect = Instantiate(effectPrefab, coll.contacts[0].point, Quaternion.FromToRotation(Vector2.up, normal));
		Destroy(effect, HitEffectTime);

		Destroy(gameObject);
	}
}
