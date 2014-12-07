using UnityEngine;
using System.Collections;

public class AirlockDoor : MonoBehaviour 
{
	public AudioClip openClip;
	public AudioClip closeClip;

	public bool isOpen = false;

	public void Open()
	{
		if (isOpen)
			return;

		var animator = GetComponent<Animator>();
		animator.SetBool("Open", true);
		gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
		AudioSource.PlayClipAtPoint(openClip, transform.position);
		isOpen = true;
	}

	public void Close()
	{
		if (!isOpen)
			return;
		var animator = GetComponent<Animator>();
		animator.SetBool("Open", false);
		gameObject.layer = LayerMask.NameToLayer("Wall");
		AudioSource.PlayClipAtPoint(closeClip, transform.position);
		isOpen = false;
	}
}
