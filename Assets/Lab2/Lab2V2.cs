using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab2 {
	// For my implementation of V2, I avoid using the built-in Animator/Animation types, as well as Rigidbody2D.

	// Most things you want characters to do will not be accurate to IRL physics, so for characters,
	// a Rigidbody is not a good basis. Additionally, often when you have something specific in mind,
	// you will want to be able to directly control what frames of animation are being displayed.

	// With Unity's animation setup (which is designed for keyframed 3d animations, not frame-perfect 2d animations),
	// it is very difficult to have decent control over the character's animations.
	
	// Note this derives from PixelPerfectBehavior,
	// this is a type I wrote to handle moving to exact positions before the camera renders,
	// in order to never have misaligned pixels (which would never happen on an old console)
	// I expanded it with a little bit of extra code that handles checking for collisions using casts.
	public class Lab2V2 : PixelPerfectBehavior {

		/// <summary> Movement speed. </summary>
		[Header("Movement Section")]
		public float walkSpeed = 7f;
		/// <summary> Downward acceleration due to gravity. </summary>
		public float gravity = 33f;
		/// <summary> Maximum -y velocity. </summary>
		public float terminalVelocity = 45f;
		/// <summary> Upward acceleration applied when jumping </summary>
		public float jumpPower = 15.0f;
		/// <summary> Additional area around collider to consider "colliding" </summary>
		public float skinWidth = .125f;
		
		/// <summary> Default x direction. Positive if sprite faces right, Negative if sprite faces left. </summary>
		[Header("Animations")]
		public float defaultXFacing = 1.0f;
		// These types are asset files (a type I made) that store animation data
		// this gives me easier control over animation frames, frame holding/repeating, and animation speed,
		// as well as creating animations that loop with mirroring over X/Y (which is a common trick)
		/// <summary> Animation for walking </summary>
		public SpriteAnimAsset walk;
		/// <summary> Animation for idle </summary>
		public SpriteAnimAsset idle;
		/// <summary> Animation for rising </summary>
		public SpriteAnimAsset jump;
		/// <summary> Animation for falling </summary>
		public SpriteAnimAsset fall;

		/// <summary> Track and skip first few frames (to not phase through ground, first frame can be long!) </summary>
		int frameNumber = 0;
		/// <summary> Number of frames to skip </summary>
		public int skipFrames = 2;

		/// <summary>current facing direction (1.0 means right, -1.0 means left) </summary>
		float facing;
		/// <summary> Input for the current frame </summary>
		Vector3 input;
		/// <summary> Movement applied for the current frame </summary>
		Vector3 movement;
		/// <summary> Distance actually moved from above applied movement </summary>
		Vector3 moved;
		/// <summary> Velocity between frames (to store gravity, and eventually physics movement) </summary>
		Vector3 velocity;

		/// <summary> Is the character currently on the ground this frame? </summary>
		bool isGrounded;
		/// <summary> Did the character bump their head this frame? </summary>
		bool bumpedHead;

		/// <summary> See if we are currently grounded.</summary>
		public bool CheckGrounded() { return velocity.y <= 0 && IsTouching(Vector2.down * skinWidth); }
		/// <summary> Check if we will touch the ground during the next frame </summary>
		public bool CheckWillTouchGround() { return velocity.y <= 0 && IsTouching(new Vector2(0, velocity.y * Time.deltaTime)); }
		/// <summary> See if we are touching the ceiling </summary>
		public bool CheckBumpedHead() { return velocity.y > 0 && IsTouching(Vector2.up * skinWidth); }
		
		/// <summary> Called by Unity, just before this object's first <see cref="Update"/>. </summary>
		void Start() {
			// Face right on start
			facing = 1;
			// Don't phase through the ground if the first frame takes long to load!
			frameNumber = 0;
		}

		/// <summary> Called by Unity every frame, after every object has had its Update. </summary>
		void LateUpdate() {
			// Apply the Pixel-Perfect adjustment for this frame (LateUpdate happens just before the cameras render!)
			ApplyPixelPerfect();
		}

		/// <summary> Called by unity every frame. </summary>
		void Update() {
			// This un-applies the Pixel-Perfect adjustment for the last frame.
			ResetPixelPerfect();
			// Check for first frame, unset it and exit if it is.
			if (frameNumber < skipFrames) {
				frameNumber++;
				return;
			}

			// Sample input
			input = Vector3.zero;
			input.x = Input.GetAxisRaw("Horizontal");
			// Update facing direction if given movement input
			if (input.x != 0) {
				facing = Mathf.Sign(input.x);
			}

			if (velocity.y > 0 && Input.GetButtonUp("Jump")) {
				// Allow the player to stop jumping upwards by releasing jump
				// This goes a long way towards the "game feel" being responsive.
				velocity.y = 0; 
			}

			if (isGrounded) {
				// On the ground? reset y velocity
				velocity.y = 0;
				// If the jump button was hit, send them into the air.
				if (Input.GetButtonDown("Jump")) {
					velocity.y = jumpPower;
				}
			} else {
				// If the head was bumped last frame, stop rising
				if (bumpedHead) { velocity.y = 0; }
				// If in the air, apply gravity.
				velocity.y -= gravity * Time.deltaTime;
			}
			// If we're falling too fast, cap the falling speed
			if (velocity.y < -terminalVelocity) { velocity.y = -terminalVelocity; }

			// Calculate base movement-per-second to apply
			movement = Vector3.zero;
			movement.x = input.x * walkSpeed;
			movement += velocity;

			// Apply movement, and get back vector of how far we actually moved.
			// This is actually quite a bit of work to do- it involves checking for collisions!
			// If the object collides with things when we try to move it, it does not move as far.
			// Also requires a `BoxCollider2D` to be attached to this `GameObject`, which is used to check for collisions.
			moved = Move(movement * Time.deltaTime);

			// Update if we are grounded or bumped our head
			isGrounded = CheckGrounded();
			bumpedHead = CheckBumpedHead();
			// Slow velocity to approach ground gracefully (always land with minimal gap)
			if (velocity.y < 0 && CheckWillTouchGround()) {
				velocity.y *= .5f;
			}

			UpdateAnimation();

		}

		/// <summary> Replaces the animation state machine. Derive next sprite from current state. </summary>
		void UpdateAnimation() {
			// First, make sure we have a SpriteAnimator (type I made)
			if (spriteAnimator == null) {
				spriteAnimator = GetComponent<SpriteAnimator>();
			}
			if (spriteAnimator == null) {
				// If we don't already have one on us, find one underneath us.
				Transform check = transform.Find("Sprite");
				if (check != null) { spriteAnimator = check.GetComponentInChildren<SpriteAnimator>(); }
			}
			if (spriteAnimator == null) { /* If it's still null, exit. */ return; }

			// Update facing 
			spriteAnimator.flipX = facing != Mathf.Sign(defaultXFacing);
			
			if (isGrounded) {
				// If we're on the ground,
				// We are idle if not moving, otherwise walking
				spriteAnimator.anim = (moved.x == 0) ? idle : walk;
			} else {
				// In the air,
				// We are jumping if moving upwards, otherwise falling.
				spriteAnimator.anim = (velocity.y > 0) ? jump : fall;
			}
			
			// The nice thing about doing things this way is with complex systems with lots of animations
			// you do not have to create all of the transitions explicitly,
			// based on the path through the code, the transitions occur naturally.

			// Unity's state machines work for certain kinds of games,
			// but also make it difficult to say, swap all of the sprites within an animation set

			// This example is based off of a system I wrote that could share animation sets between characters
			// given all of the animations were named similarly
		}

	}
}
