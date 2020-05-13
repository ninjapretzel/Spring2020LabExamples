using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Technically correct orbital mechanics!
public class OrbitTarget : MonoBehaviour {

	public Transform target;
	public Vector3 selfRotate = new Vector3(0, 10, 0);
	public Vector3 point { get { return target != null ? target.position : Vector3.zero; } }

	public float orbitTimeScale = 100f;
	public float averageDistance;
	public float orbitSpeed;


	void Start() {
		// Consider the starting distance average (this is how my scene is set up!)
		averageDistance = (transform.position - point).magnitude;

		// Kepler's laws for those astronomers: Period^2 is proportional to Average Radius^3
		float orbitPeriod = Mathf.Sqrt(Mathf.Pow(averageDistance, 3));
		orbitSpeed = 1f/orbitPeriod;


	}
	void Update() {
		transform.RotateAround(point, Vector3.up, orbitSpeed * orbitTimeScale * Time.deltaTime);
		transform.Rotate(selfRotate * Time.deltaTime);
	}
	
}
