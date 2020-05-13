This content is under the MIT License.
Unity 5.x shader by Jonathan Cohen (ninjapretzel)
Star Nest algorithm originally by Pablo Román Andrioli
	This package submitted as a collaboration between the two parties.

Original Shader:
https://www.shadertoy.com/view/XlfGRj

This is an Animated/Procedural 3d Skybox Effect, For Unity3D.
Submitted for Version 5.4.0f3
The included shader and materials should also work for Version 4.x.x

----------------------NOTE----------------------
This is *NOT* intended for use on mobile devices, and can have a *HUGE* performance hit on low-end computers.

I highly recommend that if you use this shader in your game, you include an option to disable it, or swap to a version with lower Iterations/Volumetric steps.

Here's a quick rundown of what stuff does on the material settings:

Main Color - Blend the final result with some color.
Clamp Output With Main Color- Enable to clamp the output color down, allowing the output of the shader to be darkened more evenly than the 'brightness' parameter.

Scrolling Direction - How the shader scrolls over time. Set 'w' to 0 to disable.
Center Position - 'Center' point in space/time. Can be used to animate scrolling with scripts when used with static cameras.
Camera Scroll - How much the skybox scrolls with the camera. Set to 0 to disable.
Rotation Axis - How the world rotates around the center point over time. Set 'w' to 0 to disable.

Iterations- Inner loop of volumetric rendering. Higher numbers mean more objects in the distance.
	Going higher than 20 makes the effect very bright.
Volumetric Steps - Outer loop of volumetric rendering. Higher numbers mean more objects in the distance.
	This value has a higher performance hit than Iterations.
	NOTE: Volumetric Steps must be >= 8 to view Dark Matter. Changing this can also affect how the dark matter looks.
									(at 7 it is rendered, but is really, really hard to see)

Formuparam - Magic number in the volumetric formula. Typical values are in the range (400, 600), Values between (0, 1200) are most interesting.
Step Size - Distance increase between each volumetric step. Typical values are in the range (200, 500)
Tile - How often the fractal repeats. Low numbers are busy and repetitive, high numbers are sparse. Typical values are in the range (300, 1200)
Brightness - General brightness control. Typical values are between (.1, 2), but can be adjusted as needed. Negative values flip 'matter' and 'dark matter' colors.
Dark Matter - Presence of Dark matter (in the distance). Typical values are in the range (0, 1000)
Distance Fading - Brightness of distant objects, including 'dark matter'
Saturation - Presence or lack of color. Negative values invert and white-wash colors.

	Note: All values listed above are suggestions.
	Play around and look at the example materials.

Additional properties added in 'PRO' version:
	Post-Hue Shift: Postprocessing shift to overall hue
	Post-Saturation: Postprocessing boost to saturation
	
	Field Vector- Vector used for an additional fractal effect. Best results come from (xyz) all in range (-1, 0)
	Field Power- Strength of field effect. Higher values may bring out more details and make the effect noisier.
	Field Reps- Maximum detail in the first layer of the field
	Field Reps2- Maximum detail in the second layer of the field
	Field Size- Adjustment to scaling of the first field
	Field Size2- Adjustment to scaling of the second field
	Field Brightness- Adjustment to brightness of first field 
	Field Brightness2- Adjustment to brightness of second field 
	Field Frequency- Pulse rate of first field
	Field Frequency2- Pulse rate of second field
	Field Hueshift- Change in color for first field
	Field Hueshift2- Change in color for second field
	
	Nebula Power- Amount of 'dark clouds' blocking light
	Nebula Darkness- Density of 'dark clouds'
	Seed- Seed value for clouds
	Noise Octaves- Number of fractal layers in clouds
	Noise Persistence- Amount of noise in clouds
	Noise Scale- Global scale to clouds
	
	
	
NOTE ABOUT LIGHTING BAKING:
	In the example scene, GI light baking is DISABLED, but these can have their light baked
	for reflections and other GI features.
	
	I recommend disabling the light baking in a scene when you set the skybox up, until you configure it the way you want.