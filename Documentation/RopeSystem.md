# Rope System 

A major part of NorthStar’s development focused on creating a compelling and realistic rope simulation system. To achieve this, we implemented a hybrid approach combining Verlet integration for simulation and anchoring constraints for interactive rope behavior. 

## Verlet Simulation 

![](./Images/RopeSystem/Fig3.png)

Verlet integration is well-suited for rope physics as it simplifies point mass calculations. Each point mass stores only its current and previous position (assuming a unit mass), and constraints are enforced by iteratively adjusting point positions to maintain the rope’s rest length. 

We used this approach to simulate the visual behavior of ropes reacting to player interactions. However, the forces only traveled in one direction—from the player to the rope, rather than bidirectionally. 

To further improve stability, we implemented a binding system that allowed specific nodes in the rope simulation to behave as kinematic points (not affected by forces or constraints). To support large numbers of nodes and high iteration counts for constraint solving, the Verlet simulation was optimized using Burst Jobs for better performance. 

### Relevant Files
- [BurstRope.cs](../Assets/NorthStar/Scripts/Rope/VerletRope/BurstRope.cs)

## Anchoring Constraints 

To allow the player to interact with and be constrained by ropes, we developed an anchoring system. This system enabled ropes to wrap around static objects, dynamically creating bends that constrained the Verlet simulation. 

**How Anchors Work:**

- When the rope encounters an obstacle (based on normal direction, bend angle, and bend direction), a new anchor is created.
- If the player is holding the rope, the system calculates the rope length between two anchors and applies a configurable joint with a linear limit to prevent overstretching.
- Slack and spooled rope are also accounted for, allowing for:
    - Loose rope to be pulled through when tightening.
    - Extra rope to be spooled out, such as for sail controls. 
- If the player exerts enough force, the rope can slip, allowing hands to slide along it like a real rope.
- The number of bends, slack amount, and spooled rope length can trigger events when the rope is pulled tight or tied. 

### Relevant Files
- [RopeSystem.cs](../Assets/NorthStar/Scripts/Rope/RopeSystem.cs)

## Tube Rendering 

![](./Images/RopeSystem/Fig2.png)

To visually render the ropes, we developed a tube renderer that: 

- Uses the rope nodes to generate a lofted mesh along a spline.
- Supports subdivisions for additional detail.
- Adds indentation and twisting for a more realistic rope appearance.
- Utilizes normal mapping for enhanced depth and texture.
- Is optimized using Burst Jobs for efficient performance. 

### Relevant Files
- [BurstRope.cs](../Assets/NorthStar/Scripts/Rope/VerletRope/TubeRenderer.cs)

## Collision Detection 

![](./Images/RopeSystem/Fig1.png)

Handling rope collision detection efficiently was a major challenge. We used Physics.ComputePenetration() to detect interactions between Verlet nodes and nearby level geometry. However, there were two key issues: 
1. ComputePenetration is not compatible with Jobs or Burst, meaning collision detection had to be performed on the main thread after Verlet simulation.
2. Single collision checks per frame caused phasing issues, as ropes would pass through objects when nodes were forced apart.

**Optimizations for Better Collision Detection:** 

To resolve these issues, we: 
- Split the rope simulation into multiple sub-steps, running a collision check after each sub-step.
- Forced the first job to complete immediately, allowing for collision checks early in the frame.
- Performed the second sub-step during the frame, resolving it in LateUpdate() for increased stability.
- Used SphereOverlapCommand in a Job to efficiently gather potential collisions without stalling the main thread. 

### Relevant Files
- [BurstRope.cs](../Assets/NorthStar/Scripts/Rope/VerletRope/BurstRope.cs)

## Editor Workflow 

![](./Images/RopeSystem/Fig4.png)

We streamlined the process of adding and configuring ropes in scenes with an intuitive editor workflow: 
- Start with the RopeSystem prefab.
- Edit the included spline to define the desired rope shape.
- Use context menu options in the RopeSystem component to:
    - Set up nodes.
    - Define the rope’s total length.
- Run the simulation in a test scene and allow the rope to settle naturally.
- Copy the anchor points and BurstRope nodes from the simulation back into the editor.
- Finalize the rope setup for use in live gameplay. 

![](./Images/RopeSystem/Fig0.png)

#### Relevant Files
- [RopeSystem.prefab](../Assets/NorthStar/Prefabs/RopeSystem.prefab)

## Conclusion

By combining Verlet simulation with dynamic anchoring constraints, we created a realistic and performant rope system for NorthStar. The use of Burst Jobs, tube rendering, and multi-step collision detection allowed us to balance realism, interactivity, and performance. The editor workflow further streamlined the development process, enabling efficient iteration and fine-tuning of rope behaviors. 
