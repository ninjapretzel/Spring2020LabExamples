using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Rotate something over time, used in the V2 version </summary>
public class RotateOverTime : MonoBehaviour {

	/// <summary> Rate of rotation across x/y/z axes. </summary>
	public Vector3 rate = new Vector3(0, 10, 0);

	void Update() {
		// Apply rotation scaled by time 
		transform.Rotate(rate * Time.deltaTime);
	}
	
}
