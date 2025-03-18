# Weather Effects 

## Rain 

#### Rain Ripples 

As part of the rain system in NorthStar, rain ripples were added. The ripples are a procedural shader driven effect that is part of the NorthStarDefaultShader shader function. 

To drive the rain ripples on compatible surfaces in the scene, use the Rain Data Scriptable object to configure the ripple settings and add the Rain Controller Component into the scene to enable/disable the rain ripples. 

![](./Images/WeatherEffects/Fig17.png)

#### Optimization

To reduce texture samples a procedural function was employed. The pros of using a procedural approach rather than a texture flipbook approach is the level of control and reusability. The procedural ripples can be setup to have a variable density and intensity that would be hard to achieve with a flip book. This also allows us to optimize the effect as much as we need to by removing ripple layers. For the Quest 2 builds the amount of rain ripples is halved. 
 
### RainDrops 

Raindrops are currently a standalone Particle System that's configured and placed as necessary. Implementation into the Rain Controller is a work in progress. 

#### Optimization and challenges

In VR clipping with the player camera generally doesn't look good, especially with high-frequency effects like rain. The difficulty with raindrops in NorthStar is the perception of small geometry in the distance making it hard to read the raindrops, particles become too small where they read as nothing or noise and contribute largely to fragment overdraw. 

Instead of fading out the particles as they get near the player’s camera and dealing with clipping geometry, we implemented a dynamic solution that scales the particle size by the distance from the player and when the particles are at a size of 0 they get moved behind the player, reducing fragment overdraw, increasing performance and allowing better particle readability based on distance. 

### Wind 

Wind has been implemented as a procedural shader feature in the NorthStar’s foliage shaders. Wind will only work when a Wind controller component is present in the scene and models have the correct vertex color setup. 

#### Optimization 

The wind system is a combination of multiple sine waves added together to simulate complex foliage movements. 

## Geometry vertex colors setup: 

There are two types of foliage supported: 

### Single layer for simple foliage: 

Single layer foliage is geometry that only has a single channel of motion to it. This is mostly reserved for small foliage clusters like grass, flowers or anything that doesn't need a trunk to it. 

**Vertex Color setup:**

R: Wind Influence. 0-1 How much the wind influences the vertices. 

G: None 

B: Random ID. Random offset. Can be used per leaf cluster 

A: None 

Example: 

![](./Images/WeatherEffects/Fig5.png)
 

### Double Layer for more complex foliage: 

This setup is mostly used for foliage with a primary and secondary set of motion. For instance, in NorthStar the kelp in Beat 7 has a primary set of motion for the stem and then the leaves inherit the first set and add on the second set of motion to create a more fluid complex motion. 

**Vertex Color Setup:** 

R: Stem: Black 			Leaves: Wind Influence 

G: Stem: Wind Influence		Leaves: Stem Wind Influence 

B: Stem: None 			Leaves: Random ID 

A: Stem: None 			Leave: None 

Trunk: 

![](./Images/WeatherEffects/Fig4.png)

The parameters for the wind itself is stored in the Wind Data scriptable object which can be previewed in the Wind controller when the Wind Data field is populated. 

![](./Images/WeatherEffects/Fig17.png) 

## Asset optimization 

One of our most graphically intense scenes was the Docks scene in Beat2. This scene required the boat and a whole island to be in the same frame. With set dressing in the later stages of finalization, some light profiling revealed our vertex counts were fairly high. 

![](./Images/WeatherEffects/Fig13.png) ![](./Images/WeatherEffects/Fig1.png)

 

A few techniques that were used to optimize geometry were approaches like LODs, imposters, and billboards, but these won’t be covered since they are standard approaches to optimization.  

In NorthStar, where applicable, teleportation points were used to define a rough vertex density based on the distance from the teleportation point. Since NorthStar’s navigations are based on stationary points and not free movement we can be quite brutal in reducing geometry. 

No real metric was followed for reductions as they were generally made until silhouettes started to show hard edges. 

 

Density decreases over distance. 

![](./Images/WeatherEffects/Fig2.png)

The red circles indicate places where the player stands. 

To help reduce drawcalls and the scene vertex count the boat was also broken up into 4 parts based on where the player can navigate. 

![](./Images/WeatherEffects/Fig16.png)

Each of these segments was baked down into a single mesh component with 2 materials. One material for the metal components and one material for the non-metal parts. With the LOD system extended to support manual LOD switching any part of the 4 deck parts could be switching to a cheaper version of the boat . 

![](./Images/WeatherEffects/Fig8.png)

Any geometry that didn’t need optimization was out of these custom LOD meshes. 

## Shader System 

The majority of NorthStar’s shaders were made with Shader Graph. These shaders were optimized for the Quest platform and below are the considerations that were made. 

### Texture Configuration 

To reduce the number of texture samples used in NorthStar’s PBR shaders, a custom texture packing setup was used, this is how the channels are broken down: 

BaseColor Texture: 

RGB: 	Base Color 

A:	Opacity/Emissive. Emissive is multiplied with Base Color 

Normal Texture: 

RG: 	Normal  

G: 	Smoothness 

B: 	Metallic/Ambient Occlusion/Height 

The structure is to pack the most useful channels first and leave any channels that are conditional last. This works well with ASTC since compression is linear across all channels so we don’t suffer from leaving or using any extra texture channels. 

### Shaders 

Early on in development, an uber shader approach was used to reduce the amount of shaders required to maintain and allow materials to be easily setup, any feature can be enabled and disabled as needed. When a Parameter is connected to the node graph the shader compiler sees that as a dynamic “constant” forcing the shader to compute that  entire node chain, this prevents Unity’s shader compiler from collapsing shader instructions down reducing shader complexity. 

A good example of this is between stock options for the PBR_N and PBR_NSM shaders. 

![](./Images/WeatherEffects/Fig7.png)
![](./Images/WeatherEffects/Fig0.png)
![](./Images/WeatherEffects/Fig12.png) 

(Left no parameters, right with parameters) 

Just by disconnecting the SmoothnessValue and Metallicvalue parameters from the shader graph we get a 15%~ instruction count savings, not all instructions cost the same so it’s not a 1-1 performance increase, but every little bit helps. 

Using this as our basis we created a collection of shaders covering all our common use cases. 

![](./Images/WeatherEffects/Fig10.png)

Shader functions were used extensively to maintain consistency between these shaders. One in particular was the Uber shader function that we’d manually enable and disable features with constants rather than parameters to take advantage of this optimization process. 

![](./Images/WeatherEffects/Fig11.png)

### Material ShaderGUIs 

To help artists choose the right shaders a custom shader inspector was created. By assigning any of the NorthStar PBR shaders the material interface will use the custom inspector. 

![](./Images/WeatherEffects/Fig9.png)

In the inspector, the Selected Shader dropdown gives the artist some informative choices while also limiting what information is displayed. By using this dropdown it gives a central location for all the shaders. 

![](./Images/WeatherEffects/Fig14.png)

By selecting a different shader in the above dropdown the inspector updates to display the relative options. 

PBR with smoothness: 

![](./Images/WeatherEffects/Fig15.png)

PBR Foliage: 

![](./Images/WeatherEffects/Fig3.png)
