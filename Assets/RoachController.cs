using UnityEngine;
using System.Collections;

public class RoachController : SpaceCharacterController
{
	override protected void UpdateAnimatorParams()
	{
		base.UpdateAnimatorParams();
		Animator animator = GetComponentInChildren<Animator>();
		if (animator)
		{
			animator.SetBool("Flying", State == CharacterState.Floating);
		}
	}

	private float walkingTime;
	private bool walkLeft;

	public float MinWalkTime = 1.0f;
	public float MaxWalkTime = 5.0f;

	override protected void Start()
	{
		base.Start();
		Velocity = JumpSpeed * -Vector2.up;
	}

	protected override void OnLand()
	{
		// pick random left/right direction
		walkLeft = Random.Range(0f, 1f) > 0.5;
		walkingTime = Random.Range(MinWalkTime, MaxWalkTime);
	}

	protected override void SetCharacterOrientation(SpaceCharacterController.CharacterOrientation o)
	{
		base.SetCharacterOrientation(o);

		if (State == CharacterState.Floating)
		{
			SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
			renderer.transform.rotation = Quaternion.FromToRotation(Vector2.up, Velocity);
		}
	}

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

	override protected void GetAutomatedControl()
	{
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

	protected override void Update()
	{
		base.Update();

		// Check if collision with player
		Collider2D collider = Physics2D.OverlapCircle(transform.position, WallStickRadius, LayerMask.GetMask("Player"));
		if (collider)
		{
			PlayerController player = collider.GetComponent<PlayerController>();
			player.AddDamage();
		}

	}
	
}
