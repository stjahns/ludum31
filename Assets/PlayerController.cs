using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	public enum PlayerState
	{
		Walking,
		Jumping,
		Floating
	}

	public enum PlayerOrientation
	{
		Floating,
		OnLeftWall,
		OnRightWall,
		OnFloor,
		OnCeiling
	}

	public PlayerState _state;
	public PlayerState State
	{
		get { return _state; }
		set 
		{
			_state = value; 
		}
	}

	public PlayerOrientation Orientation;

	public float WallStickRadius = 1.0f;

	public Vector2 Velocity;
	public float WalkSpeed = 1.0f;
	public float JumpSpeed = 1.0f;

	void Start()
	{
		State = PlayerState.Floating;
		Velocity = JumpSpeed * -Vector2.up;
	}

	void UpdateAnimatorParams()
	{
		Animator animator = GetComponentInChildren<Animator>();
		if (animator)
		{
			animator.SetFloat("Speed", Velocity.magnitude);
		}
	}

	void Update()
	{
		UpdateAnimatorParams();

		if (State == PlayerState.Walking)
		{
			if (GetPlayerOrientation() == PlayerOrientation.Floating)
			{
				// Walked off edge probably..
				State = PlayerState.Floating;
			}

		}

		if (State == PlayerState.Floating)
		{
			SetPlayerOrientation(GetPlayerOrientation());
		}

		if (State == PlayerState.Jumping)
		{
			if (GetPlayerOrientation() == PlayerOrientation.Floating)
			State = PlayerState.Floating;
		}

		transform.position = new Vector2(transform.position.x, transform.position.y) + Velocity * Time.deltaTime;

		QueryInput();
	}

	PlayerOrientation GetPlayerOrientation()
	{
		// Check if we should stick to a wall
		Collider2D collider = Physics2D.OverlapCircle(transform.position, WallStickRadius, LayerMask.GetMask("Wall"));
		if (collider != null)
		{
			Debug.Log(collider);
			WallComponent wall = collider.GetComponent<WallComponent>();
			if (wall != null)
			{
				RaycastHit2D raycast = Physics2D.Raycast(transform.position, collider.transform.position - transform.position, float.MaxValue, LayerMask.GetMask("Wall"));
				Debug.Log(raycast.normal);
				if (raycast.normal == Vector2.up)
				{
					return PlayerOrientation.OnFloor;
				}
				else if (raycast.normal == -Vector2.up)
				{
					return PlayerOrientation.OnCeiling;
				}
				else if (raycast.normal == Vector2.right)
				{
					return PlayerOrientation.OnLeftWall;
				}
				else if (raycast.normal == -Vector2.right)
				{
					return PlayerOrientation.OnRightWall;
				}
			}
		}

		return PlayerOrientation.Floating;
	}

	void SetPlayerOrientation(PlayerOrientation o)
	{

		SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

		switch (o)
		{
			case PlayerOrientation.OnLeftWall:
				renderer.transform.eulerAngles = new Vector3(0, 0, -90);
				break;
			case PlayerOrientation.OnRightWall:
				renderer.transform.eulerAngles = new Vector3(0, 0, 90);
				break;
			case PlayerOrientation.OnCeiling:
				renderer.transform.eulerAngles = new Vector3(0, 0, 180);
				break;
			case PlayerOrientation.OnFloor:
			case PlayerOrientation.Floating:
				renderer.transform.eulerAngles = new Vector3(0, 0, 0);
				break;
		}

		if (Orientation == PlayerOrientation.Floating && o != PlayerOrientation.Floating)
		{
			Velocity = Vector2.zero;
			State = PlayerState.Walking;
		}

		Orientation = o;
	}

	bool OnVerticalSurface()
	{
		return Orientation == PlayerOrientation.OnLeftWall || Orientation == PlayerOrientation.OnRightWall;
	}

	bool OnHorizontalSurface()
	{
		return Orientation == PlayerOrientation.OnFloor || Orientation == PlayerOrientation.OnCeiling;
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

	Vector2 GetJumpDirection()
	{
		Vector2 direction = Vector2.zero;

		if (Input.GetKey(KeyCode.W))
		{
			direction += Vector2.up;
		}
		if (Input.GetKey(KeyCode.S))
		{
			direction -= Vector2.up;
		}
		if (Input.GetKey(KeyCode.A))
		{
			direction -= Vector2.right;
		}
		if (Input.GetKey(KeyCode.D))
		{
			direction += Vector2.right;
		}

		if (direction == Vector2.zero)
		{
			direction = Vector2.up;
		}

		return direction.normalized;
	}

	void FaceDirection(Vector2 direction)
	{
		SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

		switch (Orientation)
		{
			case PlayerOrientation.OnFloor:
				renderer.transform.localScale = new Vector3(direction.x, 1, 1);
				break;

			case PlayerOrientation.OnCeiling:
				renderer.transform.localScale = new Vector3(-direction.x, 1, 1);
				break;

			case PlayerOrientation.OnLeftWall:
				renderer.transform.localScale = new Vector3(-direction.y, 1, 1);
				break;

			case PlayerOrientation.OnRightWall:
				renderer.transform.localScale = new Vector3(direction.y, 1, 1);
				break;
		}

	}

	void QueryInput()
	{
		if (State == PlayerState.Walking)
		{
			if (OnVerticalSurface())
			{
				if (Input.GetKey(KeyCode.W))
				{
					// move up if on wall
					Velocity = Vector2.up * WalkSpeed;
					FaceDirection(Vector2.up);
				}
				else if (Input.GetKey(KeyCode.S))
				{
					// move down if on wall
					Velocity = -Vector2.up * WalkSpeed;
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
					Velocity = -Vector2.right * WalkSpeed;
					FaceDirection(-Vector2.right);
				}
				else if (Input.GetKey(KeyCode.D))
				{
					// move right if on floor / ceiling
					Velocity = Vector2.right * WalkSpeed;
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
				Velocity = jumpDirection * JumpSpeed;
				State = PlayerState.Jumping;
			}
		}
	}
}
