# Boat Movement 

## Overview

The boat in North Star is a core element, requiring smooth movement, turning, and ocean interaction while allowing the player to move freely onboard. Several approaches were explored to achieve this. 

## Movement Approaches

### True Movement 

The boat was initially implemented as a kinematic rigid body moving via scripting, with all objects physically on top of it. However, this caused issues with hand tracking and physics constraints, leading us to abandon this approach. 

### Move the World 

Another idea was to move the environment around the boat instead of the boat itself. However, this led to complications with persistent objects outside the boat, so it was not pursued. 

### Fake Movement

The final approach that we stuck with used a “fake movement” system, where the boat’s visual position is updated just before rendering and reset afterward. This prevents physics issues, avoids dragging objects along with the boat, and removes the need to move the world around the boat (ocean, islands, sky, reflections, etc.). Several helper functions were also developed for transforming between world space and “boat space.” 

#### Relevant File
- [Fake Movement](../Assets/NorthStar/Scripts/Ship/FakeMovement.cs)

## Rocking and Bobbing 

To simulate ocean movement, a procedural noise system was implemented to make the boat rock and bob. The effect scales with boat speed, which is influenced by wind direction and sail angle. A more physically realistic wave-height and momentum-based system was tested but was ultimately replaced for better player comfort and direct movement control. 

## Reaction Movement 

Specific scripted boat movements were implemented using the timeline for special events, such as waves hitting the boat or attacks from creatures like the Kraken. 

### Relevant Files
- [BoatMovementAsset](../Assets/NorthStar/Scripts/Wave/BoatMovementAsset.cs)
- [BoatMovementBehaviour](../Assets/NorthStar/Scripts/Wave/BoatMovementBehaviour.cs)
- [BoatMovementMixerBehaviour](../Assets/NorthStar/Scripts/Wave/BoatMovementMixerBehaviour.cs)
- [BoatMovementTrack](../Assets/NorthStar/Scripts/Wave/BoatMovementTrack.cs)
- [WaveControlAsset](../Assets/NorthStar/Scripts/Wave/WaveControlAsset.cs)
- [WaveControlBehaviour](../Assets/NorthStar/Scripts/Wave/WaveControlBehaviour.cs)
- [WaveControlTrack](../Assets/NorthStar/Scripts/Wave/WaveControlTrack.cs)


![](./Images/BoatMovement/Fig0.png)

## Comfort Settings 

To ensure player comfort, procedural motion is controlled directly, allowing the magnitude to be adjusted. An additional comfort option locks the horizon angle in place, keeping the player upright as if they had perfect “sea legs.” This prevents motion sickness by stabilizing the horizon.

```cs
var boatRotation = BoatController.Instance.MovementSource.CurrentRotation;
CameraRig.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(boatRotation), GlobalSettings.PlayerSettings.ReorientStrength) * CameraRig.parent.localRotation;
```
*(from [BodyPositions.cs](../Assets/NorthStar/Scripts/Player/BodyPositions.cs#L241-L242))*

![](./Images/BoatMovement/Fig1.png)
