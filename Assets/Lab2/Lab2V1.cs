using UnityEngine;

namespace Lab2 {

	public class Lab2V1 : MonoBehaviour {
		
		Animator animator;
		SpriteRenderer sprite;

		void Awake() {
			animator = GetComponent<Animator>();	
			sprite = GetComponent<SpriteRenderer>();
		}

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
