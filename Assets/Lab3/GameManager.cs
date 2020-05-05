using UnityEngine;
// C# `namespace`s are like java `package`s, 
// but don't actually require being in a folder of the same name.
namespace Lab3 {

	/// <summary> Implementation of basic stuff, and Achievement #1. </summary>
	public class GameManager : MonoBehaviour {
		/// <summary> Movement speed for other objects </summary>
		public float speed = 2.0f;
		/// <summary> Rotation speed for other objects </summary>
		public float rotate = 50.0f;
		/// <summary> Scaling speed for other objects </summary>
		public float scale = 5.0f;
		/// <summary> Color change speed for other objects </summary>
		public float color = 5.0f;
	}
}
