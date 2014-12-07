using UnityEngine;
using System.Collections;

public class RoachController : SpaceCharacterController
{

	Vector2 GetRandomJumpDirection()
	{
		Vector2[] directions = new Vector2[3];
		switch (Orientation)
		{
			case CharacterOrientation.OnFloor:
				directions[0] = Vector2.up;
				directions[1] = new Vector2(1, 1).normalized;
				directions[2] = new Vector2(-1, 1).normalized;
				break;

			case CharacterOrientation.OnCeiling:
				directions[0] = -Vector2.up;
				directions[1] = new Vector2(-1, -1).normalized;
				directions[2] = new Vector2(1, -1).normalized;
				break;

			case CharacterOrientation.OnLeftWall:
				directions[0] = Vector2.right;
				directions[1] = new Vector2(1, 1).normalized;
				directions[2] = new Vector2(1, -1).normalized;
				break;

			case CharacterOrientation.OnRightWall:
				directions[0] = -Vector2.right;
				directions[1] = new Vector2(-1, 1).normalized;
				directions[2] = new Vector2(-1, -1).normalized;
				break;
		}

		int index = Random.Range(0, 2);
		return directions[index];
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

	protected override void UpdateControl()
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
					Velocity = jumpDirection * JumpSpeed;
					State = CharacterState.Jumping;
					break;
				}
				else
				{
					RoachWalk();
				}
				break;
		}
	}
}
