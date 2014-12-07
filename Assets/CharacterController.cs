﻿using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
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

	void Start()
	{
		State = CharacterState.Floating;
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

		transform.position = new Vector2(transform.position.x, transform.position.y) + Velocity * Time.deltaTime;

		//QueryInput();
	}

	CharacterOrientation GetCharacterOrientation()
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
					return CharacterOrientation.OnFloor;
				}
				else if (raycast.normal == -Vector2.up)
				{
					return CharacterOrientation.OnCeiling;
				}
				else if (raycast.normal == Vector2.right)
				{
					return CharacterOrientation.OnLeftWall;
				}
				else if (raycast.normal == -Vector2.right)
				{
					return CharacterOrientation.OnRightWall;
				}
			}
		}

		return CharacterOrientation.Floating;
	}

	void SetCharacterOrientation(CharacterOrientation o)
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
		}

		Orientation = o;
	}

	bool OnVerticalSurface()
	{
		return Orientation == CharacterOrientation.OnLeftWall || Orientation == CharacterOrientation.OnRightWall;
	}

	bool OnHorizontalSurface()
	{
		return Orientation == CharacterOrientation.OnFloor || Orientation == CharacterOrientation.OnCeiling;
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

}
