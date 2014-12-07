using UnityEngine;
using System.Collections;

public class PlayerController : SpaceCharacterController
{
	public GameObject BulletPrefab;

	public float FireOffset = 0.3f;
	public float FireRate = 0.1f;
	private float fireDelay = 0;

	public AudioClip FiringSound;


	override protected void Start()
	{
		base.Start();
		Velocity = JumpSpeed * -Vector2.up;
	}

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

	private float walkingTime;
	private bool walkLeft;

	public float MinWalkTime = 1.0f;
	public float MaxWalkTime = 5.0f;

	protected override void OnLand()
	{
		// pick random left/right direction
		walkLeft = Random.Range(0f, 1f) > 0.5;
		walkingTime = Random.Range(MinWalkTime, MaxWalkTime);
	}

	// :/
	void RoachWalk()
	{
		switch (Orientation)
		{
			case CharacterOrientation.OnFloor:

				if (walkLeft)
				{
					Walk(-Vector2.right);
				}
				else
				{
					Walk(Vector2.right);
				}

				break;

			case CharacterOrientation.OnCeiling:
				if (walkLeft)
				{
					Walk(Vector2.right);
				}
				else
				{
					Walk(-Vector2.right);
				}
				break;

			case CharacterOrientation.OnLeftWall:
				if (walkLeft)
				{
					Walk(Vector2.up);
				}
				else
				{
					Walk(-Vector2.up);
				}
				break;

			case CharacterOrientation.OnRightWall:
				if (walkLeft)
				{
					Walk(-Vector2.up);
				}
				else
				{
					Walk(Vector2.up);
				}
				break;
		}
	}

	Vector2[] aimDirections = new Vector2[] {
				new Vector2(1, 1).normalized,
				new Vector2(-1, -1).normalized,
				new Vector2(1, -1).normalized,
				new Vector2(-1, 1).normalized,
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(-1, 0),
				new Vector2(0, -1)
			};

	void AutoFire()
	{
		var roach = GameObject.FindObjectOfType<RoachController>();
		if (roach)
		{
			Vector3 direction = roach.transform.position - transform.position;

			RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, 100, LayerMask.GetMask("Wall", "EntranceDoor", "Roaches"));

			// Check line of sight
			if (raycast.collider != null && raycast.collider.GetComponent<RoachController>())
			{
				// Aim and shoot in general direction of player roach...

				Vector2 bestDirection = new Vector2(0, 0);
				float bestDirectionError = float.MaxValue;

				foreach (var aimDirection in aimDirections)
				{
					Vector2 d = direction;
					float error = (aimDirection - d).sqrMagnitude;
					if (error < bestDirectionError)
					{
						bestDirectionError = error;
						bestDirection = aimDirection;
					}
				}

				AimInDirection(bestDirection);
				Fire(bestDirection);
			}
		}
	}

	override protected void GetAutomatedControl()
	{
		AutoFire();

		switch (State)
		{ 
			case CharacterState.Floating:
			case CharacterState.Jumping:
				// Chill?
				break;
			case CharacterState.Walking:
				walkingTime -= Time.deltaTime;
				if (walkingTime < 0f)
				{
					// Jump in random direction
					Vector2 jumpDirection = GetRandomJumpDirection();
					Jump(jumpDirection);
					break;
				}
				else
				{
					RoachWalk();
				}
				break;
		}
	}

	override protected void GetHumanControl()
	{
		base.GetHumanControl();


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
