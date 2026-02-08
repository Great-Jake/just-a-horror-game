# THE ECHOING ASYLUM - Unity Horror Game Setup Guide

## Overview
The Echoing Asylum is a fully playable first-person horror game set in an abandoned psychiatric hospital. This package includes all necessary scripts for a complete horror experience with advanced AI, sanity mechanics, environmental puzzles, and optional VR support.

## Features
- **First-Person Controller**: Walking, sprinting, crouching, flashlight with battery management
- **Hiding Mechanics**: Hide in closets and under beds to avoid enemies
- **Dynamic AI Enemy**: Hunts player based on noise, light, and movement
- **Environmental Puzzles**: Keys, generators, patient files
- **Sanity System**: Visual and audio distortions based on darkness and paranormal events
- **Collectible Evidence**: Affects story progression and multiple endings
- **Atmospheric Effects**: Realistic lighting, fog, flickering lights, ambient sounds
- **VR Support**: Optional head tracking and VR controls

## Quick Start Setup (10 Minutes)

### 1. Project Setup
1. Create a new Unity 3D project (Unity 2020.3 or later recommended)
2. Import all provided C# scripts into your `Assets/Scripts` folder
3. Install required packages:
   - Post Processing Stack v2 (Window → Package Manager → Post Processing)
   - Universal Render Pipeline (optional, for better visuals)
   - XR Plugin Management (if using VR)

### 2. Scene Setup

#### A. Create the Player
1. Create an empty GameObject named "Player"
2. Add a **CharacterController** component
3. Add **PlayerController.cs**
4. Add **SanitySystem.cs**
5. Create a child object "Main Camera"
6. Add **VRSupport.cs** to camera (optional)

#### B. Create the Flashlight
1. Create a child GameObject under Player named "Flashlight"
2. Add a **Light** component (type: Spot)
3. Set intensity to 1, range to 15, angle to 45
4. In PlayerController, assign this light to the Flashlight field

#### C. Create the Enemy
1. Create a Capsule primitive (GameObject → 3D Object → Capsule)
2. Name it "Enemy"
3. Add **NavMeshAgent** component
4. Add **EnemyAI.cs**
5. Create two materials: one for normal state (gray), one for hunting (red)
6. Assign materials in the EnemyAI component
7. Add a Point Light as child (red color for atmosphere)

#### D. Create the Environment
1. Create an empty GameObject named "Environment"
2. Add **EnvironmentController.cs**
3. Create several Point Lights around the scene
4. Drag them into the "Flickering Lights" array in EnvironmentController

#### E. Create the Game Manager
1. Create an empty GameObject named "GameManager"
2. Add **GameManager.cs**
3. Create UI Canvas (GameObject → UI → Canvas)
4. Set Canvas to Screen Space - Overlay

#### F. Create UI Elements
On your Canvas, create:
- **Text** for interaction prompts (name: InteractionText)
- **Text** for messages (name: MessageText)
- **Panel** for HUD with:
  - Battery bar (Image with Fill type)
  - Sanity bar (Image with Fill type)
  - Stamina bar (Image with Fill type)
  - Text elements for labels
- **Panel** for document reading (name: DocumentPanel)
  - Title text
  - Content text (large)
- **Panel** for game over screen (name: GameOverPanel)
- **Panel** for pause menu (name: PausePanel)

Link all UI elements to the GameManager component.

#### G. Create Interactable Objects

**Key:**
1. Create a Cube primitive
2. Add **KeyItem** component from InteractableObjects.cs
3. Remove the collider, add a Trigger Box Collider
4. Set tag to "Interactable"

**Door:**
1. Create a Cube primitive (scale: 1, 3, 0.2)
2. Add **LockedDoor** component
3. Add a Trigger Box Collider
4. Set required key ID

**Generator:**
1. Create a Cube primitive
2. Add **Generator** component
3. Assign lights to activate when powered
4. Add audio source

**Patient File:**
1. Create a Quad primitive
2. Add **PatientFile** component
3. Add trigger collider
4. Write document content in inspector

**Hiding Spot:**
1. Create a Cube primitive (wardrobe/closet size)
2. Add **HidingSpot** component
3. Add trigger collider
4. Create a child "HidePosition" transform

**Battery:**
1. Create a Cylinder primitive (small)
2. Add **BatteryPickup** component
3. Add trigger collider

#### H. Setup Post Processing
1. Create a **Post-process Volume** (GameObject → 3D Object → Post-process Volume)
2. Check "Is Global"
3. Create a new Post-process Profile
4. Add effects:
   - Vignette
   - Chromatic Aberration
   - Lens Distortion
   - Bloom
5. Assign profile to SanitySystem component

#### I. Setup NavMesh
1. Select all floor/ground objects
2. Mark as "Navigation Static" in inspector
3. Window → AI → Navigation
4. Click "Bake"

### 3. Configure Components

#### PlayerController Settings:
- Walk Speed: 3.5
- Sprint Speed: 6.0
- Crouch Speed: 2.0
- Max Battery: 100
- Battery Drain Rate: 5
- Mouse Sensitivity: 2.0

#### EnemyAI Settings:
- Detection Radius: 15
- Hearing Radius: 20
- Patrol Speed: 2
- Hunt Speed: 5.5
- Create patrol points or enable random patrol

#### SanitySystem Settings:
- Max Sanity: 100
- Darkness Decay Rate: 2
- Light Recovery Rate: 5
- Assign Post-process Volume

### 4. Layer Setup
Create these layers:
- "Interactable" (for interactive objects)
- "LightSource" (for detecting nearby lights)
- "Obstacle" (for AI line-of-sight)

### 5. Tag Setup
Create these tags:
- "Player"
- "Enemy"
- "Interactable"

### 6. Audio Setup
Add audio clips:
- Ambient sounds (wind, creaking, whispers)
- Flashlight toggle sound
- Footstep sounds
- Enemy detection sound
- Heart beat sound for low sanity
- Background music
- Chase music

### 7. Build and Play
1. Set player spawn position
2. Set enemy spawn position
3. Place keys, generators, documents around level
4. Press Play!

## Controls

### Keyboard & Mouse:
- **W/A/S/D** - Move
- **Shift** - Sprint (uses stamina)
- **C or Ctrl** - Crouch
- **F** - Toggle Flashlight
- **E** - Interact
- **Mouse** - Look around
- **Esc** - Pause

### VR (if enabled):
- **Left Stick** - Move
- **Right Stick** - Snap turn / Teleport indicator
- **Trigger** - Toggle flashlight
- **Grip** - Interact
- **Menu Button** - Pause

## Advanced Setup

### Procedural Level Generation
1. Create an empty GameObject
2. Add **AsylumLevelGenerator.cs**
3. Configure settings:
   - Number of rooms
   - Room size range
   - Hallway width
4. Assign prefabs for player, enemy, objects
5. Play - level generates automatically!

### Multiple Endings
The game supports multiple endings based on:
- Evidence collected (5+ = good ending)
- Generators activated (3 required)
- Player survival

Configure in GameManager:
- totalEvidenceRequired
- generatorsRequired

### Custom Materials
For better visuals:
1. Create custom materials for walls, floors
2. Add normal maps for texture
3. Use emissive materials for lights
4. Add skybox for atmosphere

### Sound Design Tips
1. Use 3D spatial audio for immersion
2. Set audio rolloff for distance
3. Add reverb zones in hallways
4. Use audio mixer for volume control
5. Layer multiple ambient tracks

### VR Optimization
1. Keep triangle count low
2. Use occlusion culling
3. Reduce shadow quality
4. Enable single-pass stereo rendering
5. Test comfort settings with testers

## Troubleshooting

### Player falls through floor:
- Check CharacterController is on Player object
- Ensure floor has colliders

### Enemy doesn't move:
- Verify NavMesh is baked
- Check NavMeshAgent component
- Ensure patrol points exist or random patrol enabled

### Flashlight doesn't work:
- Verify Light component assigned in PlayerController
- Check battery isn't depleted
- Ensure script references are set

### UI doesn't appear:
- Check Canvas render mode (Screen Space - Overlay)
- Verify UI elements linked to GameManager
- Check Canvas Scaler settings

### Sanity effects don't work:
- Ensure Post-process Volume exists
- Check "Is Global" is enabled
- Verify profile has required effects
- Check camera has Post-process Layer component

### VR not working:
- Install XR Plugin Management
- Enable VR SDK in player settings
- Check VR headset is connected
- Verify VRSupport component has references

## Performance Tips
1. **Occlusion Culling**: Enable in Camera settings
2. **LOD Groups**: Use for complex objects
3. **Batching**: Enable static/dynamic batching
4. **Lighting**: Bake lighting where possible
5. **Shadows**: Use medium quality, reduce distance
6. **Particle Systems**: Limit max particles
7. **Audio**: Limit max voices to 32

## Customization Ideas

### Easy Modifications:
- Change wall/floor materials
- Adjust lighting colors
- Add more hiding spots
- Create new document stories
- Change enemy appearance

### Advanced Modifications:
- Multiple enemy types
- Weapons/defense items
- Inventory system
- Save/load system
- Co-op multiplayer
- Procedural story generation

## Credits & License
This horror game template is provided as-is for educational and commercial use.
Feel free to modify, extend, and publish your own versions.

## Support
For issues or questions:
1. Check all references are assigned in Inspector
2. Verify components are on correct GameObjects
3. Check console for error messages
4. Ensure all required packages are installed

## What's Next?
1. **Art Pass**: Replace primitives with 3D models
2. **Sound Design**: Add professional audio
3. **Story**: Write compelling patient files
4. **Polish**: Add particle effects, improved UI
5. **Testing**: Playtest and balance difficulty
6. **Marketing**: Create trailer, screenshots

Good luck creating your horror masterpiece!
