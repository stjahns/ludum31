using UnityEngine;
using System.Collections;

public class RoachController : MonoBehaviour
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

		//QueryInput();
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

	void FaceDirection(Vector2 direction)
	{
		SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

		float xScale = direction.x;
		if (xScale > 0) xScale = 1;
		if (xScale < 0) xScale = -1;

		float yScale = direction.y;
		if (yScale > 0) yScale = 1;
		if (yScale < 0) yScale = -1;

		switch (Orientation)
		{
			case PlayerOrientation.OnFloor:
			case PlayerOrientation.Floating:
				if (xScale != 0)
					renderer.transform.localScale = new Vector3(xScale, 1, 1);
				break;

			case PlayerOrientation.OnCeiling:
				if (xScale != 0)
					renderer.transform.localScale = new Vector3(-xScale, 1, 1);
				break;

			case PlayerOrientation.OnLeftWall:
				if (yScale != 0)
					renderer.transform.localScale = new Vector3(-yScale, 1, 1);
				break;

			case PlayerOrientation.OnRightWall:
				if (yScale != 0)
					renderer.transform.localScale = new Vector3(yScale, 1, 1);
				break;
		}

	}

}
