using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab4 {
	/// <summary> Technically correct orbital mechanics! </summary>
	public class OrbitTarget : MonoBehaviour {

		/// <summary> Target to orbit </summary>
		public Transform target;

		/// <summary> Speeds to apply to own rotation </summary>
		public Vector3 selfRotate = new Vector3(0, 10, 0);
		/// <summary> Property, gets the point to rotate. </summary>
		public Vector3 point { get { return target != null ? target.position : Vector3.zero; } }

		/// <summary> Relative time scale . </summary>
		public float orbitTimeScale = 100f;
		/// <summary> Used in orbital period/speed calculation. </summary>
		public float averageDistance;
		/// <summary> Rate of orbit. </summary>
		public float orbitSpeed;
	
		void Start() {
			// Consider the starting distance average (this is how my scene is set up!)
			averageDistance = (transform.position - point).magnitude;

			// Kepler's laws for those astronomers: Period^2 is proportional to Average Radius^3
			float orbitPeriod = Mathf.Sqrt(Mathf.Pow(averageDistance, 3));
			orbitSpeed = 1f/orbitPeriod;
		}

		void Update() {
			// Rotate around target point
			transform.RotateAround(point, Vector3.up, orbitSpeed * orbitTimeScale * Time.deltaTime);
			// Rotate self by self rotation
			transform.Rotate(selfRotate * Time.deltaTime);
		}
	
	}
}
