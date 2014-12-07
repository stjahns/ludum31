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
		collider2D.enabled = false;
		AudioSource.PlayClipAtPoint(openClip, transform.position);
		isOpen = true;
	}

	public void Close()
	{
		if (!isOpen)
			return;
		var animator = GetComponent<Animator>();
		animator.SetBool("Open", false);
		collider2D.enabled = true;
		AudioSource.PlayClipAtPoint(closeClip, transform.position);
		isOpen = false;
	}
}
