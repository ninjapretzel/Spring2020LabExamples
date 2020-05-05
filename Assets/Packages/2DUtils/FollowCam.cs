using UnityEngine;
using System;

public class FollowCam : MonoBehaviour {
	
	/// <summary> Settings struct to tell a camera how to move </summary>
	[Serializable] public struct Settings {
		[Header("Offset in worldspace, ignores rotation and orbit")]
		public Vector3 worldOffset;
		[Header("Camera's movement response")]
		public float moveDampening;
	}

	/// <summary> Currently tracked target </summary>
	public Transform target = null;
	/// <summary> Target on previous frame </summary>
	public Transform lastTarget = null;
	/// <summary> Currently used Camera </summary>
	public Camera cam;
	/// <summary> Default settings to use </summary>
	public Settings sets;
	/// <summary> Extract info from sets </summary>
	public Vector3 worldOffset { get { return sets.worldOffset; } set { sets.worldOffset = value; } }
	/// <summary> Extract info from sets </summary>
	public float moveDampening { get { return sets.moveDampening; } set { sets.moveDampening = value; } }
	
	/// <summary> Global pixel size. </summary>
	public static float globalPixelSize = .0625f;

	[Header("Pixel Perfect Settings")]
	/// <summary> Toggle to disable Pixel Perfect adjustments if needed (makes it easier to see difference) </summary>
	public bool doPP = true;
	/// <summary> Was PP applied last frame? </summary>
	bool didPP = false;

	/// <summary> Toggle if object needs to get adjusted by half of the pixel size added to X </summary>
	public bool plusHalfPPX = false;

	/// <summary> Toggle if object needs to get adjusted by half of the pixel size added to Y </summary>
	public bool plusHalfPPY = false;

	/// <summary> Current pixelSize </summary>
	public float pixelSize = .0625f;

	/// <summary> Last frame's pixel perfect offset. </summary>
	Vector3 ppOffset;
	/// <summary> Camera pixel zoom </summary>
	public int pixelZoom = 4;

	/// <summary> Called by Unity every frame </summary>
	void Update() {
		// Only undo if we actually did previous frame (in case `doPP` is toggled when live)
		if (didPP) {
			didPP = false;
			transform.position -= ppOffset;
		}
	}

	/// <summary> Called by Unity every frame, after every object has had its Update. </summary>
	void LateUpdate() {
		// Make sure we have a camera.
		if (cam == null) { cam = GetComponent<Camera>(); }
		// If we still don't have one, exit.
		if (cam == null) { return; }

		// If we have a target...
		if (target != null) {
			// Grab settings from the target
			FollowCamSettings setCheck = target.GetComponent<FollowCamSettings>();
			// Copy them if they exist.
			if (setCheck != null) { sets = setCheck.settings; }

			// And it is not the one we had last frame...
			if (target != lastTarget) {
				// Move exactly to where we need to be.
				MoveExactlyToTargetLocation();
			}

			// Mark our current location
			Vector3 lastPosition = transform.position;
			// Move to where we need to be 
			MoveExactlyToTargetLocation();
			// Mark the target location 
			Vector3 targetPosition = transform.position;
			
			if (moveDampening > 0) {
				// If move dampening is set, animate towards the target
				transform.position = Vector3.Lerp(lastPosition, targetPosition, Time.deltaTime * moveDampening);
			} else {
				// Otherwise set exactly on the target position.
				transform.position = targetPosition;
			}
					
		}
			
		// Mark current target as previous target next frame
		lastTarget = target;

		// Apply pixel perfect changes
		if (doPP) {
			// Mark as PP applied
			didPP = true;
			// Force camera into orthographic size with pixel size/zoom applied
			cam.orthographic = true;
			cam.orthographicSize = cam.pixelHeight * pixelSize / pixelZoom / 2f;
			// Calculate offset to be placed on pixel grid
			Vector3 pp = transform.position / pixelSize;
			pp.x = Mathf.Floor(pp.x);
			pp.y = Mathf.Floor(pp.y);
			pp.z = Mathf.Floor(pp.z);
			pp *= pixelSize;
			// If we are set to add half to X or Y (to fix odd alignments)
			if (plusHalfPPX) { pp.x += pixelSize * .5f; }
			if (plusHalfPPY) { pp.y += pixelSize * .5f; }
			// Calculate offset from current position, and apply
			ppOffset = pp - transform.position;
			transform.position = pp;
		}

	}

	/// <summary> Helper function to move exactly to where the camera needs to be </summary>
	/// <remarks> In other versions of this camera (eg a 3d version) this is more complex. 
	/// Still useful to extract this since the exact logic is used twice. </remarks>
	private void MoveExactlyToTargetLocation() {
		transform.position = target.position;
		transform.position += worldOffset;
	}
}
