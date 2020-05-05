using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab3 {
	// Implementation for achievement #2
	public class GameManagerV2 : MonoBehaviour {
		/// <summary> Movement speed for controlled objects </summary>
		public float speed = 5;
		/// <summary> Rotation speed for controlled objects </summary>
		public float rotate = 300;
		/// <summary> Scaling speed for controlled objects </summary>
		public float scaling = 3;
		/// <summary> Color speed for controlled objects </summary>
		public float color = 4;

		public Transform[] targets;
		/// <summary> Holds a sequence of offsets to apply to positions </summary>
		public Vector3[] positionOffsets;
		/// <summary> Holds a sequence of offsets to apply to scales </summary>
		public Vector3[] scaleOffsets;
		/// <summary> Array of colors to go through </summary>
		public Color[] colors;

		/// <summary> Cached starting positions </summary>
		Vector3[] startingPositions;
		/// <summary> Cached starting scales </summary>
		Vector3[] startingScales;

		void Start() {
			// Upon start, initialize `startingPositions` and `startingScales`
			startingPositions = new Vector3[targets.Length];
			startingScales= new Vector3[targets.Length];
			// and fill with the initial position of all tracked objects
			for (int i = 0; i < targets.Length; i++) {
				startingPositions[i] = targets[i].position;
				startingScales[i] = targets[i].localScale;
			}
		}

		void Update() {
			// In this implementation, all objects will get the same color:
			float colorPos = Time.time * color;
			// Turn that position into two indexes and a percentage between them
			int colorA = ((int)(colorPos)) % colors.Length;
			int colorB = (colorA+1) % colors.Length;
			float percentage = colorPos % 1.0f;
			// Then lerp between those colors by `percentage`.
			Color colorResult = Color.Lerp(colors[colorA], colors[colorB], percentage);
			
			// Every frame, update all target objects
			for (int i = 0; i < targets.Length; i++) {
				Transform target = targets[i];
				// Derive position/scale endpoints for this object from cached starting info
				Vector3 startPos = startingPositions[i];
				Vector3 endPos = startPos + positionOffsets[i % positionOffsets.Length];
				Vector3 startScale = startingScales[i];
				Vector3 endScale = Vector3.Scale(startScale, scaleOffsets[i % scaleOffsets.Length]);

				target.position = Vector3.Lerp(startPos, endPos, Mathf.PingPong(Time.time * speed / (endPos - startPos).magnitude, 1.0f));
				target.rotation = Quaternion.Euler(0, 0, Time.time * rotate);
				target.localScale = Vector3.Lerp(startScale, endScale, Mathf.PingPong(Time.time * scaling, 1.0f));

				Renderer rend = target.GetComponent<Renderer>();
				if (rend != null) {
					// Apply the color if the target has a renderer.
					rend.material.color = colorResult;
				}

			}
		}
	
	}
}
