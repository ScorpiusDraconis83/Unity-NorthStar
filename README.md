![](./Assets/OculusSystemSplashScreen.png)

# North Star

*North Star* is a visual showcase that demonstrates the visual fidelity possible on Meta Quest devices. It has been built using [*Interaction SDK*](https://developer.oculus.com/documentation/unity/unity-isdk-interaction-sdk-overview/) for interactions, [*Movement SDK*](https://developers.meta.com/horizon/documentation/unity/move-overview) for full body tracking, and [*Audio SDK*](https://developers.meta.com/horizon/documentation/unity/meta-xr-audio-sdk-unity) for spatialized sounds. It is designed to be used primarily for the best intended hand-tracking experience but also provides full controller support.

This showcase is also available to play for free on the [Horizon Store](https://www.meta.com/experiences/north-star/28679538058299918/).

## Licenses

The majority of North Star is licensed under MIT LICENSE. However, files from [Text Mesh Pro](http://www.unity3d.com/legal/licenses/Unity_Companion_License), [DOTween](http://dotween.demigiant.com/license.php), [Unity URP](./Packages/com.unity.render-pipelines.universal/LICENSE.md), and any other third party files are licensed under their respective licensing terms.

## Contributing

See the [CONTRIBUTING](./CONTRIBUTING.md) file for how to help out.

# Getting Started

## Getting The Code

First, ensure you have Git LFS installed by running this command:

```sh
git lfs install
```

Then, clone this repo using the "Code" button above, or this command:

```sh
git clone https://github.com/oculus-samples/Unity-NorthStar.git
```

## How to run the project in Unity

1. Make sure you're using *Unity 2022.3.34f1* or newer.
2. Load the [Assets/NorthStar/Scenes/#Staging/Launch](./Assets/NorthStar/Scenes/%23Staging/Launch.unity) scene.
3. There are two ways of testing in the editor:
    <details>
    <summary><b>Quest Link</b></summary>

    - Enable Quest Link:
        - Put on your headset and navigate to "Quick Settings"; select "Quest Link" (or "Quest Air Link" if using Air Link).
        - Select your desktop from the list and then select, "Launch". This will launch the Quest Link app, allowing you to control your desktop from your headset.
    - With the headset on, select "Desktop" from the control panel in front of you. You should be able to see your desktop in VR!
    - Navigate to Unity and press "Play" - the application should launch on your headset automatically.
    </details>
    <details>
    <summary><b>Meta XR Simulator</b></summary>

    - Select Meta -> Simulator -> Enable Simulator
    - Press Play
    - The simulator should open a new window ([Simulator Docs](https://developer.oculus.com/documentation/unity/xrsim-intro/))
    </details>

# Showcase Features

Each of these features have been built to be accessible and scalable for other developers to take and build upon in their own projects.

## Ocean

*North Star* includes a highly customizable ocean system that is optimized to be quite performant on device while achieving a visually realistic ocean volume. Developers can control the hue, activity/choppiness of the water, reflections, height and intensity of waves through the inspector, and transition smoothly between profiles.

## Boat Movement

With most of the game taking place on the vessel *Polaris* as it sails through a variety of ocean conditions, *North Star* achieves a variable motion on the boat to simulate the natural bobbing and swaying of sailing while paying special heed to the player’s comfort.

## Comfort Settings

Before starting the game, the player can set up their experience through the main menu. In addition to seated mode and height detection, the player can elect their sailing comfort level. There are 4 comfort levels to choose from (Calm, Choppy, Rough, Sailor), with the lowest dampening the natural movement of the boat to be negligible, and each following level intensifying the sailing experience further towards the highest and most realistic level.

## Time of Day

Included in the *North Star* project is a Time of Day system, where designers can define Environment Profiles to determine the sun/moon position, color of the sky, directional light and more. Designers can set up multiple Environment Profiles and either transition between them over a duration, or switch to new profiles immediately (ie: during a teleport or load screen).

## Ropes

*North Star* was built with the intention of providing the most realistic and tactile rope interactions available in VR gaming. Ropes respond naturally when interacted with, allowing players to wrap them around other objects (like securing the rope from the mainsail to a cleat), swing and propel other objects (like a life buoy) and generally behave realistically when thrown, pulled and jiggled.

## Interactables

Utilizing Meta’s Interactions SDK and built specifically with hand tracking in mind, there are a variety of unique interactions available within the *North Star* experience:

**Rope & sail manipulation:** Pull ropes to raise the mainsail and secure it off on a cleat, untie to furl the sails.

**Cranks**: Use a crank to re-angle the sails to line them up with windsocks and catch the breeze!

**Levers**: Pull the lever to begin the descent of the Diving Bell.

**Harpoon Gun**: Load bolts, pull back the tension, aim and hit the button to let loose a harpoon! Wind the cranks to retrieve cargo salvaged from the ocean.

**Spyglass**: Extend the spyglass and hold it to an eye to magnify your view and look through fog.

**Object Manipulation**: Pick up and interact naturally with a number of objects found aboard the *Polaris*, and take your turn navigating at the helm!

## Narrative & NPCs

Engage in an exciting adventure inspired by the age of exploration! Join the dynamic crew of the *Polaris* as it sets sail to find a mythical and mysterious treasure in the depths of the ocean known as Ocean Deep. Fully animated and voiced NPCs guide you through your adventure, utilizing lipsync technology when addressing you, the new deckhand.

## Free Sailing

Jump into free sailing mode to put everything you’ve learned along your journey to the test! Freely manipulate the sails to catch the wind and feel the ocean rock you under your feet.

# Source Voiceovers

To protect the voiceover talent employed for this project, the original source VO files that are utilized in the store release of North Star are not present in this public project repository. Instead all original source VO have been replaced with rudimentary placeholder generated VO.

All original VO talent was sourced from the [Big Mouth Voices online agency](https://bigmouthvoices.com/).

# Dependencies

This project makes use of the following plugins and software:

- [Unity](https://unity.com/download) 2022.3.34f1 or newer
- Meta [Core XR SDK](https://developer.oculus.com/downloads/package/unity-integration), [Movement SDK](https://github.com/oculus-samples/Unity-Movement), [Interaction SDK](https://developer.oculus.com/documentation/unity/unity-isdk-interaction-sdk-overview/), [Audio SDK](https://developers.meta.com/horizon/documentation/unity/meta-xr-audio-sdk-unity/), and [XR Simulator](https://developers.meta.com/horizon/documentation/unity/xrsim-intro/) (all released under the *[Oculus SDK License Agreement](https://developers.meta.com/horizon/licenses/)*).
- [Unity Universal Render Pipeline (Meta ASW fork)](https://developer.oculus.com/documentation/unity/unity-asw/#how-to-enable-appsw-in-app)
- [uLipSync](https://github.com/hecomi/uLipSync#upm)
- [UnityJigglePhysics](https://github.com/naelstrof/UnityJigglePhysics#upm)
- [Unity Spline Editor](https://github.com/vvrvvd/Unity-Spline-Editor#upm)
- [CoACD](https://github.com/laurentopia/CoACD)

The following is required to test this project within Unity:

- [Meta Quest Link](https://www.oculus.com/setup/)
