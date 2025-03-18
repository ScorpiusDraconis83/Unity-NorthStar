# Full Body Tracking

NorthStar supports full-body tracking using the [Meta Movement SDK](https://developers.meta.com/horizon/documentation/unity/move-overview/). While the SDK provides a solid foundation that we implemented logic on top to serve the needs of this project. 

![](./Images/FullBodyTracking/Fig1.png)

## Meta Movement SDK Setup

To set up body tracking, follow the recommended workflow: 
- Right-click the player rig GameObject. 
- Select the body tracking setup option. 

![](./Images/FullBodyTracking/Fig0.png)

This process configures the RetargetingLayer component and the FullBodyDeformationConstraint, which work largely out of the box.

## Calibration

**Avatar Scaling**

Once the player has calibrated their height in the menu, we are able to scale the player avatar to match their height. We scale the player rig based on it’s initial height vs the player’s height. We also need to adjust the rig slightly to take the height offset into account.

#### Relevant Files
- [PlayerCalibration.cs](../Assets/NorthStar/Scripts/Player/PlayerCalibration.cs)

**Seated vs Standing**

Since North Star needed to support both seated and stading play, we added an additional offset to the player rig so that they would appear standing in the game even if the player is currently sitting. The offset is calculated based on the difference between the player’s calibrated standing and seated height, which can be set in the menu.

There were some issues with the full-body tracking system since it does not seem to support manual offsets which would cause the player rig to crouch no matter how you offset the skeleton. To work around this, we opted to use only upper body tracking, while taking care of the leg placement ourselves. 

**Custom Leg Solution**

![](./Images/FullBodyTracking/Fig2.png)

Initially we used the built-in full body tracking leg solution that comes with the Movement SDK. This worked fine for standing play, but we were unable to find an option to accommodate seated play. Ultimately, we ended up creating an alternative leg-IK solution (using Unity's rigging package) that provided similar functionality for foot placement / stepping / crouching.

#### Relevant Files
- [BodyPositions.cs](../Assets/NorthStar/Scripts/Player/BodyPositions.cs)

## Postprocessors

**Physics Hands Retargeting**

Due to using physically simulated hands, we had to use a RetargetingProcessor to retarget the hands using IK. The CustomRetargetingProcessorCorrectHand is a modified version of the original from Meta that also supports overriding hand rotation in addition to position. 

#### Relevant Files
- [CustomRetargetingProcessorCorrectHand.cs](../Assets/NorthStar/Scripts/Player/CustomRetargetingProcessorCorrectHand.cs)

**Hands, Elbow and Twist Bone Correction**

After the hand and arm bones have been retargeted, the elbow and twist bones are sometimes out of alignment. To correct this, we introduced an additional processor that corrects the elbow and twist bone orientation which involved keeping the elbow locked to one axis (relative to the shoulder), while distributing the wrist rotation along the twist and wrist bones. 

To match the player’s tracked hands more closely we also use this processor to match the tracked finger bones. 

#### Relevant Files
- [SyntheticHandRetargetingProcessor.cs](../Assets/NorthStar/Scripts/Player/SyntheticHandRetargetingProcessor.cs)

## Tracking Failure

There were several cases in which full body tracking would fail. Sometimes tracking would fail to initialize, especially when testing via Oculus Link during development. 

When tracking failure is detected we simply switch off the full-body rig and change back to the standard floating ghost hands. We also attempt to shut down and restart body tracking whenever the headset waves from sleep or put back on after being taken off. 

Unfortunately, there still remains a strange, persistent bug where the player’s body would be squished into the floor, while no indication of tracking failure would be reported by the SDK. The cause remains unknown for now. 

## Future Improvements

There are still some issues with the full body tracking implementation that could be improved, such as slight alignment issues with the avatar’s head as well as the feet floating off the ground when in seated mode. 
