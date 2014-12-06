using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	//private bool _onCeiling = false;
	//public bool OnCeiling
	//public bool OnWall

	public enum PlayerState
	{
		OnWall,
		OnFloor,
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

	public PlayerState State;
	public PlayerOrientation Orientation;

	public float WallStickRadius = 1.0f;

	public Vector2 Velocity;
	public float WalkSpeed = 1.0f;
	public float JumpSpeed = 1.0f;

	// TODO - to avoid aliasy bits - maybe have a 'RealPosition' and a 'RoundedPosition'
	// or find the right way!

	// Use this for initialization
	void Start()
	{
		State = PlayerState.Floating;
		Velocity = JumpSpeed * -Vector2.up;
	}

	// Update is called once per frame
	void Update()
	{
		SetPlayerOrientation(GetPlayerOrientation());

		transform.position = new Vector2(transform.position.x, transform.position.y) + Velocity * Time.deltaTime;

		QueryInput();
	}

	PlayerOrientation GetPlayerOrientation()
	{
		// Check if we should stick to a wall
		Collider2D collider = Physics2D.OverlapCircle(transform.position, WallStickRadius);
		if (collider != null)
		{
			WallComponent wall = collider.GetComponent<WallComponent>();
			if (wall != null)
			{
				RaycastHit2D raycast = Physics2D.Raycast(transform.position, collider.transform.position - transform.position);
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
		Animator animator = GetComponent<Animator>();
		animator.SetBool("OnFloor", false);
		animator.SetBool("OnCeiling", false);
		animator.SetBool("OnLeftWall", false);
		animator.SetBool("OnRightWall", false);

		switch (o)
		{
			case PlayerOrientation.OnLeftWall:
				animator.SetBool("OnLeftWall", true);
				break;
			case PlayerOrientation.OnRightWall:
				animator.SetBool("OnRightWall", true);
				break;
			case PlayerOrientation.OnCeiling:
				animator.SetBool("OnCeiling", true);
				break;
			case PlayerOrientation.OnFloor:
				animator.SetBool("OnFloor", true);
				break;

		}

		if (Orientation == PlayerOrientation.Floating && o != PlayerOrientation.Floating)
		{
			Velocity = Vector2.zero;
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

	void QueryInput()
	{
		if (OnVerticalSurface())
		{
			if (Input.GetKey(KeyCode.W))
			{
				// move up if on wall
				Velocity = Vector2.up * WalkSpeed;
			}
			else if (Input.GetKey(KeyCode.D))
			{
				// move down if on wall
				Velocity = -Vector2.up * WalkSpeed;
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
			}
			else if (Input.GetKey(KeyCode.D))
			{
				// move right if on floor / ceiling
				Velocity = Vector2.right * WalkSpeed;
			}
			else
			{
				Velocity = Vector2.zero;
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("JUMP");

			Vector2 jumpDirection = GetAimDirection();
			Velocity = jumpDirection * JumpSpeed;
		}

		// TODO - space -> jump in mouse direction
		// TODO - click -> fire weapon
	}
}
