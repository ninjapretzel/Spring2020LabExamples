# Lab4
### V1
##### (Version with an implementation of base requirements)
For this one, I didn't do the achievables, but I did throw some realistic textures (sourced from https://www.solarsystemscope.com/textures/) on the planets, and modeled it after the real solarsystem, and threw one of my camera controllers into the scene.

For Phobos/Deimos, I generated the textures using a free program called "Genetica Viewer", which is easy to use.  
(http://spiralgraphics.biz/viewer/download_viewer.htm)  
I rendered various default sources like "Craterscape" and "Deep Caves", took effects maps, and combined them in Paint.NET to produce the `"rocky_x.png"` textures in the project.

The background is a version of a shader I put up on the Unity Asset Store:  
(https://assetstore.unity.com/packages/vfx/shaders/star-nest-skybox-63726)

In Lab4 scenes:
- Camera will pan with the mouse.
- Right-click and drag to rotate camera.

### V2
#### (Version using just `Transform.Rotate()`)
It is possible to rotate objects in a scene just by rotating the parents, so my v2 script rotates the parent objects, and all of the children objects automatically rotate around their parents

### Extra
##### (More realistic version)
For this one, I again didn't do the achievables, but I did make it so that the planets realistically orbit their target, based on Kepler's Laws of Planetary Motion (Period^2 ~= Radius^3)

