# Implementing Environment Transitions 

## Overview 

Scenes may require environment transitions to reflect changes in time of day, such as from midnight to late morning. Each transition uses a new Environment Profile and can be either instantaneous (within 0.01 seconds) or longform (5–50 seconds). 

Most transitions occur on teleport warps, limiting the number of transitions per scene. We carefully manage transitions to shift from one extreme weather and time profile to another smoothly. 

## Managing Transitions

Sometimes, duplicate Environment Profiles are needed with minor variations (e.g., different cloud textures or ocean wind speeds) to ensure natural-looking transitions. This prevents clouds and oceans from appearing like an unnatural timelapse. 

**Example Scenes:** 

- Beat6 – Rapid time-of-day shifts within 5 teleport warps. 
- Beat3and4 – Transitioning from clear day to stormy night. 

## Environment Profile 

Environment Profiles are data assets which consist of several parameters allowing us to create varying environmental, ocean and weather changes throughout a scene, see below for a screenshot of the data asset. 

![](./Images/EnvImplementation/Fig4.png)

 ## Beat6 Scene: Example of Environment Profile Transitions 

The Transitions in the Beat6 scene were a challenge to implement as the time of day changes rapidly which is difficult to convey in less than a few minutes and within 5 teleport warps.  The requirements for Beat 6 were to transition from the end of the stormy dark weather in Beat 5 into a late morning profile by the end of Beat 6. 

There are 6 Environment Profiles for Beat 6 where the starting profile is nearly identical to the stormy night environment profile that Beat 5 ends with. A few key changes were crucial for setting up seamless transitions from the dark storm profile into late morning profiles in Beat 6.  

  - The Skybox 

    - In order to create seamless transitions, the skybox had to be updated so that the clouds had the same values set as the rest of the transitions in the scene. These include; 

      - Cloud Offset 

      - Cloud Scale 

      - Cloud Speed 

      - Cloud Direction 

    - If these values vary drastically between profiles it can cause unintended results where the clouds look like they are lowering in the sky, and/or they move across the sky at an unnatural speed. 

      - You can get away with different cloud textures however, so long as these values are the same, it will reduce any unintended effects, but it is preferable to have the same cloud texture as there will be less chance of problems occurring during a longer transition. 

![](./Images/EnvImplementation/Fig6.gif)

- The Ocean

  - The ocean at the end of Beat 5 is quite choppy with a very large patch size and high wind speed, but the weather by the end of Beat 6 needs to be calmer and sunnier. The following had to be tweaked in order to create seamless transitions: 

    - Parameters on the Environment Profile: 

      - Wind Speed 

      - Directionality 

      - Patch Size 

        - Big changes in patch size for the ocean were hidden behind instant transitions. 

      - Ocean Material 

        - Ocean Normal Map texture 

        - Normals parameters 

        - Smoothness 

        - Displacement noise 

By changing the parameters above, more consistency can be achieved between Beat 4 and 5’s stormy weather, while also setting it up for the following Beat 6 transitions in a way which doesn’t feel jarring to the player as they teleport across the boat. 

For the night time and stormy environment profiles, we set the Sun Disk Material to be the Moon material so that the light reflections on the water are from the moon instead of the sun. 

  - This means that when we transition into or out of these environment profiles we need to create instant transitions where we swap the moon and sun.  

    - If it were a long transition the player would see the moon’s position rapidly change like a timelapse.

     - We hide this in Beat 6 between the second and third environment transitions, and this technique can also be seen in the Beat 3 and 4 scene where we transition from a clear day into the stormy night. 

To check how transitions are going to flow into one another we can drag different environment profiles onto the **‘Target Profile’** on the Environment prefab in a scene, in this case Beat 6, and adjust the Transition Time so we can get a feel for which transitions need to be instant vs longform, and how long those transitions should go for.  

![](./Images/EnvImplementation/Fig5.gif)

We set up the transitions using Unity Event Receivers on the Environment Profile prefab and assign them to Event Broadcasters on the Teleport Warps in the scene. Once the Receivers are set up, we add which Environment Profile the transition will use, how long the transition will be, and skybox updaters to make sure the transition is as smooth as possible. See below for an example of what the Transitions in Beat6 look like on the Environment Prefab. 

**Starting Environment Profile in Beat 6:**

![](./Images/EnvImplementation/Fig2.png)

**Final Environment Profile in Beat 6:**

![](./Images/EnvImplementation/Fig1.png)
 
**Example of the Beat 6 Environment Transitions Setup in Scene**

![](./Images/EnvImplementation/Fig0.png)
  
![](./Images/EnvImplementation/Fig3.png)



# Summary of Things to Look Out For 

### Sun and Moon positions  

- Higher or lower in the Sky 

  - Small changes in position can be used in an instant or long transition (5 - 30 seconds) 

  - Large changes in position can be used in an instant or long transition (20 - 50 seconds)  

    - NOTE: a large change in position needs to occur over a significant amount of time, otherwise the sun/moon will lerp quickly between the environment profiles and look unnatural to the player. 

- Swapping Moon and Sun 

    - Must only be done via an instant transition 

- Try to keep the sun and moon changes on the X axis only, so it’s only going up or down and not moving horizontally across the sky (can get away with Y axis changes between scenes if you really had to). 

### Ocean Patch sizes 

- Longer transitions require the same patch size because the patch size limits the max wave size, if the patch sizes are different it can cause visual inconsistencies when the transitions try to convert the ocean’s patch size, i.e. noticeable shifting / scrolling as the ocean patch size lerps to the new size. 

### Same skybox cloud values for longer transitions 

- Cloud Offset should be the same since this moves the height of the clouds up and down. 

- Cloud Speed should be the same otherwise there will be noticeable scrolling as the clouds shift to their new position 

- Cloud Scaling is similar to Cloud Speed, it can cause noticeable scrolling if they are different between environment profiles’ skyboxes. 

- NOTE: You don’t always have to set the clouds to the same values, this is only necessary when creating transitions which occur over an extended period of time. 

### Colours 

- It can be difficult to work out how to manage the colour changes between profiles, especially when the time of day is changing rapidly throughout the scene. 

- Making smaller changes earlier on in either instant or very short transitions (3 - 5 seconds) can help a great deal to get closer to some colour values which transition nicely over a longer period of time. 

- In a Beat 6 environment profile when the moon was in the ‘sun’ position on the environment profile, I had to make it seem like the sun was about to crest the ocean. To do that, I edited the Skybox ‘Cloud Back Color’ to look more orange, when typically that’s a darker version of the ‘Cloud Sun Color’.  

    - Considering tricks like this can be helpful when trying to create transitions between very different environment profile setups. 

- Experimenting with Post Processing profiles can also help to make the colours transition nicely between environment profiles. 

### Relevant Files
- [EnvironmentSystem.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Environment/EnvironmentSystem.cs)
- [EnvironmentProfile.cs](../Packages/com.meta.utilities.environment/Runtime/Scripts/Water/EnvironmentProfile.cs)
