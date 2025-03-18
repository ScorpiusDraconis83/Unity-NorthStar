# Vertex Shader 

The ocean shader begins by sampling the displacement map in the vertex shader. The Uvs for this are calculated based on the world position of the vertices, divided by the total scale of the ocean.  

A Displacement Noise texture is then sampled with a scaling independent of the ocean scale. This noise is used to control the strength of the displacement. It's intended use is to vary the displacement effect across the surface of the ocean to reduce noticeable tiling/repetition. Careful choice of noise texture and scaling is required however, to ensure the displacement is not reduced too much across the entire ocean.  

The total length of the horizontal displacement, multiplied by the displacement noise strength is passed to the fragment shader, where it can be used for fake subsurface scattering. 

Additionally there is also a “Giant Wave” functionality. This is used in a few cutscenes to produce a single, very large wave that travels towards the boat. It uses a single gerstner wave, with a masking function to restrict the effect to a single wave, and a specified width. Due to time constraints, this was implemented in HLSL via a custom function node, instead of shadergraph. There is a fade-in which causes the wave to gradually rise out of the ocean, and then quickly fade out after hitting the boat. Several parameters are passed through from C# code, so that the wave is always moving towards the boat as it moves through the world. The output is an additional displacement, partial derivatives, both of which are combined with the existing ocean displacement/normals. 

![](./Images/OceanShader/Fig1.png) 

The giant wave has a few fudge factors in the function to make the wave line up and hit the ship and then fade, however getting it right for all situations was tricky, so some parameters are additionally controlled through Timeline curves for an additional level of control. Some of the equations could potentially be improved to reduce the need for manual tweaking. 

# Fragment Shader 

The tiling normal map is sampled using the same Uvs as the displacement map. This also contains foam and smoothness values in the blue/alpha channels, which will be discussed shortly. This normal value contains the X and Z components, as the Y component is reconstructed using a usual `NormalReconstructZ` function. (Since the ocean is a world-space grid, we don't need to perform a regular tangent to world transform, we can simply just swizzle the Y and Z channels).

A detail normal map is then sampled with a different scale/offset to the main normal map. Generally this should be smaller than the primary ocean normal map. The normal maps are combined using partial derivative blending, which is an accurate way to combine different normals produced by heightmaps. (In contrast to a technique such as Reoriented Normal Mapping (RNM) which rotates one normal map onto another) This normal map can be scaled, panned and rotated over time, so when panned in a similar direction to the ocean, with a slightly offset rotation and speed, it can add a convincing amount of extra detail to the lower-resolution ocean simulation. 

When the giant wave is enabled, the partial derivatives are combined with the ocean normal and detail normal partial derivatives to produce the final world space normal. The giant wave is re-evaluated in the pixel shader to obtain the normals, as passing them through per-vertex often did not provide enough accuracy to avoid visual artifacts. 

# Foam

![](./Images/OceanShader/Fig4.gif)    

A value for computing foam coverage is packed into the blue channel of the normal map. This is then converted into a foam mask with a configurable threshold using the formula `saturate(foamStrength * (threshold – normalMap.b))`. This allows foam amount to easily vary across ocean profiles. This is then multiplied with a detail foam texture. The final result is then used to lerp between the ocean’s blue albedo color, and a configurable foam color. (In practice, the color is often best left at white, however in some scenarios a slight tint can be used to provide certain effects). 

A shore foam functionality was also implemented, and involved sampling the depth buffer, and using the depth difference produce foam along the edges of objects that intersected the water. However for optimisation reasons, this is not currently used, as it also depends on the scene depth texture, and copying this can have a notable performance cost. 

The total foam amount is used to mask out underwater effects such as subsurface, as heavy layers of foam should block out strong amounts of light coming from underwater. 

# Subsurface Scattering 

![](./Images/OceanShader/Fig0.png) 

The subsurface scattering effect adds an extra amount of tinting/highlights inside of steep/tall waves. This is implemented by using the length of the horizontal displacement from the vertex shader, as tall waves appear where there is notable horizontal displacement, and where the subsurface scattering is most noticeable. The effect uses a two-lobe phase function, which allows for an configurable amount of forward and backward scattering. The result is multiplied by a configurable color. While the effect is not physically based, it works reasonably well in practice and can add some convincing extra detail to larger waves and help break up the uniform look of the ocean. 

# Refraction and Underwater Tint (Not used) 

An additional effect to sample the scene color behind the water surface with a fake refraction effect was also included, however due to performance costs from copying the scene texture, we decided not to use this. The effect is mostly only noticeable in shallow waters such as the docks scene, so for this project it was an acceptable compromise to maintain performance goals. The effect involves multiplying the world space normal XZ components by a small factor, and using that to offset a screenspace UV, which is then used to sample the scene color texture. 

The depth texture is also sampled at this location, and the scene color is tinted based on a simple exponential fog formula: `sceneColor *= exp(-depthDifference * extinction))`. Extinction is calculated using the “Color at Distance to Extinction” function, which calculates the extinction coefficient required to achieve a specific tint color at a specific distance, which can be more intuitive than working with an extinction coefficient directly.  

The water albedo also needs to be modified by the inverse of this amount, so that shallow areas of water do not contain more scattering/fog than they should, due to their shallow depth.  

# Underwater Surface 

![](./Images/OceanShader/Fig3.png)  

In the final part of the game where the player is underwater, a duplicate of the water shader is used with double-sided rendering enabled, and some extra logic for the backfaces of the water. A custom fog function is used to apply a tint to the underwater surface as it gets further away from the player. 

In addition to this, the environment reflection lookup is modified to use a refracted, instead of reflected direction. This produces the “snells window” effect that is observed when looking up at an underwater surface where parts of the sky are visible in a circle above the viewer. Using the existing environment cubemap means this does not require a copy of the scene texture, so the performance overhead is minimal, as it is simply replacing the sky reflection lookup that would otherwise be done in a regular PBR shader. The downside is that above water objects such as the ship are not visible, however a similar effect could be achieved with alpha blending if desired. 

# Parameter Overview 

- **Albedo:** The general color of the water, this functions the same as albedo in any regular PBR shader, except it may be modified by the foam amount if enabled. 
 
**Foam** 

- **Enable Shore Foam:** This enables the shore foam effect where objects intersecting the water plane will create a line of foam. However this requires the depth texture to be available. 
- **Foam Texture:** This is the texture used for foam. Only the red channel is used, to save bandwidth, but a full color texture with an alpha could be used if required, but minor shader changes would be needed. 
- **Foam Scale:** The world space tiling of the foam. Larger numbers will make the foam smaller and more detailed, but more repetitive. 
- **Foam Scroll:** This applies a world space panning effect to the foam to add extra visual interest. However in practice, the movement of the ocean often makes this unnoticeable, so may not be required for most situations. 
- **Foam Color:** Color tint that is applied to the foam texture. This is used as a lerp parameter instead of a multiply, to avoid ending up with blue-white foam that can’t ever reach fully-white. 
- **Foam Strength:** Multiplier for the foam texture strength. Higher values produce more foam but may remove some soft gradients/fading at edges which may occur. 
- **Foam Threshold:** Higher values will create foam in more areas, whereas lower values mean foam will only appear at the very peaks of high waves. 
- **Foam Aeration Color:** This was used to provide additional detail to the foam calculations and make use of additional texture channels, however the effect is currently disabled for performance reasons. 
- **Foam Aeration Threshold:** Similar to above, this would control the secondary foam texture details, however it is currently not enabled. 

**Scatter** 

- **Enable Subsurface:** Enables or disables the effect which produces extra light scattering from tall waves. In very gentle seas or certain lighting situations, the effect may not be noticeable or desired, so it can be disabled to slightly improve performance.
- **Scattering Color:** This is the color and intensity applied to the effect. The best values will depend on the environment and ocean itself, and is largely a matter of opinion. Often, a blue-green or even green value looks best, as red light is quickly absorbed as it travels through water, and blue light is mostly scattered as albedo, so green is generally the strongest remaining color for secondary lighting effects. 
- **Forward Scatter:** This controls the sharpness of the scattering along the light direction. A high value will give a small but bright, focused highlight, but produce no scattering across the rest of the water. Smaller values spread the effect out for a uniform look. Higher values generally work best here to provide a noticeable, dynamic effect. 
- **Back Scatter:** This produces extra scattering when looking away from the light source. Larger values will produce a more focused highlight as above. A smaller value such as 0.3 generally works better here, as back-scattering is usually more diffused.  
- **Scatter Blend:** This controls the contribution of the two above factors. A value of 0.5 provides an equal combination of both, and is generally the best balance. 
- **Scatter Displacement Power:** The strength of the scatter effect is affected by how much horizontal displacement the current vertex has been moved by, with larger displacements providing stronger scattering. A power function is used to vary the effect from a simple linear increase to a more sharp dramatic change for more enhanced visuals. The best value will depend on the sea state and ocean parameters. 
- **Scatter Displacement Factor:** This is a simple multiplier for the amount of scatter that is added compared to the ocean’s displacement. It is combined with the power function above to control the overall effect. 

**Normals** 

- **Normal Map:** This is the secondary normal map that is combined with the base ocean normals that are produced from the simulation. Any normal map can be used, however a normal map that is baked from the ocean simulation, using the “Bake Normal Map” context menu on the Ocean Simulation component is a good way to achieve high quality results.  
- **Normal Map Size:** This is the area that the normal map covers in world space. Should be several times smaller than the ocean profile’s patch size, to provide a  range of detail at different scales and reduce tiling. 
- **Normal Map Scroll:** Offset over time applied to the normal map, should be in a similar direction to the wind direction so that the ocean simulation and normal map move in similar directions. Avoid making the direction match exactly, as having slight interference between the ocean simulation and normal map creates more interesting interactions and results. 
- **Normal Map Strength:** Scales the strength of the detail normal map. 
- **Normal Rotation:** Rotates the normal map UVs. 

**Displacement Noise** 

- **Noise Scroll:** Scrolls the displacement noise texture, similar to normals and foam. In practice, a small amount of scrolling, or no scrolling is often fine. 
- **Noise Texture:** The noise texture to use. It should tile seamlessly, and contain a mix of light and dark areas, so that the ocean displacement will be reduced in some areas but not others. This will break up the repetition of the ocean simulation at a distance, without unnecessarily reducing the ocean displacement elsewhere. 
- **Noise Strength:** Controls how strongly the ocean displacement will get attenuated by the noise texture. A strength of 0 will completely disable the noise texture effect. 
- **Noise Scale:** Controls the size of the noise texture in world space. Should be set to a value larger than the ocean patch size, so that the effect of the noise texture does not noticably repeat in the same way as the ocean. The best value will depend on ocean patch size and the noise texture contents. 

**Smoothness** 

- **Smoothness Close:** The smoothness value to use for the ocean. Previously there was a “Smoothness Far” slider which would be used at a distance, to reduce harsh sun/environment reflections in the distance, however this data is now baked into the mip maps of the normal/foam/smoothness texture, as it is automatically generated/filtered based on the normal maps. 

**Shore** 

- **Enable Refraction:** This enables the underwater refraction effect, but requires the Camera Opaque Texture to be enabled which has a performance cost. 
- **Refraction Offset:** This controls how much the refraction effect is offset based on the normal map. 
- **Shore Foam Strength:** Controls intensity of the shore foam effect.
- **Shore Foam Threhsold:** Controls how quickly the shore foam fades in at the edge of objects intersecting the water. 
- **Shore Foam Fade:** Controls how quickly the shore foam fades out as the distance increases to the background object. 
- **Depth Threshold:** Controls the distance at which an underwater object will be fully tinted by the depth color. 
- **Depth Color:** The color which an underwater object will be tinted by at the “depth threshold” value. 

**Giant Wave** 

The following settings are generally set from C# code, but can be used to control or debug some aspects: 
- **Giant Wave Height:** The peak height of the giant wave.
- **Giant Wave Width:** The length along the wave at which the effect will be calculated. 
- **Giant Wave Length:** The wavelength of the giant wave, this controls how wide the wave looks and how fast it moves. 
- **Giant Wave Angle:** This controls the angle at which the wave will move towards the center location. 
- **Center:** This is the location that the wave will move towards. 
- **Falloff:** Controls how gradual the slope along the sides of the wave will be, which fades in the wave. 
- **Phase:** Mostly a debug option, this is the “progress” of the wave towards the target location. 
- **Giant Wave Steepness:** Controls the steepness of the gerstner wave which is used to calculate the giant wave. 
- **Giant Wave Curve Strength:** Controls how strongly the peak of the wave “curves” towards its target direction for a more dramatic effect. 
- **Giant Wave Enabled:** Controls if the giant wave is enabled or not, saving some calculations when disabled. 

### Relevant Files
- [Water Realistic.shadergraph](../Packages/com.meta.utilities.environment/Runtime/Shaders/Water/Water%20Realistic.shadergraph)
- [UnderwaterShader.shader](../Assets/NorthStar/Shaders/Water/UnderwaterShader.shader)
