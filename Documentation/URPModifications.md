## URP Modifications

The Universal Render Pipeline served as a great starting point for the project, it offers a lot of features and flexibility to kick start development. To better serve the needs of the project, some modifications were made to URP shaders and code.

**Shader Modifications**

The Unity BRDF has been replaced by a more accurate approximation, improving the lighting response on the ocean and other shiny surfaces. This comes at a slight increase in cost for all surfaces in the game. To counteract this, a specialized non-metallic BRDF was also included for all non-metallic surfaces. This can be configured in the Shader Graph Settings by toggling “Non-Metallic Surface”.

Unity shadowing is quite flexible, selecting at runtime between supported quality settings. While small, the cost of reading these cbuffer values, reducing shader size, and potentially reducing register usage presents an easy optimization opportunity. Where appropriate, these values were hard-coded to the ones required by the project. Surfaces facing away from the light source, or otherwise with light fully attenuated, are unaffected by shadow sampling. Shadow sampling was moved into a branch so that surfaces facing away from the light source do not sample from the shadow map. 

The ship relies on Reflection Probes to simulate reflections on the deck surface, particularly where wet. These probes, and the box projection, must rotate with the ship for this to be convincing. We achieve this by passing a _ProbeReorientation matrix into GlobalIllumination.hlsl, and applying this when calculating the cubemap sample point. 

### Relevant Files
- [BRDF.hlsl](../Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl)
- [UniversalLitSubTarget.cs](../Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Targets/UniversalLitSubTarget.cs)
- [Shadows.hlsl](../Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl)
- [Lighting.hlsl](../Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl)
- [GlobalIllumination.hlsl](../Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl)

**Code Modifications**

By default, the XR display will be configured to match the MSAA value configured in the URP asset, however any post processing will force an MSAA resolve making it unnecessary to blit to an MSAA display target. 

URP versions before 14.0.9 have a bug causing excessive texture reallocation with Dynamic Resolution, leading to poor framerate and eventually an out-of-memory crash. This can be fixed by following the guide [here](https://developers.meta.com/horizon/documentation/unity/dynamic-resolution-unity/). We found it necessary to also enable DynamicResolution support in the RTHandles system immediately after initializing it in UniversalRenderPipeline.cs. 

We use ShadowImportanceVolumes to dynamically adjust the shadow map projection, which requires an entrypoint in ShadowUtils. This allows modifying the shadow projection matrix and distance values. MainLightShadowCasterPass.cs was also modified to pass through the camera data to facilitate computing accurate shadow importance volumes.

### Relevant Files
- [UniversalRenderPipeline.cs](../Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipeline.cs)
- [UniversalRenderer.cs](../Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderer.cs)
- [ShadowUtils.cs](../Packages/com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs)
- [MainLightShadowCasterPass.cs](../Packages/com.unity.render-pipelines.universal/Runtime/Passes/MainLightShadowCasterPass.cs)
