using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {
	public class DiegeticAmmoDisplay : MonoBehaviour {
		[Serializable] public class TargetInfo {
			public Renderer rendererOverride;
			public int materialIndex;
			public Material mat;
		}

		public AWeapon attachedWeapon;
		public Renderer target;
		
		public Texture2D[] digitTextures;
		public TargetInfo[] digitTargets;
		
		void Update() {
			if (attachedWeapon == null || digitTextures.Length != 10) {
				return;
			}

			int ammo = (int)attachedWeapon.ammoInMag;
			for (int i = 0; i < digitTargets.Length; i++) {
				var info = digitTargets[i];

				Renderer tgt = info.rendererOverride != null ? info.rendererOverride : target;
				if (tgt == null) { continue; }

				// Create a new material to avoid clobbering...
				if (info.mat == null) {
					info.mat = new Material(tgt.sharedMaterials[info.materialIndex]); 
					tgt.materials[info.materialIndex] = info.mat;
				}

				int digit = ammo % 10;
				tgt.materials[info.materialIndex].mainTexture = digitTextures[digit];
				ammo /= 10;
			}
		}
	
	}
}
