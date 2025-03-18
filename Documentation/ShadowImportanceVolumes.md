## Shadow Importance Volumes 

Unity will ensure the full viewport, up to the configured maximum shadow distance, has coverage in the shadowmap. For many games and on other platforms, this is sufficient to balance texel density with full scene coverage, however as this game takes place on the surface of a ship, and with the Meta Quests impressive FOV, it is wasteful to assign shadow coverage for areas of the viewport such as the sky and ocean. Shadow Importance Volumes address this by allowing a designer to specify which surfaces require shadowmap coverage, and allowing all other surfaces to lie outside of the shadow volume.

![](./Images/ShadowImportance/Fig1.png)

ShadowImportanceVolumes require changes to URP in provide a mechanism to adjust the shadow projection matrix and shadow distance before shadow casting.

### Relevant Files
- [ShadowImportanceVolume.cs](../Assets/NorthStar/Scripts/Utils/ShadowImportanceVolume.cs)
- [ShadowUtils.cs](../Packages/com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs)
- [MainLightShadowCasterPass.cs](../Packages/com.unity.render-pipelines.universal/Runtime/Passes/MainLightShadowCasterPass.cs)

## Using Shadow Importance Volumes

Shadow Importance Volumes will automatically be enabled when any enabled volume intersects the camera. To create a new volume, create an empty Game Object, and then select Add Component and choose ShadowImportanceVolume. You can then reposition and scale the volume into position. Any surface that can receive shadows should be inside a volume. The entire object/mesh is not required to be inside the volume, only the surface.

![](./Images/ShadowImportance/Fig2.png)

If all volumes are disabled or do not intersect the camera, shadows will revert to their default Unity implementation.

## How they work

All volumes in the game world are first intersected with the active cameras frustum. Any volume or part of a volume that lies outside of the camera frustum is ignored. The intersection of the volumes and frustum is then used as a hull for shadowcasting - the hull is transformed into shadow space, and an adjustment matrix is generated to fit the shadowmap more tightly around the active areas.

Unity camera frustum with a far distance of 20:

![](./Images/ShadowImportance/Fig3.png)

Comparison of shadowmap between default and with volumes on the ship deck. The very wide FOV forces Unity to size the shadowmap inefficiently:

![](./Images/ShadowImportance/Fig4.png)
