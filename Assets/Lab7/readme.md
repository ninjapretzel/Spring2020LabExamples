# Lab6
### V1
##### (Version with an implementation of base requirements)
For this one, I didn't do achievables. There are is a small scene set up with cubes and spheres. 

There are green dynamic rigidbodies, some red kinematic rigidbodies, and a blue trigger, which log events.

There are also brown platforms.

### V2
##### Pinball machine
I made a small pinball machine using physics. 2d and 3d both work in very similar ways.

The paddles are set up as kinematic rigidbodies, as their positions/rotations are controlled directly with scripts.

The ball is a dynamic object, with a script that applies a custom gravity force to it every physics frame, and resets its position/velocities when requested. The ball's mass is reduced to `.1` to make it easier to affect with forces. It is given a texture from a previous lab to make it easier to see it rolling.

There are bumpers which expand when the ball collides with them, however, this force is applied with a trigger, not the solid bit inside.

There are still some oddities (such as the ball getting stuck inside or pushed weirdly by the paddles), but it is fairly solid and feels pretty close to right.

###### Additional info:
I also changed some physics settings under `Edit->Project Settings->Physics`

`"Default Max Angular Speed"` near the bottom was like 6, I upped it to 99999. This lets objects spin as fast as they realistically would, but can cause problems.

I also placed an invisible collider over the pinball machine, which prevents the ball from escaping.

I am using a the `UnityEvent` type (you've already indirectly used it with the UI labs) to handle keybinds. All keybinds are set up on the `"PinballBoard"` GameObject.