using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {
	public class QuickAndDirtyWeaponSwitch : MonoBehaviour {

		public AWeapon[] weapons;
		PlayerControl player;
	
		void Update() {
			if (player == null) { player = GetComponent<PlayerControl>(); }
			if (Input.GetKeyDown(KeyCode.Alpha1)) { TrySwitch(0); }
			if (Input.GetKeyDown(KeyCode.Alpha2)) { TrySwitch(1); }
			if (Input.GetKeyDown(KeyCode.Alpha3)) { TrySwitch(2); }
			if (Input.GetKeyDown(KeyCode.Alpha4)) { TrySwitch(3); }
			if (Input.GetKeyDown(KeyCode.Alpha5)) { TrySwitch(4); }
			if (Input.GetKeyDown(KeyCode.Alpha6)) { TrySwitch(5); }
			if (Input.GetKeyDown(KeyCode.Alpha7)) { TrySwitch(6); }
			if (Input.GetKeyDown(KeyCode.Alpha8)) { TrySwitch(7); }
			if (Input.GetKeyDown(KeyCode.Alpha9)) { TrySwitch(8); }
			if (Input.GetKeyDown(KeyCode.Alpha0)) { TrySwitch(9); }
		}

		public void TrySwitch(int index) {
			if (index < weapons.Length) {
				player.GiveWeapon(weapons[index], true);
			}
		}
	
	}
}
