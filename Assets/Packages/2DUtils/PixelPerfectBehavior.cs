using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Base behavior class for anything intending to be pixel-perfect. </summary>
public class PixelPerfectBehavior : MonoBehaviour {
	/// <summary> Global pixel size. </summary>
	public static float globalPixelSize = .0625f;

	/// <summary> Draw some debugging information </summary>
	public bool DEBUG_DRAW = false;

	[Header("Pixel Perfect Settings")]
	/// <summary> Toggle to disable Pixel Perfect adjustments if needed </summary>
	public bool doPP = true;
	/// <summary> Was PP applied last frame? </summary>
	bool didPP = false;

	/// <summary> Toggle if object needs to get adjusted by half of the pixel size added to X </summary>
	public bool plusHalfPPX = false;

	/// <summary> Toggle if object needs to get adjusted by half of the pixel size added to Y </summary>
	public bool plusHalfPPY = false;

	/// <summary> Current pixelSize </summary>
	public float pixelSize = .0625f;

	/// <summary> Attached spriteAnimator </summary>
	public SpriteAnimator spriteAnimator;

	/// <summary> Last frame's pixel perfect offset. </summary>
	Vector3 ppOffset;

	[Header("Collision Stuff")]
	/// <summary> Physics collider </summary>
	public Collider2D col;
	/// <summary> Distance to snap to ground surface. Default = 1/16 </summary>
	public float snapDistance = 0.0625f;

	/// <summary> Preallocated array for collisions </summary>
	private Collider2D[] collisions = new Collider2D[16];
	/// <summary> Preallocated array for collisions </summary>
	private RaycastHit2D[] raycastHits = new RaycastHit2D[16];
	/// <summary> Preallocated array for trigger caching </summary>
	public Collider2D[] triggers { get; private set; } = new Collider2D[16];

	/// <summary> Applies pixel perfection to this object's position. Intended to be called in LateUpdate, or somehow before rendering. </summary>
	public void ApplyPixelPerfect() {
		// If doPP is set, apply pixel perfect offset 
		if (doPP) {
			// Mark as applied
			didPP = true;
			// Find placement on exact pixel grid 
			Vector3 pp = transform.position / pixelSize;
			pp.x = Mathf.Floor(pp.x);
			pp.y = Mathf.Floor(pp.y);
			pp.z = Mathf.Floor(pp.z);
			pp *= pixelSize;
			// Adjustment for odd alignments
			if (plusHalfPPX) { pp.x += pixelSize * .5f; }
			if (plusHalfPPY) { pp.y += pixelSize * .5f; }
			// Calculate offset from current position and apply
			ppOffset = pp - transform.position;
			transform.position = pp;
		}
	}
	
	/// <summary> Resets pixel perfection, Intended to be called at the begining of Update, or somewhere after rendering. </summary>
	public void ResetPixelPerfect() {
		// Find attached animator.
		if (spriteAnimator == null) { spriteAnimator = GetComponentInChildren<SpriteAnimator>(); }
		// Only reset if was applied
		if (didPP) {
			didPP = false;
			transform.position -= ppOffset;
		}
	}

	/// <summary> Debug helper to draw boxes </summary>
	/// <param name="p"> Center of box </param>
	/// <param name="s"> Size of box </param>
	/// <param name="color"> Optional color to use (Defaults to <see cref="Color.white"/> if not provided) </param>
	protected void DrawBox(Vector3 p, Vector3 s, Color? color = null) {
		// Unpack color from nullable (Color?) or used default
		Color c = color ?? Color.white;
		// Pre-calc corners
		Vector3 c1 = s;
		Vector3 c2 = s; c2.x *= -1;
		Vector3 c3 = s; c3.x *= -1; c3.y *= -1;
		Vector3 c4 = s; c4.y *= -1;
		// Draw lines that make up box
		Debug.DrawLine(p + c1, p + c2, c);
		Debug.DrawLine(p + c3, p + c2, c);
		Debug.DrawLine(p + c3, p + c4, c);
		Debug.DrawLine(p + c1, p + c4, c);
	}
	
	/// <summary> Attempt to move the object, and get how far they actually moved. </summary>
	/// <param name="movement"> Requested movement </param>
	/// <returns> Applied movement </returns>
	public Vector3 Move(Vector3 movement) {
		// Get our collider 
		col = GetComponent<Collider2D>();
		// Start out with zero movement 
		Vector3 moved = Vector3.zero;
		// Try to move the whole distance over y...
		moved += DoMove(new Vector3(0, movement.y, 0));
		// If we don't move anything, 
		while (moved.y == 0 && Math.Abs(movement.y) > snapDistance) {
			// repeatedly try to move half the distance down to `snapDistance` until we actually move.
			movement.y *= .5f;
			moved += DoMove(new Vector3(0, movement.y, 0));
		}
		// Do the same thing over x:
		// Try to move the whole distance over x...
		moved += DoMove(new Vector3(movement.x, 0, 0));
		// If we don't move anything, 
		while (moved.x == 0 && Math.Abs(movement.x) > snapDistance) {
			// repeatedly try to move half the distance down to `snapDistance` until we actually move.
			movement.x *= .5f;
			moved += DoMove(new Vector3(movement.x, 0, 0));
		}
		// Return whatever distance we actually moved.
		return moved;
	}

	/// <summary> Attempt to move the object, and get how far they actually moved. `</summary>
	/// <param name="movement"> Requested movement. </param>
	/// <returns> Applied movement </returns>
	public Vector3 DoMove(Vector3 movement) {
		// Start by assuming we move
		bool move = true;

		// Make sure we have a collider
		if (col != null) {
			// AABB (Axis-Aligned Bounding Box) collsion checking is fairly simple,
			// And we simply assume for BoxCollider2D we have an AABB.
			if (col is BoxCollider2D) {
				BoxCollider2D box = col as BoxCollider2D;
				// Calculate where we will be.
				Vector3 point = box.transform.position + (Vector3)box.offset + movement;
				// Debug draw the box 
				DrawBox(point, box.size / 2f, Color.blue);
				// Tell the physics system to cast our collider into the scene at the target location
				// This gives us back the count of colliders we overlapped, 
				// and fills the `collisions` array with information about them.
				int numCollisions = Physics2D.OverlapBoxNonAlloc(point, box.size, 0, collisions);
				// loop over collisions to check for solids...
				for (int i = 0; i < numCollisions; i++) {
					// Skip our own collider
					if (collisions[i] == col) { continue; }
					// And if we hit any solids (non-triggers), 
					if (!collisions[i].isTrigger) {
						// don't move and exit.
						move = false;
						break;
					}
				}
			}
			// Todo: Other collision primitives.
		}

		// If we won't hit anything, move the object and return the delta.
		if (move) {
			transform.position = transform.position + movement;
			return movement;
		}
		// Otherwise return zero.
		return Vector3.zero;
	}


	/// <summary> Check the current position, and get the number of overlapping triggers. </summary>
	/// <returns> Number of overlapping triggers stored in <see cref="triggers"/> </returns>
	public int CheckTriggers() {
		// Nullify all members in pre-allocated array.
		int numTriggers = 0;
		Array.Clear(triggers, 0, triggers.Length);

		// Require an attached collider.
		if (col != null) {
			// AABB (Axis-Aligned Bounding Box) collsion checking is fairly simple,
			// And we simply assume for BoxCollider2D we have an AABB.
			if (col is BoxCollider2D) {
				// Get collider and center point
				BoxCollider2D box = col as BoxCollider2D;
				Vector3 point = box.transform.position;
				// Similarly to DoMove, ask for information about what overlaps our collider
				// However, this should be called after having actually moved.
				int numCollisions = Physics2D.OverlapBoxNonAlloc(point, box.size, 0, collisions);
				// Loop over overlapped colliders, look for triggers this time.
				for (int i = 0; i < numCollisions; i++) {
					// Skip our own collider
					if (collisions[i] == col) { continue; }
					// Insert any triggers into our pre-allocated triggers array
					// Keep track of how many we add.
					if (collisions[i].isTrigger) {
						triggers[numTriggers++] = collisions[i];
					}
				}
			}
			// Todo: Other collision primitives.
		}

		// Return the number of triggers we overlapped if any.
		return numTriggers;
	}

	/// <summary> Check if the object would touch something when their collider is swept along a direction </summary>
	/// <param name="sweep"> Sweep direction/distance vector </param>
	/// <returns> True if they would hit something, false otherwise. </returns>
	public bool IsTouching(Vector3 sweep) {
		// Require an attached collider.
		if (col != null) {
			// AABB (Axis-Aligned Bounding Box) collsion checking is fairly simple,
			// And we simply assume for BoxCollider2D we have an AABB.
			if (col is BoxCollider2D) {
				// Get collider and center point
				BoxCollider2D box = col as BoxCollider2D;
				Vector3 point = box.transform.position + (Vector3)box.offset;
				if (DEBUG_DRAW) { 
					// Draw the touching check.
					DrawBox(point, box.size * .5f);
					DrawBox(point + sweep, box.size * .5f, Color.cyan);
				}

				// Sweep box downwards, see if we collide with anything.
				int numCollisions = Physics2D.BoxCastNonAlloc(point, box.size, 0, sweep, raycastHits, sweep.magnitude + snapDistance);
				if (numCollisions > 0) {
					for (int i = 0; i < numCollisions; i++) {
						// Skip own collider.
						if (raycastHits[i].collider == col) { continue; } 
						// Look for any touched solids.
						if (!raycastHits[i].collider.isTrigger) {
							return true;
						}
					}
				}
			}
		}

		// If nothing was touched, we aren't touching anything.
		return false;
	}
}
