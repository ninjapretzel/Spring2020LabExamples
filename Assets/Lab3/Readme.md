# Lab3

### V1
##### (Version with an implementation of base requirements + Achievement #1)  

This is a straightforward implementation.  
I show different ways of animating features between two known endpoints using `Time.time`  
I also added a parameter to the `GameManager` type for color changing speed.

It is possible to animate things between endpoints using `Time.deltaTime`, and stepping towards a target value, but this requires that you check if you have reached/passed the destination point.  
I have an example of that, used for scaling in `Lab3V1`.

### V2
##### (Version with an implementation of all of the acheivements)  

The v2 for this lab is the `GameManagerV2` type- this type has `public Transform[] targets;` which is used to assign the objects it will control in the inspector. It also has some `Vector3[]`s which are used to specify position/scale changes. 

It caches the initial position/scale of all of its targets on startup, and animates the position/scale of each object by deriving the endpoints of its animations every frame. It animates the rotation simply by rotating at a constant speed, and animates color by stepping through a `Color[]` over time, smoothly transitioning between colors as time passes.
