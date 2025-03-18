# Time of Day System 

![](./Images/TimeOfDay/Fig0.gif) 

The Time of Day System in NorthStar is controlled by an environment profile, which centralizes various settings to streamline scene management and real-time adjustments. The system supports real-time previews and transitions, even in scene mode, to support faster iteration and development. 

To achieve smooth transitions, the system primarily relies on lerping between floats, vectors, and colors. Where possible, values are precomputed to minimize GPU overhead. The system is designed to directly control environment parameters, avoiding computationally expensive techniques such as full atmospheric scattering and complex cloud lighting. 

## Environment System

![](./Images/TimeOfDay/Fig1.png)  

The Environment System serves as the core of the Time of Day System. It: 
- Manages references to scene objects. 
- Stores global and default values. 
- Handles transitions between different environment profiles. 
- Integrates rendering logic with the current render pipeline via RenderPipelineManager callbacks.

**Custom Rendering Logic**

This system also manages key rendering tasks, including: 
- Updating ocean simulations.
- Rendering the ocean quadtree.
- Rendering the sun and moon discs.
- Setting global shader properties. 

## Skybox Updater 

The Skybox Updater manages environmental lighting by dynamically adjusting: 
- Skybox reflection probes.
- Ambient lighting.
- Fog settings.
- Directional light angles and colors. 

These updates ensure accurate lighting and atmospheric transitions throughout different times of the day. 

## Sun Disk Renderer 

Rather than embedding additional logic within the skybox shader, the Sun Disk Renderer renders the sun and moon as simple quads with a custom shader. 

**Key Features:** 
- Basic color and texture functionality.
- Ability to simulate distant light source illumination (e.g., moon shading using a normal map).
- Cloud, fog, and atmospheric occlusion, ensuring the sun and moon integrate naturally with skybox colors. 

Since these materials inherit from the base skybox materials, they maintain proper cloud occlusion and sky color blending. 

# Environment Profiles 

The Time of Day System relies on scriptable objects to define environment profiles. Each profile contains nested classes grouping relevant settings. Below is an overview of the main configurable elements: 

![](./Images/TimeOfDay/Fig2.png)  

**Post Process Profile**

- Allows for custom post-processing settings per environment.
- Supports smooth transitions between profiles.
- Uses a default profile if none is assigned. 

**Ocean Settings** 

- Defines the current ocean state.
- Uses a specific ocean material with the Ocean Shader (refer to the Ocean System documentation for details). 

**Skybox Material**

- Defines the skybox shader used to render the sky.

## Sun and Celestial Object Settings

**Sun Settings** 

- Intensity – Controls directional light strength.
- Filter – Adjusts directional light color.
- Rotation – Controls sun orientation (X and Y components have a noticeable effect).
- Sun Disk Material – Must use a skybox shader with Is Celestial Object enabled.
- Angular Diameter – Controls the sun's apparent size (real-world value ~0.52, but can be adjusted for visuals). 

**Moon/Secondary Celestial Object** 

- Render Secondary Celestial Object – Enables rendering of a secondary disk (e.g., the moon).
- Secondary Object Rotation – Similar to the sun, but controls the secondary celestial body's orientation.
- Secondary Object Material – Configured similarly to the sun.
- Angular Diameter – Adjusts the moon’s apparent size (real-world value ~0.5 degrees, but can be tweaked for aesthetics).

## Fog and Wind Settings 

**Fog Settings** 

- Fog Color – Defines the full-density fog color.
- Density – Controls fog thickness via an exponential density function.
- Underwater Fog Color – Specific to underwater environments (used in Beat 7).
- Underwater Tint & Distance – Controls underwater color blending and fade distance. 

**Wind Settings** 

- Wind Yaw (Horizontal Angle) – Affects ocean movement and sail interactions.
- Wind Pitch (Vertical Angle) – Influences sail behavior and ocean wind speed calculations.

## Gradient Ambient & Precomputed Lighting 

**Gradient Ambient Settings** 

- Used alongside the Environment System to apply ambient lighting based on a gradient rather than the skybox. 

**Environment Data Optimization** 

- Stores precomputed ambient lighting from the skybox to reduce runtime calculations, optimizing performance. 

## Conclusion 

The Time of Day System in NorthStar provides a highly optimized and flexible tool for dynamic environment control in mobile VR. By focusing on direct parameter manipulation, precomputed values, and scriptable environment profiles, the system achieves realistic time-based transitions while minimizing GPU load. 

This structured approach ensures a high degree of customization, allowing for seamless environmental shifts and efficient real-time iteration during development. 

### Relevant Files
- [EnvironmentSystem.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/EnvironmentSystem.cs)
- [SkyboxUpdater.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/SkyboxUpdater.cs)
- [SunDiskRenderer.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/SunDiskRenderer.cs)
- [EnvironmentProfile.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Water/EnvironmentProfile.cs)
- [RainController.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/RainSystem/RainController.cs)
- [RainData.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/RainSystem/RainData.cs)
- [UnderwaterEnvironmentController.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/UnderwaterSystem/UnderwaterEnvironmentController.cs)
- [UnderwaterEnvironmentData.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/UnderwaterSystem/UnderwaterEnvironmentData.cs)
- [WindController.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/WindSystem/WindController.cs)
- [WindData.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/WindSystem/WindData.cs)
