using UnityEngine;
using System.Collections;

public class DoorCloser : MonoBehaviour {

	public AirlockDoor door;
	float triggerRadius = 0.25f;

	void Update()
	{
		Collider2D collider = Physics2D.OverlapCircle(transform.position, triggerRadius, LayerMask.GetMask("Player"));
		if (collider)
		{
			var player = collider.GetComponent<SpaceCharacterController>();
			if (player && player.HumanControlled)
			{
				door.Close();
			}
		}
	}
}
