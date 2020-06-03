using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {
	public class OnBallisticProjectileHit : MonoBehaviour {

		public Func<BallisticProjectile, bool> OnHit = (proj => false);

	}
}
