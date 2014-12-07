﻿using UnityEngine;
using System.Collections;

public class PlayerController : SpaceCharacterController
{
	public GameObject BulletPrefab;

	public float FireOffset = 0.3f;
	public float FireRate = 0.1f;
	private float fireDelay = 0;

	public AudioClip FiringSound;

	override protected void Update()
	{
		base.Update();

		if (fireDelay > 0)
		{
			fireDelay -= Time.deltaTime;
		}
	}

	override protected void UpdateControl()
	{
		if (HumanControlled)
		{
			GetHumanControl();
		}
		else
		{
			if (!WalkingIn)
			{
				GetAutomatedControl();
			}
		}
	}

	void GetAutomatedControl()
	{
		// TODO
		// Random whatever, shoot at stuff
	}

	void GetHumanControl()
	{
		if (State == CharacterState.Walking)
		{
			if (OnVerticalSurface())
			{
				if (Input.GetKey(KeyCode.W))
				{
					// move up if on wall
					//Velocity = Vector2.up * WalkSpeed;
					Walk(Vector2.up);
					FaceDirection(Vector2.up);
				}
				else if (Input.GetKey(KeyCode.S))
				{
					// move down if on wall
					//Velocity = -Vector2.up * WalkSpeed;
					Walk(-Vector2.up);
					FaceDirection(-Vector2.up);
				}
				else
				{
					Velocity = Vector2.zero;
				}
			}

			if (OnHorizontalSurface())
			{
				if (Input.GetKey(KeyCode.A))
				{
					// move left if on floor / ceiling
					//Velocity = -Vector2.right * WalkSpeed;
					Walk(-Vector2.right);
					FaceDirection(-Vector2.right);
				}
				else if (Input.GetKey(KeyCode.D))
				{
					// move right if on floor / ceiling
					//Velocity = Vector2.right * WalkSpeed;
					Walk(Vector2.right);
					FaceDirection(Vector2.right);
				}
				else
				{
					Velocity = Vector2.zero;
				}
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				Vector2 jumpDirection = GetJumpDirection();
				Jump(jumpDirection);
			}
		}

		Vector2 aimDirection = GetAimDirection();
		if (aimDirection != Vector2.zero)
		{ 
			AimInDirection(aimDirection);
			Fire(aimDirection);
		}
	}

	Vector2 GetAimDirection()
	{
		Vector2 direction = Vector2.zero;

		if (Input.GetKey(KeyCode.UpArrow))
		{
			direction += Vector2.up;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			direction -= Vector2.up;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			direction -= Vector2.right;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			direction += Vector2.right;
		}

		return direction.normalized;
	}


	void Fire(Vector2 direction)
	{
		if (fireDelay <= 0)
		{
			AudioSource.PlayClipAtPoint(FiringSound, transform.position);

			// TODO show muzzle effect?
			// TODO shake screen?

			Vector3 bulletOffset = direction * FireOffset;
			Instantiate(BulletPrefab,
						transform.position + bulletOffset,
						Quaternion.FromToRotation(Vector2.up, direction));

			fireDelay = FireRate;
		}
	}

	void AimInDirection(Vector2 direction)
	{

		FaceDirection(direction);

		Animator animator = GetComponentInChildren<Animator>();
		if (animator)
		{
			bool aimUpForward = false;
			bool aimDownForward = false;

			switch (Orientation)
			{
				case CharacterOrientation.Floating:
				case CharacterOrientation.OnFloor:
					animator.SetBool("AimUp", direction == Vector2.up);
					animator.SetBool("AimDown", direction == -Vector2.up);
					aimUpForward = (direction == new Vector2(1, 1).normalized) || (direction == new Vector2(-1, 1).normalized);
					aimDownForward = (direction == new Vector2(1, -1).normalized) || (direction == new Vector2(-1, -1).normalized);
					break;

				case CharacterOrientation.OnCeiling:
					animator.SetBool("AimUp", direction == -Vector2.up);
					animator.SetBool("AimDown", direction == Vector2.up);
					aimUpForward = (direction == new Vector2(1, -1).normalized) || (direction == new Vector2(-1, -1).normalized);
					aimDownForward = (direction == new Vector2(1, 1).normalized) || (direction == new Vector2(-1, 1).normalized);
					break;

				case CharacterOrientation.OnLeftWall:
					animator.SetBool("AimUp", direction == Vector2.right);
					animator.SetBool("AimDown", direction == -Vector2.right);
					aimUpForward = (direction == new Vector2(1, 1).normalized) || (direction == new Vector2(1, -1).normalized);
					aimDownForward = (direction == new Vector2(-1, 1).normalized) || (direction == new Vector2(-1, -1).normalized);
					break;

				case CharacterOrientation.OnRightWall:
					animator.SetBool("AimUp", direction == -Vector2.right);
					animator.SetBool("AimDown", direction == Vector2.right);
					aimUpForward = (direction == new Vector2(-1, -1).normalized) || (direction == new Vector2(-1, 1).normalized);
					aimDownForward = (direction == new Vector2(1, 1).normalized) || (direction == new Vector2(1, -1).normalized);
					break;
			}

			animator.SetBool("AimUpForward", aimUpForward);
			animator.SetBool("AimDownForward", aimDownForward);

		}

	}
}
