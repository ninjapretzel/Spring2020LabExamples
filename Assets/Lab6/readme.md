# Lab6
### V1
##### (Version with an implementation of base requirements)
For this one, I didn't do achievables. There are 3 cannons, move ther mouse to aim, and left click anywhere to fire.
If you left click anything solid, it will create a wandering orb where you clicked.

### V2
##### (Version with an implementation of achievables)
I may have gone a bit overboard for this one. 

I brought in and modified some code from a game I worked on for player controls, viewmodel+animations, projectiles, and some other stuff.

Controls are:
```
WASD - Move
Space - Jump
Mouse - Look around
Left click - Shoot
Left Shift - sprint
C - Crouch

Left Control - release/capture mouse
```
There are crates to shoot and knock around.

The brown ones are lighter, the purple ones are heavier.

The amount of code is kinda high for you students, but if you want to have specific behavior (like my team did), then you need to have some code to make that stuff happen.

The weapon is set up as a set of prefabs:
- `PlasmaRifle`: Holds `ViewModel` and `ProjectileWeapon` scripts, and a few effects.
	- The "Fire is cool!" achievable is done by using a `UnityEvent` to tell a particle system to emit projectiles
	- `UnityEvent` is the same kind of thing you have used in the UI system, but you can use them in your own scripts too!
- `PlasmaShot`: Holds the `BallisticProjectile` script and a trail effect
	- `BallisticProjectile` simulates physics (in a fairly thorough way) for the "Obey Physics (revisited)" achievable.
- `PlasmaHitEffect`: Holds a sound and two particle effects. Is created through the 'decal' system of `BallisticProjectile`.

