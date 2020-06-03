using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Needed this as my framerate was varying wildly, and my viewmodel assumes a nearly-constant framerate due to the way springs work.
/// <summary> Sets the application's target framerate every frame. </summary>
public class FramerateTarget : MonoBehaviour {
	/// <summary> Target FPS</summary>
	public int target = 60;
	
	void Update() {

		Application.targetFrameRate = target;
	}
	
}
