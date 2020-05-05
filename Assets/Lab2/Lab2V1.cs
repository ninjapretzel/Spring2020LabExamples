using UnityEngine;
// C# `namespace`s are like java `package`s, 
// but don't actually require being in a folder of the same name.
namespace Lab2 {

	/// <summary> Basic implementation of Lab2 </summary>
	public class Lab2V1 : MonoBehaviour {
		
		/// <summary> Linked <see cref="Animator"/> component </summary>
		Animator animator;
		/// <summary> Linked <see cref="SpriteRenderer"/> component </summary>
		SpriteRenderer sprite;

		/// <summary> Called by Unity when the object the script is on is fully loaded,
		/// Before it has had 'Start' called. </summary>
		void Awake() {
			animator = GetComponent<Animator>();	
			sprite = GetComponent<SpriteRenderer>();
		}

		/// <summary> Called by unity every frame. </summary>
		void Update() {
			// Sample input
			float movement = Input.GetAxisRaw("Horizontal");

			// Update sprite 
			if (movement < 0) { sprite.flipX = true; }
			if (movement > 0) { sprite.flipX = false; }
			// Don't change if movement is exactly zero...

			// Use absolute value for parameter (always positive!)
			animator.SetFloat("Movement", Mathf.Abs(movement));

		}

	}
}
