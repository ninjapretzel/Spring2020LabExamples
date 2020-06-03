using UnityEngine;
namespace Lab6 { 
	/// <summary> Little behaviour to make something move around </summary>
	public class Wander : MonoBehaviour {
		/// <summary> Minimum lifetime in seconds </summary>
		public float minLifetime = 10f;
		/// <summary> Maximum lifetime in seconds </summary>
		public float maxLifetime = 15f;
		/// <summary> Minimum speed in units per second </summary>
		public float minSpeed = 6f; 
		/// <summary> Maximum speed in units per second </summary>
		public float maxSpeed = 10f; 
		
		/// <summary> Chosen lifetime </summary>
		float lifetime;
		/// <summary> Chosen speed </summary>
		float speed;
		/// <summary> Chosen direction </summary>
		Vector3 direction;

		void Start() {
			// Pick lifetime/speed
			lifetime = Random.Range(minLifetime, maxLifetime);
			speed = Random.Range(minSpeed, maxSpeed);
			// Force movement to be along x/z only (flat)
			direction = Random.onUnitSphere;
			direction.y = 0;
		}

		void Update() {
			// Move- note that this will push the wanderball through walls!
			// If we want to detect collisions and avoid walls, we need to use something
			// like CharacterController or Rigidbody!
			transform.position += direction * speed * Time.deltaTime;	
			// Subtract lifetime
			lifetime -= Time.deltaTime;
			// Destroy self if out of life
			if (lifetime < 0) {
				Destroy(gameObject);
			}
		}
	
	}
}
