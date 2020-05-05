# Lab1

### V1
##### (Version with an implementation of base requirements)  
For V1, I implemented it with some things that you probably haven't seen before.  

I track the target position to move to as a single non-public `Vector3`.   

Upon initialization, I set the target to the `end` position, and move the `transform` to the `start` position.  

Every frame, I subtract the current `transform.position` from the `target` to get the difference to the target.  
I use that difference to both direct the movement, and check if the object has reached its destination.  

To get the actual distance, I use the `Vector3.magnitude` property to get the length of the vector.  
I then calculate how far the object will step in a single frame.  

Depending on how those to values compare, I either:  
- switch the object to move to the other position (end vs start)  
	- Move if to the target position
	- then make the target the other position
- or move the object towards its target, using the difference to target as a direction  
	- The `Vector3.normalized` property returns a `Vector3` in the same direction, with a length of `1.0`.
	- Multiplying that by the `movementSpeed` variable allows it to move the correct distance.
	- Then multiplying that by `Time.deltaTime` to make it move consistantly over time.
	
Then finally, rotate the object by `rotateSpeed * Time.deltaTime`.

### V2
##### (Version with implementation for the achievements)

Instead of using `Input.GetAxis`, I use `Input.GetAxisRaw`- this does not have "smoothing" applied to it.  
For those of you who noticed your player-controlled objects continue to move after releasing buttons, that is why.  
Using `Input.GetAxisRaw`, the control of the object feels more snappy- the object quickly responds and stops when the player releases movement keys.

Logging is easy, just use `Debug.Log()` to write things to the console.  
There are also `Debug.LogWarning()` and `Debug.LogError()` which can be used to signal more severe things.