## Spyglass

In NorthStar a telescope allows players to see farther distances through the dense fog. It achieves this by using an adjusted projection matrix to magnify objects and reduce fog density allowing the player to see distant objects. It is opened by pulling on the eyeglass, extending the length of the telescope.

![](./Images/Eyeglass/Fig1.png)

### Interactions

The telescope is separated into 2 objects, a Front and a Back, each with a Rigidbody and containing many grab points. A joint holds the two parts together. A script monitors the separation between these Rigidbodies and will update the telecope state (open or closed) based on this separation. Upon state change, the joint targetPosition is adjusted to hold the telescope open or closed, and eyeglass rendering is enabled or disabled.

When the telescope is placed up to the user’s eye, any jitter becomes very apparent and can make the telescope difficult or sickening to use. To combat this, the telescope will use another joint to attach itself to the nearest eye for stabilization.

![](./Images/Eyeglass/Fig2.png)

### Rendering

Initially this system rendered the world to a RenderTexture and composited this texture over the eyeglass. However this technique proved to be too slow for real-time rendering. To reduce processing costs, a ScriptableRenderPass renders the visible world directly to the screen with adjusted projection matrices, viewport, and stenciled to only render inside the eyeglass. It uses the same culling as the main scene.

First the lens is drawn, writing a stencil bit to mark pixels of the eyeglass that are visible. This ensures that the magnified world is only visible through the eyeglass, and any objects obscuring the eyeglass will also obscure the magnified world.

A screen rect is then calculated for each eye based on the bounds of the lens mesh. Because of the separation of eyes, the screen-space bounds of the lens can be significantly different for each eye. These rects are combined to construct a screen-space viewport containing the lens for both eyes and used to adjust the projection matrix of each eye. The projection matrix is also skewed by the relative telescope orientation multiplied by a “trackingFactor”.

Fog density is adjusted by overwriting the FogParams vector, and control is passed to the base DrawObjectsPass to render the game scene, before restoring everything to its initial state.

Finally the lens is drawn again with a smooth transparent material to give the lens a glassy look.

### Relevant Files
- [Telescope.cs](../Assets/NorthStar/Scripts/Items/Telescope.cs)
- [ViewportRenderer.cs](../Packages/com.meta.utilities.viewport-renderer/Scripts/ViewportRenderer.cs)
- [EyeStencilWrite.shader](../Packages/com.meta.utilities.viewport-renderer/Materials/EyeStencilWrite.shader)
