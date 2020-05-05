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
	/// <summary> Move in elegant (but neausiating) arcs </summary>
	public bool useSlerp;
	/// <summary> Default settings to use </summary>
	public Settings sets;
	/// <summary> Extract info from sets </summary>
	public Vector3 worldOffset { get { return sets.worldOffset; } set { sets.worldOffset = value; } }
	/// <summary> Extract info from sets </summary>
	public float moveDampening { get { return sets.moveDampening; } set { sets.moveDampening = value; } }
	
	/// <summary> Global pixel size. </summary>
	public static float globalPixelSize = .0625f;

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

	/// <summary> Last frame's pixel perfect offset. </summary>
	Vector3 ppOffset;
	/// <summary> Camera pixel zoom </summary>
	public int pixelZoom = 4;
	
	void Update() {
		if (didPP) {
			didPP = false;
			transform.position -= ppOffset;
		}
	}

	void LateUpdate() {
		if (cam == null) { cam = GetComponent<Camera>(); }

		if (target != null) {
			if (target != lastTarget) {
				MoveExactlyToTargetLocation();
			}

			FollowCamSettings setCheck = target.GetComponent<FollowCamSettings>();
			if (setCheck != null) { sets = setCheck.settings; }

			Vector3 lastPosition = transform.position;
			Quaternion lastRotation = transform.rotation;
			MoveExactlyToTargetLocation();

			Vector3 targetPosition = transform.position;
			Quaternion targetRotation = transform.rotation;
			
			{ // Submethod, animate position/facing
				if (moveDampening > 0) {
					if (useSlerp) {
						transform.position = Vector3.Slerp(lastPosition, targetPosition, Time.deltaTime * moveDampening);
					} else {
						transform.position = Vector3.Lerp(lastPosition, targetPosition, Time.deltaTime * moveDampening);
					}
				} else {
					transform.position = targetPosition;
				}
					
			}

		}
		lastTarget = target;

		if (doPP) {
			didPP = true;
			cam.orthographic = true;
			cam.orthographicSize = cam.pixelHeight * pixelSize / pixelZoom / 2f;
			Vector3 pp = transform.position / pixelSize;
			pp.x = Mathf.Floor(pp.x);
			pp.y = Mathf.Floor(pp.y);
			pp.z = Mathf.Floor(pp.z);
			pp *= pixelSize;
			if (plusHalfPPX) { pp.x += pixelSize * .5f; }
			if (plusHalfPPY) { pp.y += pixelSize * .5f; }
			ppOffset = pp - transform.position;
			transform.position = pp;
		}

	}

	private void MoveExactlyToTargetLocation() {
		transform.position = target.position;
		transform.position += worldOffset;

		
	}
}
