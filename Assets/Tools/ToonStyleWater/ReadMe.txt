Toon-Styled Foam Shader.

It is a bit tricky. In order to be performance friendly it does not use Render Texture.
Instead it utilizes screen buffer's alpha chanel. CPU can't read from it, but it can set BlendMode to multiply by it.
So this method includes shader for water and for terrain or any object that is expected to go in the water.
Those objects draw foam on themselves, and drow 1 to screen alpha chanel if that area should not contain water. 

1. If you use water by itself, make sure your Clear Color sets alpha to 0, or water will be invisible. 
2. All underwater objects should have WetGeometry shader.
3. I recommend setting Environment lighting to Gradient. 

In addition to shaders, there is a script that has to be applied to water plane. 
It will set global Shader parameters for other objects to read from. 

