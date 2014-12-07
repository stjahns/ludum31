using UnityEngine;
using System.Collections;
using System;

public class SpaceCharacterController : MonoBehaviour
{
	public event Action<GameObject> OnDeath;

	public enum CharacterState
	{
		Walking,
		Jumping,
		Floating
	}

	public enum CharacterOrientation
	{
		Floating,
		OnLeftWall,
		OnRightWall,
		OnFloor,
		OnCeiling
	}

	public CharacterState _state;
	public CharacterState State
	{
		get { return _state; }
		set 
		{
			_state = value; 
		}
	}

	public CharacterOrientation Orientation;

	public float WallStickRadius = 1.0f;

	public Vector2 Velocity;
	public float WalkSpeed = 1.0f;
	public float JumpSpeed = 1.0f;

	public AudioClip WalkSound;
	public AudioClip DeathSound;
	public AudioClip LandingSound;
	public AudioClip JumpSound;

	public GameObject CorpsePrefab;

	public int Health = 1;

	void Start()
	{
		State = CharacterState.Floating;
		Velocity = JumpSpeed * -Vector2.up;
	}

	protected virtual void UpdateAnimatorParams()
	{
		Animator animator = GetComponentInChildren<Animator>();
		if (animator)
		{
			animator.SetFloat("Speed", Velocity.magnitude);
		}
	}

	public float CleanUpDistance = 15;

	virtual protected void Update()
	{
		UpdateAnimatorParams();

		if (transform.position.magnitude > CleanUpDistance)
		{
			AddDamage();
		}

		if (State == CharacterState.Walking)
		{
			if (GetCharacterOrientation() == CharacterOrientation.Floating)
			{
				// Walked off edge probably..
				State = CharacterState.Floating;
			}

		}

		if (State == CharacterState.Floating)
		{
			SetCharacterOrientation(GetCharacterOrientation());
		}

		if (State == CharacterState.Jumping)
		{
			if (GetCharacterOrientation() == CharacterOrientation.Floating)
			State = CharacterState.Floating;
		}


		RaycastHit2D raycast = Physics2D.Raycast(transform.position, Velocity, WallStickRadius, BlockingLayerMask());
		if (raycast.normal == Vector2.zero)
		{
			transform.position = new Vector2(transform.position.x, transform.position.y) + Velocity * Time.deltaTime;
		}

		UpdateControl();
	}

	virtual protected void UpdateControl()
	{
	}

	public CharacterOrientation GetOrientationForNormal(Vector2 normal)
	{
		if (normal == Vector2.up)
		{
			return CharacterOrientation.OnFloor;
		}
		else if (normal == -Vector2.up)
		{
			return CharacterOrientation.OnCeiling;
		}
		else if (normal == Vector2.right)
		{
			return CharacterOrientation.OnLeftWall;
		}
		else if (normal == -Vector2.right)
		{
			return CharacterOrientation.OnRightWall;
		}
		else
		{
			return CharacterOrientation.Floating;
		}
	}

	public Vector2 GetNormalForOrientation()
	{
		Vector2 normal = Vector2.zero;
		switch (Orientation)
		{
			case CharacterOrientation.OnLeftWall:
				normal = Vector2.right;
				break;
			case CharacterOrientation.OnRightWall:
				normal = -Vector2.right;
				break;
			case CharacterOrientation.OnFloor:
				normal = Vector2.up;
				break;
			case CharacterOrientation.OnCeiling:
				normal = -Vector2.up;
				break;
			case CharacterOrientation.Floating:
				normal = Vector2.zero;
				break;
		}

		return normal;
	}

	protected void Jump(Vector2 direction)
	{
		// Direction must be in general direction of normal...
		Vector2 normal = GetNormalForOrientation();
		if (Vector2.Dot(normal, direction) <= 0)
		{
			return; // Don't jump
		}

		if (JumpSound)
			AudioSource.PlayClipAtPoint(JumpSound, transform.position);
		Velocity = direction * JumpSpeed;
		State = CharacterState.Jumping;
	}

	virtual public int BlockingLayerMask()
	{
		return LayerMask.GetMask("Wall", "EntranceDoor");
	}

	public void Walk(Vector2 direction)
	{
		FaceDirection(direction);

		// Check if there's a wall in the direction we are trying to walk...
		Velocity = direction * WalkSpeed;

		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, WallStickRadius, BlockingLayerMask());
		if (raycast.normal != Vector2.zero)
		{
			// Stick to the new wall!
			SetCharacterOrientation(GetOrientationForNormal(raycast.normal));
		}
	}

	CharacterOrientation GetCharacterOrientation()
	{
		Vector2 normal = Vector2.zero;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, WallStickRadius, BlockingLayerMask());
		foreach(var collider in colliders)
		{
			WallComponent wall = collider.GetComponent<WallComponent>();
			if (wall != null)
			{
				RaycastHit2D raycast = Physics2D.Raycast(transform.position, collider.transform.position - transform.position, float.MaxValue, BlockingLayerMask());

				// Prefer normal that matches current orientation...
				if (normal == Vector2.zero || raycast.normal == GetNormalForOrientation())
				{
					normal = raycast.normal;
				}
			}
		}

		return GetOrientationForNormal(normal);
	}

	protected virtual void OnLand()
	{
		if (LandingSound)
			AudioSource.PlayClipAtPoint(LandingSound, transform.position);
	}

	protected virtual void SetCharacterOrientation(CharacterOrientation o)
	{

		SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

		switch (o)
		{
			case CharacterOrientation.OnLeftWall:
				renderer.transform.eulerAngles = new Vector3(0, 0, -90);
				break;
			case CharacterOrientation.OnRightWall:
				renderer.transform.eulerAngles = new Vector3(0, 0, 90);
				break;
			case CharacterOrientation.OnCeiling:
				renderer.transform.eulerAngles = new Vector3(0, 0, 180);
				break;
			case CharacterOrientation.OnFloor:
			case CharacterOrientation.Floating:
				renderer.transform.eulerAngles = new Vector3(0, 0, 0);
				break;
		}

		if (Orientation == CharacterOrientation.Floating && o != CharacterOrientation.Floating)
		{
			Velocity = Vector2.zero;
			State = CharacterState.Walking;
			OnLand();
		}

		Orientation = o;
	}

	protected bool OnVerticalSurface()
	{
		return Orientation == CharacterOrientation.OnLeftWall || Orientation == CharacterOrientation.OnRightWall;
	}

	protected bool OnHorizontalSurface()
	{
		return Orientation == CharacterOrientation.OnFloor || Orientation == CharacterOrientation.OnCeiling;
	}

	protected void FaceDirection(Vector2 direction)
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
			case CharacterOrientation.OnFloor:
			case CharacterOrientation.Floating:
				if (xScale != 0)
					renderer.transform.localScale = new Vector3(xScale, 1, 1);
				break;

			case CharacterOrientation.OnCeiling:
				if (xScale != 0)
					renderer.transform.localScale = new Vector3(-xScale, 1, 1);
				break;

			case CharacterOrientation.OnLeftWall:
				if (yScale != 0)
					renderer.transform.localScale = new Vector3(-yScale, 1, 1);
				break;

			case CharacterOrientation.OnRightWall:
				if (yScale != 0)
					renderer.transform.localScale = new Vector3(yScale, 1, 1);
				break;
		}

	}

	public void AddDamage()
	{
		Health -= 1;

		if (Health <= 0)
		{
			AudioSource.PlayClipAtPoint(DeathSound, transform.position);

			if (CorpsePrefab)
			{
				// TODO - orientation?
				Instantiate(CorpsePrefab, transform.position, Quaternion.identity);
			}

			if (OnDeath != null)
			{
				OnDeath(gameObject);
			}

			Destroy(gameObject);
		}
	}
}
