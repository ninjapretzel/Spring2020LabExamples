using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Rotate all children over time, used in the V1 version </summary>
public class RotateChildrenOverTime : MonoBehaviour {

	/// <summary> Rate of rotation across x/y/z axes </summary>
	public Vector3 rate = new Vector3(0, 10, 0);
	
	void Update() {
		// Get number of children
		int children = transform.childCount;
		// Loop that many times
		for (int i = 0; i < children; i++) {
			Transform child = transform.GetChild(i);
			// Rotate each child about each axis.
			child.RotateAround(transform.position, Vector3.forward, rate.z * Time.deltaTime);
			child.RotateAround(transform.position, Vector3.up, rate.y * Time.deltaTime);
			child.RotateAround(transform.position, Vector3.right, rate.x * Time.deltaTime);
		}
	}
	
}
