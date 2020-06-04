using UnityEngine;

/// <summary> Selects a random texture for the surface </summary>
// Used to select sparks and muzzle flashes for bullet weapons
public class RandomTexture : MonoBehaviour {
	/// <summary> Textures to pick from </summary>
	public Texture2D[] textures;
	/// <summary> Target renderer to affect </summary>
	public Renderer target;
	void Awake() {
		// Make a copy to not clobber materials 
		Material mat = new Material(target.sharedMaterial);
		target.sharedMaterial = mat;
		mat.mainTexture = textures[Random.Range(0, textures.Length)];
			
	}
	
}
