# North Star’s implementation of ULipSync 

To support fully voiced NPC dialogue, NorthStar needed an efficient lip-syncing solution without relying heavily on animators. [ULipSync](https://github.com/hecomi/uLipSync) was chosen due to the team’s familiarity with the plugin and its strong support for narrative control, customization, and ease of use. 

 ## ULipSync Setup  

ULipSync offers three ways to process lip-sync data: 

1. Runtime processing – Analyzes audio dynamically. 

2. Baking into scriptable objects – Stores data for reuse. 

3. Baking into animation clips – Prepares animations for timeline use. 

Given the project’s CPU constraints and use of narrative timelines, baking the data into animation clips was the most suitable approach. 

![](./Images/LipSync/Fig2.png)

## Phoneme Sampling & Viseme Groups 
ULipSync maps phonemes (smallest speech components) to viseme groups (blend shape controls for facial animation). 

 - English has **44 phonemes**, but not all are necessary for lip-syncing. 

 - **Plosive sounds** (e.g., "P" or "K") are difficult to calibrate and may not significantly impact the final animation. 

 - Stylized models require fewer viseme groups than realistic ones, sometimes only needing vowels. 

Since we couldn’t determine upfront which phonemes were essential, we recorded all 44 phonemes for each voice actor. This ensured flexibility in refining the system later. 

![](./Images/LipSync/Fig0.png)


## Challenges in Phoneme Sampling 

Not all phonemes were sampled perfectly. Issues included: 

 - Regression effects, where certain phonemes worsened results. 

 - Lack of matching viseme groups, making some phonemes irrelevant. 

 - Volume inconsistencies, causing some sounds to be too quiet for accurate sampling. 

To refine accuracy, we documented problematic phonemes for future improvements and considered additional recordings where necessary.

## Ensuring Realistic Lip-Sync 

A common issue in automated lip-syncing is excessive mouth openness. Realistic speech involves frequent mouth closures for certain sounds. To address this: 

 - We referenced real-life speech patterns. 

 - Animators provided feedback to refine mouth movement accuracy. 

## Final Implementation 

Each voice line was baked with a pre-calibrated sample array, storing blend shape weights per NPC. This per-character approach worked due to a limited NPC count, but a more generalized system would be required for larger-scale projects. 

![](./Images/LipSync/Fig1.png)

### Relevant Files
- [NpcController.cs](../Assets/NorthStar/Scripts/NPC/NpcController.cs)
- [NpcRigController.cs](../Assets/NorthStar/Scripts/NPC/NpcRigController.cs)
