using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Behavior to destroy something after a delay. </summary>
public class DestroyAfterDelay : MonoBehaviour {
	/// <summary> Time before destruction in seconds </summary>
	public float time = .5f;
	
	void Update() {
		time -= Time.deltaTime;
		if (time < 0) {
			Destroy(gameObject);
		}
	}
	
}
