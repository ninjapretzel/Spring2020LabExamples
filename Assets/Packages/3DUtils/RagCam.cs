using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RagCam : MonoBehaviour {
	
	public float rotationDampening = 11;
	public Vector2 sensitivity = new Vector2(25, 15);
	public Vector2 velocity;
	public float xmin = 30;
	public float xmax = 60;

	public float zoomDampening = 5f;
	public float zoomTarget = 11f;

	public float zoomMin = 5f;
	public float zoomMax = 50f;
	public float zoomRate = 1.05f;
	
	public bool mousePushCamera = false;
	public float pushResponse = 5f;
	public Vector3 camPushRate = new Vector3(.5f, 0, .5f);
	public Vector3 camHardPush = Vector3.zero;
	public Vector3 hardOffset = Vector3.zero;

	public bool useMouseBlacklist = false;
	public Rect mouseScrollArea = new Rect(0, 0, 1, 1);
	public Rect[] rectBlacklist = new Rect[0];
	public bool doLate = true;

	bool mouseRotate = false;

	void Awake() {
		
	}
	
	void Start() {
		
	}

	void Update() {
		if (!doLate) { Go(); }
		
	}

	void LateUpdate() {
		if (doLate) { Go(); }

	}
	void Go() {

		bool mouseEnabled = !useMouseBlacklist || MouseInRects();
		bool mousePress = Input.GetMouseButton(1);
		

		if (!mouseRotate) {
			if (mouseEnabled) {
				mouseRotate = mousePress;
			}
		} else {
			if (!Input.GetMouseButton(1)) {
				mouseRotate = false;
			}
		}

		

		if (mouseRotate) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;
			velocity.x += Input.GetAxis("Mouse X") * sensitivity.x;
			velocity.y -= Input.GetAxis("Mouse Y") * sensitivity.y;
			
		} else {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		float scroll = (mouseEnabled || mouseRotate) ? -Input.GetAxis("Mouse ScrollWheel") : 0;
		if (Mathf.Abs(scroll) > 0) {
			if (scroll > 0) {
				zoomTarget *= zoomRate;
			} else {
				zoomTarget /= zoomRate;
			}

			zoomTarget = Mathf.Clamp(zoomTarget, zoomMin, zoomMax);
		}

		UpdateFollowCam();

		velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * rotationDampening);

	}

	private void UpdateFollowCam() {
		var cam = GetComponent<FollowCam>();
		if (cam) {

			Vector3 rot = cam.orbitRotation;
			rot += new Vector3(velocity.y, velocity.x, 0) * Time.deltaTime;
			rot.x = Mathf.Clamp(rot.x, xmin, xmax);

			cam.orbitRotation = rot;


			float zoom = cam.orbitDistance;
			zoom = Mathf.Lerp(zoom, zoomTarget, Time.deltaTime * zoomDampening);
			cam.orbitDistance = zoom;

			Vector3 push = Vector3.zero;
			if (mousePushCamera) {
				push = Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) /2f;
				push.x /= (Screen.width/2f);
				push.y /= (Screen.height/2f);
				if (push.sqrMagnitude > 1.0f) { push.Normalize(); }
				push.z = push.y;
				push.y = 0;

			}

			push += camHardPush;
			push.x *= camPushRate.x;
			push.z *= camPushRate.z;
			Vector3 flatFow = transform.forward;
			flatFow.y = 0; flatFow.Normalize();
			Vector3 flatRight = transform.right;
			flatRight.y = 0; flatRight.Normalize();

			Vector3 offset = (flatFow * push.z + flatRight * push.x) * zoom + hardOffset;
			cam.worldOffset = Vector3.Lerp(cam.worldOffset, offset, Time.deltaTime * pushResponse);


			

		}
	}

	public bool MouseInRects() {
		Vector2 mousePos = Input.mousePosition;
		Vector2 normalizedPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

		if (mouseScrollArea.Contains(normalizedPos)) {

			foreach (Rect blacklist in rectBlacklist) {
				if (blacklist.Contains(normalizedPos)) { return false; }
			}

			return true;
		}


		return false;
	}
}
