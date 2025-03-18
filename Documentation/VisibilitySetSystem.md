# Visibility Set System 

## Overview

![](./Images/VisibilitySet/Fig0.png)

During development, we found that only a small portion of the boat and surrounding environment was fully visible at any given time. Additionally, various elements—such as the boat’s interior and the island in Beat 2—were often only partially visible or required lower levels of detail. 

While LOD (Level of Detail) groups are typically used for this purpose, LOD transitions were highly noticeable in VR due to frequent head movement during gameplay. Since we could not set precise LOD transition points to eliminate this effect, we developed a custom solution to dynamically manage object visibility and LOD activation and improve performance across the entire experience. 

This became known as the Visibility Set System—a toolset that allows designers to define and manage named object groups within a scene, enabling and disabling them as needed. 

## How the Visibility Set System Works 

1. Named Visibility Sets 

    - Designers can define multiple "sets" in a scene, each grouping related objects. 
    - These sets can be enabled or disabled dynamically, optimizing rendering and performance. 

2. Multi-Level Detail Management 

    - Each set supports multiple LOD levels, including a fully disabled state. 
    - Sets can be linked, allowing objects to dynamically adjust visibility based on narrative events or scene interactions. 

3. Example: Boat Cabin System 

    - The boat was split into multiple visibility sets, including a dedicated set for the cabin interior. 
    - The cabin door had two visibility states: 
        - Closed: The system activates the cabin_interior_door_closed set, applying a high LOD factor (9999) to disable unnecessary objects. 
        - Open: A narrative trigger switches to the cabin_interior_door_open set, revealing parts of the island visible through the door. 

4. Seamless Transitions 

    - Visibility transitions occur during teleports, helping mask any performance spikes caused by activating objects. 
    - This approach was critical in Beat 2, where rendering the cabin interior, boat exterior, port, and island simultaneously was too expensive. 

5. Performance Optimizations 

    - Entire sections of the level—including physics, logic, and scripts—are disabled when not needed, improving both CPU and GPU performance. 
    - On scene load, all objects are enabled for the first frame to ensure that Awake() / Start() methods execute properly. This prevents lag spikes when enabling large scene sections later. 

### Relevant Files
- [ActiveVisibilitySetLevelData.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/ActiveVisibilitySetLevelData.cs)
- [VisibilitySet.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/VisibilitySet.cs)
- [VisibilitySetData.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/VisibilitySetData.cs)

## Editor Support 

While the Visibility Set System was highly effective, managing multiple overlapping sets could be difficult to visualize. 

To assist designers and artists: 
- We implemented a context menu tool to automatically convert LOD groups into Visibility Set items. 
- The tool could rearrange object hierarchies if the original LOD group was misconfigured. 

This allowed artists to continue using familiar workflows while benefiting from the system’s optimizations.

## Further Improvements 

There are opportunities to further improve the Visibility Set System, particularly in editor usability: 
- Custom inspectors for better visualization of active/culled objects. 
- In-editor previews to show which objects are disabled under specific set conditions. 

These enhancements would make the system more intuitive and reduce iteration time when working with complex scenes. 

## Conclusion
The Visibility Set System significantly optimized performance and rendering efficiency in NorthStar, particularly for VR gameplay. By dynamically managing visibility and LOD levels, we eliminated LOD popping, improved scene transitions, and enabled large-scale optimizations without sacrificing visual quality. 

With further editor enhancements, the system could become even more powerful, providing better visual debugging tools and workflow improvements for designers. 
