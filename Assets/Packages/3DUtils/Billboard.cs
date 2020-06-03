using UnityEngine;

/// <summary> Commons Behaviour for 'bilboarding' an entire transform. </summary>
public class Billboard : MonoBehaviour {

	/// <summary> Set this to override what camera is going to be looked at. </summary>
	public Camera targetOverride = null;

	/// <summary> Camera to look at</summary>
	Camera target = null;

	/// <summary> Set this to use the camera's up vector as the local up vector. </summary>
	public bool useCameraUp = false;

	/// <summary> Does this billboard match camera rotation instead of looking at the camera? </summary>
	public bool matchRotationMode = false;

	/// <summary> Flip the object 180 degrees on the Y axis after billboarding. </summary>
	public bool flip = false;

	/// <summary> Speed of rotation along the z axis. </summary>
	public float zRotationSpeed = 0;

	/// <summary> Current rotation on the z-axis.</summary>
	float zRotation = 0;

	public bool doLate = false;

	void Update() {
		if (!doLate) { DoBillboard(); }
	}

	void LateUpdate() {
		if (doLate) { DoBillboard(); }
	}

	private void DoBillboard() {
		// If target is null, use main camera.
		if (target == null) { target = Camera.main; }
		// Set camera if override is not null 
		if (targetOverride != null) { target = targetOverride; targetOverride = null; }

		// If we are simply to match the rotation, copy it directly
		if (matchRotationMode) {
			transform.rotation = target.transform.rotation;
		} else {
			// Otherwise we look at the target
			if (useCameraUp) {
				// For spherical gravity
				transform.LookAt(target.transform, target.transform.up);
			} else {
				// For normal gravity
				transform.LookAt(target.transform, Vector3.up);
			}
		}

		// Invert flip for matchRotationMode...
		bool actuallyFlip = matchRotationMode ? !flip : flip;
		if (actuallyFlip) { transform.Rotate(0, 180, 0); }
		// Apply zRotation
		zRotation += zRotationSpeed * Time.deltaTime;
		transform.Rotate(0, 0, zRotation);
	}

}
