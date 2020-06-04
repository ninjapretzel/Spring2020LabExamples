# Lab6
### V1
##### (Version with an implementation of base requirements)
For this one, I didn't do achievables. There are 3 cannons, move the mouse to aim, and left click anywhere to fire one of them. The other two will autofire.
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

The amount of code is kinda high for you students, but if you want to have specific behavior (like my team did for that game), you need to write enough code to get that behaviour.

The projectiles you shoot bounce around, and create an effect anywhere they touch.

I the textures for the particle effects and other VFX (such as the plasma-bolt at the front of the rifle) I made myself, using a free program called `Paint.Net`. The sources (`.pdn` files) for the ones I made just for this example are included, while some of the others I brought in from previous projects.

The weapon is set up as a set of prefabs:
- `PlasmaRifle`: Holds `ViewModel` and `ProjectileWeapon` scripts, and a few effects.
	- The "Fire is cool!" achievable is done by using a `UnityEvent` to tell a particle system to emit projectiles
	- `UnityEvent` is the same kind of thing you have used in the UI system, but you can use them in your own scripts too!
- `PlasmaShot`: Holds the `BallisticProjectile` script and a trail effect
	- `BallisticProjectile` simulates physics (in a fairly thorough way) for the "Obey Physics (revisited)" achievable.
- `PlasmaHitEffect`: Holds a sound and two particle effects. Is created through the 'decal' system of `BallisticProjectile`.

