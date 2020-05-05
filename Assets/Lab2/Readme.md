# Lab2

### V1
(Version with an implementation of base requirements)  

This is a straightforward implementation.  
I used the "Automatic" setting to slice the sprite (as opposed to either of the "Grid by X" options.  
The animations were made in a straightforward manner.  
The script samples the Horizontal axis, flips the sprite if needed, and passes the absolute value of the Horizontal axis to the animator state machine.

### V2
(Version with an implementation of the acheivements)  
I personally have a high amount of distaste for Unity's animation system, and the 2d tooling in general, so I have an example implementation which uses some libraries I have developed for 2d projects I had worked on.

(Some students did submit things close to this level- just not with the custom things or pixel-perfect 2d rendering)

This is a bit of stuff that may be a bit more advanced than some of you have seen so far, but shows some of the kinds of fundamental things that you would skip over simply by using `Rigidbody` or a built-in `Animator` type.

The included libraries are outside of the Lab2 folder, as I plan to use them in other examples as well.

The `PixelPerfectBehavior` is a type which adjusts the positions of objects based on pixel-size so that there are never any mis-aligned pixels. This helps prevent a common mistake people who don't yet have careful eyes will make when trying to duplicate retro-styled games (eg NES/SNES/Genesis, etc)

`FollowCam`, and `FollowCamSettings` make up a simple camera system (more on that in later labs) to follow the player.  
It also adjusts the screen size to be an exact integer pixel zoom (eg, 1x, 2x, 3x, etc.) based off of the set pixel size.  
For this example, I just use 1x zoom with the default 1/100 units per pixel.  
The `FollowCamSettings` is used to change the way the camera moves when it follows a certain object. If one is found on its target object, the `FollowCam` adjusts its behavior accordingly.

`SpriteAnimator` is a fairly simple sprite animation system which simply flips frames of the currently active animation, and `SpriteAnimAsset` is an asset type, which allows for a new type of asset to be created to hold animation data for the prior system.

If you inspect the 4 `SpriteAnimAssets` in this folder,  you will notice they each have a different "Animation Speed" parameter. This makes it really easy to change the speed of the animation if to fit what you want. The frames of the animations are just `Sprite[]`s, and unity's array editor is actually fairly competent:
- you can hit `delete` key to empty the selected index
- `shift+delete` to remove that index and shift everything over
- you can duplicate an index and shift everything down by using `ctrl+d`
	- (if you haven't already learned about it, `ctrl+d` is a a universal "duplicate selection" hotkey in unity! Very useful!
- you can multi-select sprites in the order you want to insert them, and drag them into the array  
I find such controls more intuitive for editing sprite animations.

You can also edit the animations in play mode- which you cannot do with Unity's default system (as far as I know at least!)

There are more details in the `Lab2V2` script.
