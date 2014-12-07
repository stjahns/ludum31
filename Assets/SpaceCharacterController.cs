using UnityEngine;
using System.Collections;
using System;

public class SpaceCharacterController : MonoBehaviour
{
	public event Action<GameObject> OnDeath;

	public bool HumanControlled = false;
	public bool WalkingIn = false;

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

	virtual public int BlockingLayerMask()
	{
		if (WalkingIn || HumanControlled)
		{ 
			return LayerMask.GetMask("Wall");
		}
		return LayerMask.GetMask("Wall", "EntranceDoor", "PlayerOnly");
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

	protected virtual void Start()
	{
		State = CharacterState.Floating;
		FaceDirection(-Vector2.right);
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

	void EscapeFromEvilCorner()
	{
		// Have zero velocity and not close enough to corner because we a circle?
		Collider2D collider = Physics2D.OverlapCircle(transform.position, WallStickRadius + 0.5f, BlockingLayerMask());
		if (collider)
		{
			var direction = transform.position - collider.transform.position;
			Velocity = JumpSpeed * direction.normalized;
			AudioSource.PlayClipAtPoint(JumpSound, transform.position);
		}
		else
		{ 
			// Give up on life :( 
			AddDamage();
		}
	}

	virtual protected void Update()
	{
		UpdateAnimatorParams();

		if (WalkingIn)
			return;

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
			if (Velocity == Vector2.zero)
			{
				EscapeFromEvilCorner();
			}

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

	protected virtual void UpdateControl()
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

	protected virtual void GetAutomatedControl()
	{
	}


	protected Vector2 GetRandomJumpDirection()
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

		int index = UnityEngine.Random.Range(0, 2);
		return directions[index];
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

	bool ShouldBlockJump(Vector2 direction)
	{
		// Direction must be in general direction of normal...
		Vector2 normal = GetNormalForOrientation();
		if (Vector2.Dot(normal, direction) <= 0)
		{
			return true; // Don't jump
		}

		Vector2 tangent = new Vector2(normal.y, normal.x);

		// Don't jump if we are in a corner
		RaycastHit2D raycast;

		float checkDistance = WallStickRadius;
		if (Vector2.Dot(tangent, direction) > 0)
		{
			// If we are closeish to a corner and jump diagonally into it we can also get stuck... increase distance to check..
			checkDistance *= 2;
		}
		raycast = Physics2D.Raycast(transform.position, tangent, checkDistance, BlockingLayerMask());
		if (raycast.normal != Vector2.zero)
			return true;

		checkDistance = WallStickRadius;
		if (Vector2.Dot(-tangent, direction) > 0)
		{
			checkDistance *= 2;
		}
		raycast = Physics2D.Raycast(transform.position, -tangent, checkDistance, BlockingLayerMask());
		if (raycast.normal != Vector2.zero)
			return true;

		// ALSO don't jump if we are under a low ceiling...
		raycast = Physics2D.Raycast(transform.position, normal, WallStickRadius + 0.1f, BlockingLayerMask());
		if (raycast.normal != Vector2.zero)
			return true;

		return false;
	}

	protected void Jump(Vector2 direction)
	{
		if (ShouldBlockJump(direction))
			return;
		

		if (JumpSound)
			AudioSource.PlayClipAtPoint(JumpSound, transform.position);
		Velocity = direction * JumpSpeed;
		State = CharacterState.Jumping;
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

	protected Vector2 GetJumpDirection()
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
			direction = GetNormalForOrientation();
		}

		return direction.normalized;
	}

	protected virtual void GetHumanControl()
	{
		if (State == CharacterState.Walking)
		{
			if (OnVerticalSurface())
			{
				if (Input.GetKey(KeyCode.W))
				{
					// move up if on wall
					Walk(Vector2.up);
					FaceDirection(Vector2.up);
				}
				else if (Input.GetKey(KeyCode.S))
				{
					// move down if on wall
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
					Walk(-Vector2.right);
					FaceDirection(-Vector2.right);
				}
				else if (Input.GetKey(KeyCode.D))
				{
					// move right if on floor / ceiling
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

	public Vector2 GetClosestPlayerDirection()
	{
		Vector2 bestDirection = Vector2.zero;

		if (Game.Player)
		{
			Vector3 direction = Game.Player.transform.position - transform.position;

			RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, 100, LayerMask.GetMask("Wall", "EntranceDoor", "Player"));

			// Check line of sight
			if (raycast.collider != null && raycast.collider.GetComponent<SpaceCharacterController>() == Game.Player)
			{
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
			}
		}

		return bestDirection;
	}
}
