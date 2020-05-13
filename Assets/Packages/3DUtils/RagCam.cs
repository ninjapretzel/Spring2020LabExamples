using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Behaviour for a stylish camera, based off the one in "Rangarok Online", but with a few extra features added. </summary>
public class RagCam : MonoBehaviour {
	
	/// <summary> How quickly the rotation velocity returns to zero </summary>
	public float rotationStopDampening = 11;
	/// <summary> How sensitive the camera is to mouse movements </summary>
	public Vector2 sensitivity = new Vector2(25, 15);
	/// <summary> Current camera rotation velocity </summary>
	public Vector2 velocity;
	/// <summary> Minimum bound for pitch rotation </summary>
	public float xmin = 30;
	/// <summary> Maximum bound for pitch rotation </summary>
	public float xmax = 60;

	/// <summary> How quickly camera zoom responds to inputs </summary>
	public float zoomDampening = 5f;
	/// <summary> Current zoom distance target </summary>
	public float zoomTarget = 11f;

	/// <summary> Minimum bound for zoom distance </summary>
	public float zoomMin = 5f;
	/// <summary> Maximum bound for zoom distance </summary>
	public float zoomMax = 50f;
	/// <summary> Proportion of zoom per zoom step </summary>
	public float zoomRate = 1.05f;
	
	/// <summary> Does the mouse push (scroll) the camera? </summary>
	public bool mousePushCamera = false;
	/// <summary> How quickly does the camera respond to mouse movements? </summary>
	public float pushResponse = 5f;
	/// <summary> How far does the mouse movement push the camera in given directions? </summary>
	public Vector3 camPushRate = new Vector3(.5f, 0, .5f);
	/// <summary> Always push the camera this far </summary>
	public Vector3 camHardPush = Vector3.zero;
	/// <summary> Final offset to camera position </summary>
	public Vector3 hardOffset = Vector3.zero;

	/// <summary> System to prevent the mouse clicks from being detected </summary>
	public bool useMouseBlacklist = false;
	/// <summary> Should the cursor be hidden during rotation? </summary>
	public bool showCursorWhileRotating = true;
	/// <summary> Base area to consider mouse clicks for beginning to rotate the camera </summary>
	public Rect mouseScrollArea = new Rect(0, 0, 1, 1);
	/// <summary> Rects to ignore mouse clicks to begin rotating the camera </summary>
	public Rect[] rectBlacklist = new Rect[0];

	/// <summary> Do operation in LateUpdate if true, or Update if false. </summary>
	public bool doLate = true;

	/// <summary> Flag if mouse rotate button (Right click) is held </summary>
	bool mouseRotate = false;
	
	/// <summary> Run logic if <see cref="doLate"/> is unset. </summary>
	void Update() {
		if (!doLate) { Go(); }
	}

	/// <summary> Run logic if <see cref="doLate"/> is set. </summary>
	void LateUpdate() {
		if (doLate) { Go(); }
	}

	/// <summary> Logic for mouse movement </summary>
	void Go() {
		// Check for blacklist, mouse within a valid area, and sample button press
		// (for example, we wouldn't want a click over a UI window to begin rotating the camera)
		bool mouseEnabled = !useMouseBlacklist || MouseInRects();
		bool mousePress = Input.GetMouseButton(1);
		
		// Update mouse rotation state. 
		if (!mouseRotate) {
			// If not rotating and mouse pressed, begin rotating if mouse was pressed
			if (mouseEnabled) {
				mouseRotate = mousePress;
			}
		} else {
			// else if rotating, end rotation if mouse was released. 
			if (!Input.GetMouseButton(1)) {
				mouseRotate = false;
			}
		}
		
		// If we are rotating...
		if (mouseRotate) {
			// Hide cursor if we are supposed to, but at least confine it to the window.
			Cursor.visible = showCursorWhileRotating;
			Cursor.lockState = CursorLockMode.Confined;
			// And add the mouse movement deltas to our velocity, scaled by sensitivity.
			velocity.x += Input.GetAxis("Mouse X") * sensitivity.x;
			velocity.y -= Input.GetAxis("Mouse Y") * sensitivity.y;
			
		} else {
			// Otherwise, show and unlock cursor
			// (Note, doing this every frame could interfere with 
			// other scripts controlling those properties!)
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		// Sample scroll wheel, again, only if cursor is in a valid area.
		// (same deal as above, don't want to change camera zoom if the user is scrolling some UI)
		float scroll = (mouseEnabled || mouseRotate) ? -Input.GetAxis("Mouse ScrollWheel") : 0;
		if (Mathf.Abs(scroll) > 0) {
			// Zoom in or out by zoom-rate steps every time the scroll wheel is flicked
			zoomTarget *= (scroll > 0) ? zoomRate : (1f/zoomRate);
			zoomTarget = Mathf.Clamp(zoomTarget, zoomMin, zoomMax);
		}

		// Call below logic to reposition camera by updating the FollowCam (if it exists)
		UpdateFollowCam();

		// 
		velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * rotationStopDampening);

	}

	/// <summary> Update <see cref="FollowCam"/> attached to this object </summary>
	private void UpdateFollowCam() {
		var cam = GetComponent<FollowCam>();
		if (cam != null) {

			// Get rotation from FollowCam
			Vector3 rot = cam.orbitRotation;
			// Add velocity to it over time
			rot += new Vector3(velocity.y, velocity.x, 0) * Time.deltaTime;
			// Clamp pitch between min/max limits
			rot.x = Mathf.Clamp(rot.x, xmin, xmax);

			// Set the updated rotation into the camera.
			cam.orbitRotation = rot;

			// Update the zoom of the camera similarly.
			float zoom = cam.orbitDistance;
			zoom = Mathf.Lerp(zoom, zoomTarget, Time.deltaTime * zoomDampening);
			cam.orbitDistance = zoom;

			Vector3 push = Vector3.zero;
			if (mousePushCamera) {
				// Update "push" distance based on mouse position on screen 
				push = Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) /2f;
				push.x /= (Screen.width/2f);
				push.y /= (Screen.height/2f);
				if (push.sqrMagnitude > 1.0f) { push.Normalize(); }
				push.z = push.y;
				push.y = 0;

			}

			// Apply push based on camera's orientation, with the 'y' component flattened (so cam is pushed along x/z)
			push += camHardPush;
			push.x *= camPushRate.x;
			push.z *= camPushRate.z;
			Vector3 flatFow = transform.forward;
			flatFow.y = 0; flatFow.Normalize();
			Vector3 flatRight = transform.right;
			flatRight.y = 0; flatRight.Normalize();

			// Calculate targetOffset based on all position adjustments
			Vector3 targetOffset = (flatFow * push.z + flatRight * push.x) * zoom + hardOffset;
			// Move FollowCam's offset towards that targetOffset
			cam.worldOffset = Vector3.Lerp(cam.worldOffset, targetOffset, Time.deltaTime * pushResponse);
		}
	}

	/// <summary> See if the mouse position is in a valid area for inputs to be directed to camera controls. </summary>
	/// <returns> True if mouse is within a valid area, false otherwise. </returns>
	public bool MouseInRects() {
		// Normalize positions to be between [0,1]
		Vector2 mousePos = Input.mousePosition;
		Vector2 normalizedPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

		// Check for mouse within primary rectangle 
		if (mouseScrollArea.Contains(normalizedPos)) {
			// Check for mouse within any blacklisted rects
			// These may come from UI of some sort.
			foreach (Rect blacklist in rectBlacklist) {
				if (blacklist.Contains(normalizedPos)) { return false; }
			}

			return true;
		}
		
		return false;
	}
}
