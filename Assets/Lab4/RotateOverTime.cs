using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RotateOverTime : MonoBehaviour {

	public Vector3 rate = new Vector3(0, 10, 0);

	void Update() {
		transform.Rotate(rate * Time.deltaTime);
	}
	
}
