# Scenes README
North Star presents its narrative in a series of gameplay "beats", which each feature unique locations, time of day and weather effects. In Unity, these beats are broken into scenes so they could be worked on in parallel and manage performance.

## Beat 1: 
Beat 1 commences with the player adrift, castaway in the middle of the ocean on a piece of debris. They can ring the bell to start the adventure, and use the available Spyglass to pierce the surrounding fog and witness an approaching vessel.

The _Polaris_ emerges from the fog, her crew spotting the player and rescuing them from their peril.

### Features
- Main menu UI, featuring player scale options and sea-sickness comfort settings.
- Ringing a large ship bell.
- Spyglass that articulates on joints to open and close, and allows players to see further through fog when used.
- Silver hand shader at its earliest/beginning point.

## Beat 2:
Beat 2 starts with the player awakening in the Captain's Cabin onboard the _Polaris_. They learn to teleport and are able to pick up and interact with items on the table as the Captain, Bessie, enters and greets the player. Following her out onto the main deck, it's revealed the crew have arrived at an island and are currently docked. 

The player is offered to join as the ship's new deckhand and is put through a series of tests to prove they can join the crew. Along the way they meet Audrey (ship Engineer) and Thomas (Navigator). They learn how to set sail and take the helm to commence the adventure.

### Features
- Teleport FTU.
- Object manipulation.
- Spinning fixed object (globe).
- Rolling object (barrel).
- Rope manipulation (pull to raise mainsail & tie off on a cleat).
- Helm control.

## Beat 3:
During Beat 3 the _Polaris_ is out in open ocean once again. The player learns to re-angle the sails while getting to know their fellow crew members. A large wave knocks cargo overboard, and Audrey teaches the player how to use the Harpoon to retrieve crates from the ocean.

### Features
- Winding crank (re-angle sails with wind direction).
- Progressive advancement of time of day.
- Harpoon manipulation to salvage cargo.
- Silver hand progress.

## Beat 4:
Looking through the spyglass reveals an approaching storm. The player helps tie down and secure the cargo before the storm hits! The player is assailed by beating rain, heavy waves and flashing lightning as they retreat to the safty of the cabin.

### Features
- Severe weather change (light rain to heavy, lightning, dangerous ocean waves).
- Storm lighting when inside the cabin.
- Silver hand progress.

## Beat 5:
When the storm eases, the crew leave the cabin to inspect the damage to the _Polaris_. The ship is knocked and Thomas is thrown overboard. When the player throws him a life buoy, a giant tentacle hoists him in the air. The Kraken has emerged. The player must use the harpoon to defend the ship from the tentacles, before working with Audrey to reveal the beast's head and finally drive him off.

### Features
- Throw life buoy.
- Quick-reloading harpoon.
- Kraken battle.

## Beat 6
Thomas scrambles back onboard safely. Bessie reveals that they have arrived at their destination, based on the player's hand turning completely silver. Togehter with Audrey they turn two cranks to lift a heavy diving bell onto the deck. The player enters and is dropped into the ocean to seek the treasure below.

### Features
- Silver hand completed state.
- Combined cranking (hoisting diving bell).
- Lock diving bell hatch.
- Gesture to signal ready (thumbs up).

## Beat 7:
The player pulls the lever to descend through the depths of the ocean in the diving bell, witnessing the marvels that lay in wait. When they step out of the diving bell, they can use their hands to gesture towards the waiting shimmering orb, drawing it towards them, and witness the conclusion of the adventure.

### Features
- Diving bell descent.
- Underwater shaders.
- Orb gesture attraction.
- Looping time effect.
- Circling back to Beat 1.
