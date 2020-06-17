using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {

	public List<Vector3> waypoints;
	void Reset() {
		
		waypoints = new List<Vector3>()  {
			new Vector3(-2.5f, 2.5f, 0),
			new Vector3(-2.5f, -2.5f, 0),
			new Vector3(2.5f, -2.5f, 0),
			new Vector3(2.5f, 2.5f, 0),
		};
	}
	void Awake() {
		
	}
	
	void Start() {
		
	}
	
	void Update() {
		
	}
	
}
