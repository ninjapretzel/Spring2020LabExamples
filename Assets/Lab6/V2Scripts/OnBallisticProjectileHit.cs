using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {

	/// <summary> Class for attaching callbacks that can interrupt a <see cref="BallisticProjectile"/> during collision </summary>
	public class OnBallisticProjectileHit : MonoBehaviour {

		/// <summary> Callback. Should return false if the projectile should live, true if it should die. </summary>
		public Func<BallisticProjectile, bool> OnHit = (proj => false);

	}
}
