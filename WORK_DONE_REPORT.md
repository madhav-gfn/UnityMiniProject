# 3. WORK DONE (REPORT ON PRESENT INVESTIGATION)

This chapter presents the work carried out during the present investigation of the VR multiplayer dungeon extraction game project. The project has been implemented in Unity, and the complete repository has been considered as part of the project base. This includes imported framework code, Photon networking code, Unity package configuration, dungeon assets, scenes, prefabs, and source scripts.

The implementation comprises a VR interaction foundation through BNG Framework, multiplayer support through Photon Unity Networking, dungeon art assets through PolygonDungeon, XR platform support through OpenXR and Oculus XR, and gameplay modules for locomotion, grabbing, hand animation, UI interaction, weapons, damage handling, projectiles, bow mechanics, and networked object synchronization.

The work described in this chapter is based on the functionality and assets present in the current Unity project folder. Features that are planned but not represented in the current codebase are identified separately to maintain report accuracy.

### Present Investigation Overview

| Item | Availability | Evidence from Project |
|---|---:|---|
| Unity project | Available | `ProjectSettings/ProjectVersion.txt` shows Unity 6000.3.13f1 |
| C# source files | Available | 397 C# files under `Assets` |
| VR interaction framework | Available | `Assets/BNG Framework` |
| Photon multiplayer framework | Available | `Assets/Photon` and `Assets/BNG Framework/Integrations/PUN` |
| Dungeon environment assets | Available | `Assets/PolygonDungeon` |
| Main Unity scenes | Available | `Assets/Scenes/SampleScene.unity`, BNG scenes, Photon scenes, PolygonDungeon scenes |
| XR packages | Available | Oculus XR, OpenXR, XR Management in `Packages/manifest.json` |
| URP package | Available | Universal Render Pipeline 17.3.0 in `Packages/manifest.json` |
| AI Navigation package | Available | `com.unity.ai.navigation` 2.0.12 in `Packages/manifest.json` |
| Custom enemy NavMesh controller script | Not available | No `NavMeshAgent` or `UnityEngine.AI` usage found in current C# scripts |

## 3.1 Tools and Technologies Used

Unity is used as the main development environment for the project. Unity is suitable for this work because it supports VR development, real time three dimensional rendering, physics simulation, C# scripting, XR device integration, and multiplayer framework integration. The current Unity editor version used by the project is Unity 6000.3.13f1.

### Development Environment

| Category | Tool or Technology | Version or Status | Purpose in Project |
|---|---|---|---|
| Game engine | Unity | 6000.3.13f1 | Main development engine for scenes, GameObjects, prefabs, physics, scripting, rendering, and build configuration |
| Programming language | C# | Unity C# scripting | Used for VR interaction, player control, networking integration, combat logic, UI logic, and component behavior |
| IDE support | Visual Studio package | 2.0.26 | Unity integration for editing and debugging scripts |
| IDE support | JetBrains Rider package | 3.0.39 | Alternative Unity integrated C# development support |
| Version control | Git | Present through `.git` folder | Tracks project changes and supports team development |
| Package management | Unity Package Manager | Present | Manages Unity packages through `Packages/manifest.json` |
| Testing package | Unity Test Framework | 1.6.0 | Provides base support for Unity edit mode and play mode tests |
| Collaboration package | Unity Version Control package | 2.12.4 | Package support for Unity collaboration workflows |

### Unity Packages Used

| Package Name | Version | Role in Project |
|---|---:|---|
| `com.unity.ai.navigation` | 2.0.12 | Provides Unity NavMesh tools and runtime navigation support for future or scene based AI pathfinding |
| `com.unity.inputsystem` | 1.19.0 | Provides the new Unity Input System used by VR controls and action references |
| `com.unity.render-pipelines.universal` | 17.3.0 | Provides URP rendering support for optimized real time rendering |
| `com.unity.xr.management` | 4.5.4 | Provides XR loader and platform management |
| `com.unity.xr.oculus` | 4.5.4 | Provides Oculus and Meta Quest runtime support |
| `com.unity.xr.openxr` | 1.16.1 | Provides standards based XR runtime support |
| `com.unity.ugui` | 2.0.0 | Provides Unity UI support for Canvas based interfaces |
| `com.unity.visualscripting` | 1.9.11 | Provides visual scripting package support |
| `com.unity.timeline` | 1.8.12 | Provides timeline sequencing support |
| `com.unity.multiplayer.center` | 1.0.1 | Provides Unity multiplayer tooling entry points |
| `com.unity.multiplayer.playmode` | 2.0.2 | Supports multiplayer play mode testing workflows |

### Imported Project Frameworks and Assets

| Asset or Framework | Location | Purpose |
|---|---|---|
| BNG Framework | `Assets/BNG Framework` | Main VR interaction framework used for player control, grabbing, locomotion, hand animation, UI, weapons, physics interaction, and PUN integration |
| Photon Unity Networking | `Assets/Photon` | Multiplayer networking framework used for rooms, PhotonViews, RPCs, synchronization, and ownership |
| BNG PUN integration | `Assets/BNG Framework/Integrations/PUN` | Project specific bridge between BNG interaction scripts and Photon networking |
| PolygonDungeon | `Assets/PolygonDungeon` | Dungeon themed 3D environment assets and demo scenes |
| XR assets | `Assets/XR` | XR configuration and device related project assets |
| Unity scenes | `Assets/Scenes`, BNG scenes, Photon scenes, PolygonDungeon scenes | Scene files used for testing, framework demos, and project setup |

### Core Technical Stack

| Layer | Technology | Description |
|---|---|---|
| Engine layer | Unity | Manages GameObject lifecycle, scenes, physics, rendering, input, and components |
| XR layer | XR Management, OpenXR, Oculus XR | Enables VR headset and controller support |
| Interaction layer | BNG Framework | Provides VR player, hands, grabbing, locomotion, haptics, weapons, and UI interaction |
| Networking layer | Photon PUN 2 | Provides room connection, remote player spawning, PhotonView synchronization, RPCs, and ownership transfer |
| Rendering layer | URP | Provides render pipeline support for real time VR graphics |
| Environment layer | PolygonDungeon | Provides dungeon assets used for dungeon visual construction |
| Gameplay layer | C# components | Provides combat, damage, projectiles, bow mechanics, object interaction, and helper systems |

## 3.2 System Architecture

The project architecture follows Unity's component based design. Each system is implemented as a collection of MonoBehaviour scripts attached to GameObjects. Instead of building one large centralized script, the project is divided into separate modules such as player movement, input, hand animation, object grabbing, UI interaction, weapons, networking, and environmental support.

At a high level, the system is organized into six major layers.

| Architecture Layer | Main Responsibility | Important Scripts or Assets |
|---|---|---|
| XR input layer | Reads headset and controller inputs from the selected XR provider | `InputBridge`, Unity Input System action references, XR packages |
| VR player layer | Handles player height, camera rig movement, locomotion, climbing, and collision | `BNGPlayerController`, `SmoothLocomotion`, `PlayerTeleport`, `PlayerClimbing`, `PlayerGravity` |
| Hand and interaction layer | Handles hand animation, grabbing, releasing, remote grab, snap zones, and object manipulation | `HandController`, `Grabber`, `Grabbable`, `GrabPoint`, `SnapZone`, `GrabbablesInTrigger` |
| Combat layer | Handles damage, raycast weapons, projectiles, bows, arrows, and collision based damage | `Damageable`, `DamageCollider`, `RaycastWeapon`, `Projectile`, `ProjectileLauncher`, `Bow`, `Arrow` |
| Networking layer | Handles Photon connection, room joining, player replication, object synchronization, and ownership transfer | `NetworkManager`, `NetworkPlayer`, `NetworkedGrabbable`, `NetworkedBow`, `NetworkedArrow`, `NetworkedRaycastWeapon` |
| Presentation layer | Handles URP rendering, world space UI, pointer interaction, and VR canvas support | URP package, `VRUISystem`, `UIPointer`, `VRCanvas`, `ScreenFader` |

### Main Folder Structure

| Folder | Contents | Project Use |
|---|---|---|
| `Assets/BNG Framework/Scripts/Core` | Core VR systems | Player controller, grabbing base, locomotion, teleportation, interactable controls |
| `Assets/BNG Framework/Scripts/Components` | Reusable components | Damage, collision, constraints, haptics, return to snap zone, velocity tracking |
| `Assets/BNG Framework/Scripts/Helpers` | Helper systems | Hand physics, controller offsets, UI trigger, teleport helpers, joint helpers |
| `Assets/BNG Framework/Scripts/UI` | VR UI systems | VR canvas, pointer, keyboard, text input, UI event system |
| `Assets/BNG Framework/Scripts/Weapons` | Weapon systems | Raycast weapon, magazine, projectile, ammo display, bullets |
| `Assets/BNG Framework/Scripts/Extras` | Extra gameplay mechanics | Bow, arrow, flashlight, moving platform, marker, vehicle, grapple shot, projectile launcher |
| `Assets/BNG Framework/Integrations/PUN` | Multiplayer integration | Photon based player, grabbable, bow, arrow, marker, and weapon networking |
| `Assets/Photon` | Photon source and demos | Photon Realtime, Photon Chat, Photon PUN, utility scripts, demo scenes |
| `Assets/PolygonDungeon` | Dungeon assets | Dungeon models, materials, prefabs, scenes |
| `Assets/Scenes` | Project scene folder | Contains `SampleScene.unity` |

### Data Flow in the Project

The runtime data flow of the implemented system is as follows:

1. The XR runtime provides headset and controller data.
2. `InputBridge` converts provider specific input into unified controller values.
3. `BNGPlayerController` updates the player body, camera rig, capsule height, and collision response.
4. `HandController` reads grip, trigger, thumb, and point values to animate hands.
5. `Grabber` checks nearby `Grabbable` objects and performs grab or release behavior.
6. Combat components such as `DamageCollider`, `RaycastWeapon`, `Projectile`, `Bow`, and `Arrow` apply gameplay effects.
7. Photon components synchronize player movement, hand animation, weapon visuals, arrow flight, and networked object state.
8. UI components such as `VRUISystem`, `UIPointer`, and `VRCanvas` allow controller based interaction with world space UI.

### Architecture Outcome

| Completed Work | Outcome |
|---|---|
| Modular VR architecture established | The project is organized into independent systems that can be tested and extended separately |
| Imported framework source integrated | BNG Framework and Photon source are present and available inside the Unity project |
| Multiplayer interaction bridge present | BNG interaction scripts are connected with Photon through PUN integration scripts |
| Dungeon asset base present | PolygonDungeon assets provide the visual foundation for a dungeon environment |
| XR rendering and input packages configured | OpenXR, Oculus XR, XR Management, Input System, and URP are present in the package manifest |

## 3.3 VR Player System

The VR player system forms the foundation of the project. It is responsible for making the player feel physically present inside the virtual dungeon. The player system handles the head mounted display, left hand, right hand, controller input, height adjustment, movement, collision, teleportation, smooth locomotion, climbing, and interaction with physical objects.

The central player controller script is `BNGPlayerController`. This script manages how the player's physical headset position is reflected in the Unity character body. It supports moving the character with the camera, rotating the character with the camera, resizing the character height based on the camera height, checking distance from ground, and resetting the player if they move outside safe elevation limits.

### Important VR Player Scripts

| Script | Main Function |
|---|---|
| `BNGPlayerController` | Main VR player body controller, camera alignment, height adjustment, collision movement, elevation checks |
| `InputBridge` | Unified input interface for XRInput, Oculus, SteamVR, Pico, Unity Input System, WebXR, and other supported input sources |
| `SmoothLocomotion` | Continuous joystick based movement using CharacterController or Rigidbody |
| `PlayerTeleport` | Parabolic teleport system with valid and invalid teleport detection |
| `PlayerClimbing` | Physical climbing system based on hand movement and climbable objects |
| `PlayerGravity` | Gravity support for player movement |
| `PlayerRotation` | Player turning support |
| `TrackedDevice` | Tracks XR device position and rotation |
| `ControllerModelSelector` | Handles controller model selection |
| `HandModelSelector` | Handles hand model selection |

### Player Body and Camera Handling

`BNGPlayerController` uses the following important transforms:

| Transform | Purpose |
|---|---|
| `TrackingSpace` | Represents the origin of the VR tracking space |
| `CameraRig` | Parent object used to offset the main camera |
| `CenterEyeAnchor` | Represents the headset or main camera position |
| Main camera | Represents the player's view inside VR |

The controller updates the player body by checking the headset position. The character capsule can be resized to match the user's real head height. This is important in VR because users may stand, crouch, or move their head naturally. The script also checks ground distance through raycasts and stores `DistanceFromGround`, which helps locomotion and gravity systems understand whether the player is grounded.

### Player Safety Handling

The player controller also contains safety logic. If the player falls below or goes above configured elevation limits, the controller can reset the player to the initial position. This prevents physics glitches or falling through the level from breaking the experience.

| Safety Feature | Implementation |
|---|---|
| Ground detection | Physics raycast from player body toward ground layers |
| Height correction | CharacterController or Rigidbody capsule height can be updated from camera height |
| Out of bounds reset | Player position is reset if it passes configured minimum or maximum elevation |
| Camera alignment | Tracking space can rotate to match the headset yaw angle |

## 3.3.1 Hand Tracking and Finger Animation

The hand system is handled mainly by `HandController`, `InputBridge`, hand poser scripts, and animator based hand models. The project supports animated VR hands that respond to controller grip, trigger, thumb, and point input values.

The `InputBridge` script exposes controller values such as:

| Input Value | Description |
|---|---|
| `LeftGrip` and `RightGrip` | Analog grip amount for each controller |
| `LeftTrigger` and `RightTrigger` | Analog trigger amount for each controller |
| `LeftGripDown` and `RightGripDown` | Grip pressed this frame |
| `LeftTriggerDown` and `RightTriggerDown` | Trigger pressed this frame |
| `LeftThumbstickAxis` and `RightThumbstickAxis` | Thumbstick movement axes |
| `LeftThumbNear` and `RightThumbNear` | Thumb proximity information |
| `LeftTriggerNear` and `RightTriggerNear` | Trigger touch proximity information |

The `HandController` script converts these input values into animation values.

| Hand Animation Value | Meaning |
|---|---|
| `GripAmount` | Controls how closed or open the hand is |
| `PointAmount` | Controls index finger pointing pose |
| `ThumbAmount` | Controls thumb pose |
| `PoseId` | Selects a predefined hand pose |

The hand system supports different pose sources.

| Pose Method | Description |
|---|---|
| Animator pose | Uses Unity Animator layers and parameters to animate fingers |
| HandPoser pose | Uses predefined hand pose data for a specific grabbed object |
| AutoPoser pose | Attempts to automatically fit the hand around an object |
| Hand pose override | Allows a fixed hand pose to override normal behavior |

When a player grabs an object, `HandController` checks the held `Grabbable` object and changes the hand pose according to that object's configuration. For example, a sword, bow, gun, or generic object can use different pose settings. The system supports selected hand poses, animator IDs, and auto posing options.

### Hand Animation Outcome

| Completed Work | Outcome |
|---|---|
| Grip based animation support | Hands can visually close and open based on controller input |
| Point and thumb animation support | Hands can represent pointing and thumb movement |
| Object specific pose support | Grabbed objects can define how the hand should look while held |
| Network hand animation support | `NetworkPlayer` sends grip, point, thumb, pose, and holding state to remote players |

## 3.3.2 Locomotion System

The project contains multiple locomotion systems so that users can move through the VR environment in different ways. This is important because VR users have different comfort levels. Some users prefer smooth movement, while others prefer teleportation to reduce motion sickness.

### Implemented Locomotion Methods

| Locomotion Method | Script | Description |
|---|---|---|
| Smooth movement | `SmoothLocomotion` | Uses thumbstick input for continuous movement |
| Teleportation | `PlayerTeleport` | Uses parabolic arc prediction and valid teleport surface checks |
| Climbing | `PlayerClimbing` | Moves the player based on hand movement while gripping climbable objects |
| Rotation | `PlayerRotation` | Supports player turning behavior |
| Gravity | `PlayerGravity` | Applies gravity behavior to the player |

### Smooth Locomotion

`SmoothLocomotion` supports both CharacterController based and Rigidbody based movement. It reads movement axes through `InputBridge` or Input System action references. The movement system supports walking, strafing, sprinting, jumping, air control, movement drag, static drag, and slope checks.

Important smooth locomotion parameters include:

| Parameter | Purpose |
|---|---|
| `MovementSpeed` | Controls forward and backward movement speed |
| `StrafeSpeed` | Controls side movement speed |
| `SprintSpeed` | Controls faster forward movement |
| `JumpForce` | Controls jump force |
| `AirControl` | Allows movement input while not grounded |
| `ControllerType` | Selects CharacterController or Rigidbody movement mode |

### Teleportation

`PlayerTeleport` implements a parabolic teleport arc. The teleport line is drawn using a LineRenderer. The system checks collision layers and valid teleport layers, shows different colors for valid and invalid teleport locations, and can rotate the teleport marker before movement.

Important teleportation features include:

| Feature | Description |
|---|---|
| Parabolic simulation | Teleport arc is calculated using segment count, velocity, and scale |
| Valid surface checking | The destination must be on configured valid layers |
| Invalid surface rejection | The teleport marker turns invalid when the target surface is not allowed |
| Slope checking | Teleport destination can be rejected if slope is too steep |
| Screen fade | Optional screen fade can be used during teleport |
| Direction indicator | Player can choose facing direction before teleporting |

### Locomotion Manager

`LocomotionManager` allows switching between teleportation and smooth locomotion. It can load the selected locomotion mode from PlayerPrefs or use a default mode. This supports both comfort based movement and continuous movement in the same project.

### Climbing

`PlayerClimbing` supports physical climbing. When a hand grabs a `Climbable` object, the script uses the difference between previous and current controller positions to move the player body. This makes climbing feel physical because the user's hand movement directly moves the VR body.

### Locomotion Outcome

| Completed Work | Outcome |
|---|---|
| Smooth locomotion support | Player can move continuously using controller axes |
| Teleportation support | Player can teleport using a valid location arc |
| Climbing support | Player movement can be driven by hand movement on climbable objects |
| Locomotion switching | Teleport and smooth locomotion can be toggled |
| VR comfort support | Project supports both comfort locomotion and continuous movement |

## 3.3.3 Physics Based Object Grabbing

Physics based object grabbing is one of the most important interaction features of the VR project. The grabbing system allows users to reach toward objects, grab them with controller input, hold them, move them, throw them, snap them to zones, and interact with them in a physically meaningful way.

The main scripts involved are `Grabber`, `Grabbable`, `GrabbablesInTrigger`, `GrabPoint`, `SnapZone`, `VelocityTracker`, and optional helper components.

### Main Grabbing Components

| Component | Function |
|---|---|
| `Grabber` | Hand side object that checks grab input and attempts to grab nearby objects |
| `Grabbable` | Base component for any object that can be picked up |
| `GrabbablesInTrigger` | Tracks all grabbable objects currently inside the hand trigger area |
| `GrabPoint` | Defines precise hand placement points on an object |
| `SnapZone` | Defines object placement zones for inventory, sockets, or item holders |
| `VelocityTracker` | Tracks controller velocity for throwing and impact logic |
| `GrabbableEvents` | Provides event callbacks for grab, release, trigger, and controller inputs |

### Grabbable Object Features

The `Grabbable` class contains a large number of settings that make object interaction flexible. Important implemented features include:

| Feature | Description |
|---|---|
| Grab button selection | Objects can use grip, trigger, inherited control, or grip or trigger |
| Hold type | Objects can use hold down or toggle behavior |
| Grab mechanic | Objects can use precise grab positions or general grab behavior |
| Grab physics mode | Objects can use velocity, kinematic, or joint based movement behavior |
| Remote grabbing | Objects can be pulled from a distance if configured |
| Throw force multiplier | Released objects can receive controller velocity for throwing |
| Angular velocity multiplier | Released objects can receive spin from controller movement |
| Two handed behavior | Objects can support secondary hand handling |
| Hand hiding and hand posing | Objects can hide hand graphics or apply object specific poses |
| Snap zone support | Objects can be configured to snap into placement zones |

### Physics Behavior

The grab system uses Rigidbody physics, ConfigurableJoint behavior, controller velocity tracking, and collision modes. When an object is grabbed, the system can move it using velocity, joint constraints, or kinematic positioning. When released, the object can inherit controller velocity and angular velocity, allowing natural throwing behavior.

### Object Interaction Outcome

| Completed Work | Outcome |
|---|---|
| Near object grabbing | Hands can grab objects within trigger range |
| Remote grabbing support | Objects can be pulled from a distance when configured |
| Physics based release | Objects can be thrown using tracked controller velocity |
| Two handed support | Grabbable objects contain configuration for two handed interaction |
| Snap zone support | Objects can be placed into defined snap locations |
| Event driven behavior | Grabbable objects can invoke Unity events during grab and release |

## 3.4 Networking System

The networking system is implemented using Photon Unity Networking. The project includes the Photon framework source code and BNG PUN integration scripts. The network layer supports connection to Photon, room joining, automatic room creation, remote player instantiation, head and hand synchronization, hand animation synchronization, networked grabbables, and networked weapon behavior.

The most important project networking scripts are in `Assets/BNG Framework/Integrations/PUN`.

### Main Networking Scripts

| Script | Purpose |
|---|---|
| `NetworkManager` | Connects to Photon, joins or creates rooms, spawns the remote player representation |
| `NetworkPlayer` | Synchronizes head, hands, grip animation, point animation, thumb animation, pose ID, and holding state |
| `NetworkedGrabbable` | Synchronizes position, rotation, and held state of shared grabbable objects |
| `NetworkedRaycastWeapon` | Sends weapon firing visual events to other clients using RPC |
| `NetworkedBow` | Synchronizes bow draw visuals, arrow knock position, bow model rotation, draw percentage, and string distance |
| `NetworkedArrow` | Synchronizes arrow flight, impact, damage RPCs, position, rotation, and collider state |
| `NetworkedMarker` | Provides network support for marker type objects |

### Photon Concepts Used

| Photon Concept | Use in Project |
|---|---|
| `PhotonNetwork.ConnectUsingSettings()` | Connects the client to Photon using project settings |
| `PhotonNetwork.JoinRoom()` | Attempts to join a named room |
| `PhotonNetwork.CreateRoom()` | Creates a room if joining fails |
| `PhotonNetwork.Instantiate()` | Spawns networked objects such as remote players or arrows |
| `PhotonView` | Identifies networked objects and object ownership |
| `IPunObservable` | Sends and receives synchronized data streams |
| `RPC` | Sends event based messages such as shooting, bow sound, arrow impact, and damage |
| Ownership transfer | Allows control of a networked object to move between clients |

## 3.4.1 Connection and Room Management

Connection and room management is handled by `NetworkManager`. On startup, the script checks whether the client is already connected to Photon. If connected and `JoinRoomOnStart` is enabled, it attempts to join the configured room. If not connected, it calls `PhotonNetwork.ConnectUsingSettings()` and sets the Photon game version.

### Connection Flow

| Step | Action | Script Method |
|---|---|---|
| 1 | Start networking object | `Start()` |
| 2 | If already connected, join room | `PhotonNetwork.JoinRoom(JoinRoomName)` |
| 3 | If not connected, connect to Photon | `PhotonNetwork.ConnectUsingSettings()` |
| 4 | After connection, join room | `OnConnectedToMaster()` |
| 5 | If room does not exist, create room | `OnJoinRoomFailed()` |
| 6 | After room join, spawn remote player object | `OnJoinedRoom()` |

### Room Configuration

| Field | Purpose |
|---|---|
| `maxPlayersPerRoom` | Controls maximum players per room |
| `JoinRoomOnStart` | Automatically joins room when the scene starts |
| `JoinRoomName` | Name of the room to join or create |
| `GameVersion` | Separates clients using different game versions |
| `RemotePlayerObjectName` | Name of the networked player prefab to instantiate |
| `dontDestroyOnLoad` | Keeps network manager alive across scene changes |

### Outcome

| Completed Work | Outcome |
|---|---|
| Photon connection workflow present | Client can connect using Photon settings |
| Room join workflow present | Client attempts to join a configured room |
| Room creation fallback present | If room does not exist, the script creates it |
| Remote player spawning present | On joining room, a remote player representation is instantiated |
| Scene synchronization enabled | `PhotonNetwork.AutomaticallySyncScene` is set to true |

## 3.4.2 Player Synchronization with Interpolation

Player synchronization is handled by `NetworkPlayer`. The script sends the local player's head position, head rotation, left hand position, left hand rotation, right hand position, right hand rotation, and hand animation values through `OnPhotonSerializeView`.

### Synchronized Player Data

| Data Type | Sent by Local Player | Used by Remote Player |
|---|---|---|
| Head position | Yes | Moves remote head model |
| Head rotation | Yes | Rotates remote head model |
| Left hand position | Yes | Moves remote left hand |
| Left hand rotation | Yes | Rotates remote left hand |
| Right hand position | Yes | Moves remote right hand |
| Right hand rotation | Yes | Rotates remote right hand |
| Left grip amount | Yes | Animates remote left hand grip |
| Left point amount | Yes | Animates remote left hand pointing |
| Left thumb amount | Yes | Animates remote left hand thumb |
| Left pose ID | Yes | Sets remote left hand pose |
| Left holding state | Yes | Shows held item hand pose behavior |
| Right grip amount | Yes | Animates remote right hand grip |
| Right point amount | Yes | Animates remote right hand pointing |
| Right thumb amount | Yes | Animates remote right hand thumb |
| Right pose ID | Yes | Sets remote right hand pose |
| Right holding state | Yes | Shows held item hand pose behavior |

### Interpolation Method

Remote motion uses interpolation to reduce visible network jitter. The script stores start and end positions and rotations for the head and hands. It calculates a synchronization value based on elapsed time and sync delay, then uses:

| Operation | Function |
|---|---|
| Position interpolation | `Vector3.Lerp()` |
| Rotation interpolation | `Quaternion.Lerp()` |
| Finger animation smoothing | `Mathf.Lerp()` |

This is important because Photon updates do not arrive every rendered frame. Interpolation allows the remote player's head and hands to move smoothly between received network snapshots.

### Outcome

| Completed Work | Outcome |
|---|---|
| Head and hand transform synchronization | Remote players can see the other player's head and hand motion |
| Hand animation synchronization | Grip, point, thumb, pose ID, and holding state are sent over network |
| Interpolation implemented | Remote transforms move smoothly instead of snapping every update |
| Local object hiding implemented | Local player can hide its own remote representation while showing other players |

## 3.4.3 Object Ownership Transfer

Object ownership transfer is essential for shared VR objects. If two players can interact with the same sword, bow, cube, or arrow, only one client should control the object at a time. The project handles this using PhotonView ownership.

### Networked Grabbable Ownership

`NetworkedGrabbable` extends `Grabbable` and implements `IPunObservable`. It synchronizes position, rotation, and held state for shared objects. If the object is not owned locally, the object is made kinematic and its transform is interpolated toward received network positions.

Important behavior includes:

| Behavior | Description |
|---|---|
| Owner writes transform | The owning client sends position, rotation, and held state |
| Remote clients read transform | Non owning clients receive and interpolate object position |
| Distance based correction | If object is too far from synced position, it teleports to correct location |
| Held state synchronization | `BeingHeld` is synchronized across clients |
| Null owner handling | Master client can transfer ownership to itself if owner is missing |

### Ownership Request Flow

`NetworkPlayer` checks nearby grabbables from the local left and right `GrabbablesInTrigger` components. If a player tries to grab a nearby networked object not owned by them, the script can request ownership.

| Step | Description |
|---|---|
| 1 | Local grabber detects nearby grabbable |
| 2 | Script checks whether the object has a PhotonView |
| 3 | If object is not locally owned, ownership is requested |
| 4 | Owner transfer RPC moves ownership to requesting player |
| 5 | New owner controls the object and sends transform updates |

### Networked Arrow Ownership

`NetworkedArrow` also requests ownership before shooting if the arrow is not owned by the local client. It uses Photon RPCs to broadcast shooting, impact, and damage events. This is important because arrows are fast moving projectiles and require event synchronization in addition to transform synchronization.

### Outcome

| Completed Work | Outcome |
|---|---|
| Networked object synchronization | Shared objects can synchronize position, rotation, and held state |
| Ownership transfer support | Control of networked objects can move between players |
| Networked arrow control | Arrow ownership, shooting, impact, and destruction are network aware |
| Networked bow visual sync | Bow draw state and string visuals are synchronized |

## 3.5 Combat System

The combat system in the current project is implemented through reusable damage, collision, projectile, raycast, bow, and weapon components. The current codebase supports several forms of combat behavior, including collision based damage, raycast weapons, projectile weapons, and bow and arrow mechanics.

The combat system is component based. An object can receive damage if it has a `Damageable` component. A weapon or projectile can apply damage if it calls `DealDamage()` on that component.

### Main Combat Components

| Component | Purpose |
|---|---|
| `Damageable` | Stores health, receives damage, handles destruction and respawn behavior |
| `DamageCollider` | Applies damage when collision force passes a minimum threshold |
| `RaycastWeapon` | Implements firearm style weapons using raycasts or projectiles |
| `Projectile` | Implements collision based projectile damage and impact effects |
| `ProjectileLauncher` | Spawns and launches projectile objects from a muzzle transform |
| `Bow` | Implements local bow draw, arrow grabbing, haptics, and release |
| `Arrow` | Implements arrow flight, impact damage, and sticking behavior |
| `NetworkedRaycastWeapon` | Sends weapon shooting visuals across Photon |
| `NetworkedBow` | Provides networked bow and arrow draw synchronization |
| `NetworkedArrow` | Provides networked arrow flight, impact, and damage synchronization |

### Damageable System

`Damageable` is the health and destruction component. It stores health, subtracts damage, invokes damage events, and handles destruction behavior when health reaches zero.

| Feature | Description |
|---|---|
| Health value | Object starts with a configured health value |
| Damage event | `onDamaged` event can be invoked with damage amount |
| Death event | `onDestroyed` event can be invoked when health reaches zero |
| Spawn on death | Optional object can spawn at death position |
| Activate on death | Optional GameObjects can be activated |
| Deactivate on death | Optional GameObjects and Colliders can be disabled |
| Destroy delay | Object can be destroyed after a configured delay |
| Respawn support | Object can be restored after a delay if respawn is enabled |
| Drop on death | If the object is grabbable and held, it can be dropped on death |

## 3.5.1 Melee Weapons and Damage Colliders

Melee style damage is supported through `DamageCollider`. This component applies damage when it collides with an object that has `Damageable`. It uses collision impulse and relative velocity information to avoid applying damage on very weak touches.

### DamageCollider Logic

| Step | Description |
|---|---|
| 1 | Collision occurs through Unity physics |
| 2 | Script records collision impulse magnitude |
| 3 | Script records relative velocity magnitude |
| 4 | If force is greater than `MinForce`, it checks for `Damageable` |
| 5 | If target has `Damageable`, damage is applied |
| 6 | If configured, the object itself can take collision damage |

### Important Parameters

| Parameter | Purpose |
|---|---|
| `Damage` | Damage amount applied to target |
| `ColliderRigidbody` | Rigidbody used to evaluate movement and collision |
| `MinForce` | Minimum force required before damage is applied |
| `LastRelativeVelocity` | Stores previous collision relative velocity |
| `LastDamageForce` | Stores last collision impulse magnitude |
| `TakeCollisionDamage` | Allows object to damage itself from collisions |
| `CollisionDamage` | Damage received by this object when self collision damage is enabled |

### Outcome

| Completed Work | Outcome |
|---|---|
| Collision based damage present | Physical impacts can damage `Damageable` objects |
| Force threshold present | Weak touches can be ignored |
| Rigidbody based combat support | Physical weapons can use Unity collision and impulse |
| Reusable damage receiver present | The same `Damageable` component works for enemies, objects, and props |

## 3.5.2 Ranged Weapons (Raycast and Projectile)

The current project includes both raycast based and projectile based ranged combat support.

### Raycast Weapon

`RaycastWeapon` implements firearm style behavior. It can use raycast hit detection or spawn projectiles depending on weapon configuration and time scale behavior.

Important features include:

| Feature | Description |
|---|---|
| Maximum range | Controls how far the weapon can hit |
| Damage value | Controls damage applied to `Damageable` targets |
| Firing method | Supports semi automatic and automatic firing logic |
| Reload method | Supports infinite ammo and other reload behavior options |
| Firing rate | Controls delay between shots |
| Recoil force | Supports recoil behavior through Rigidbody force |
| Bullet impact force | Applies force to hit Rigidbody objects |
| Muzzle flash | Optional muzzle flash object can be activated |
| Bullet casing | Optional casing object can be ejected |
| Hit effects | Optional hit effect prefab can be spawned |
| Gunshot sound | Spatial audio can play when firing |
| Empty sound | Sound can play when firing without ammo |

### Projectile

`Projectile` applies damage when it collides with another object. It can spawn hit effects, apply force to hit rigidbodies, and destroy itself after impact. The projectile can also be converted into a raycast projectile if required by time scale behavior.

### Projectile Launcher

`ProjectileLauncher` spawns a configured projectile from a muzzle transform and applies forward force. This provides a reusable launcher for bombs, magic projectiles, arrows, or other ranged objects.

### Networked Ranged Weapon Support

`NetworkedRaycastWeapon` extends `RaycastWeapon`. When a local player fires, it sends an RPC to other clients so they can play the remote weapon sound and firing animation. This synchronizes the visual and audio feedback of shooting across the network.

### Outcome

| Completed Work | Outcome |
|---|---|
| Raycast weapon logic present | Firearm style hit detection is available |
| Projectile damage logic present | Physics based projectile impacts can deal damage |
| Projectile launcher present | Reusable projectile spawning and launching exists |
| Network firing feedback present | Remote players can receive firing visual and audio RPCs |

## 3.5.3 Bow and Arrow Mechanics

The bow and arrow system is one of the more advanced interaction features in the project. It uses VR hand position to simulate drawing a bow string, grabbing an arrow, aligning the arrow, calculating draw percentage, applying haptics, and shooting the arrow with force based on draw distance.

### Local Bow System

The `Bow` script handles the local non networked bow interaction. Its important responsibilities include:

| Responsibility | Description |
|---|---|
| Arrow grabbing | Player can grab an arrow from the knock area if configured |
| Bow string movement | Arrow knock follows the pulling hand within a maximum string distance |
| Draw percentage calculation | Draw percent is calculated from knock distance and max string distance |
| Haptic feedback | Haptic pulses are applied at different draw percentages |
| Draw sound | Bow draw sound can play after draw threshold |
| Bow alignment | Bow model can align toward the arrow hand |
| Arrow release | Arrow is released when grab input drops below threshold |
| Shot force | Arrow receives force based on draw amount and `BowForce` |

### Arrow System

The `Arrow` script handles arrow physics after release. It enables gravity, applies shot force, aligns the arrow with its velocity during flight, checks collision, applies damage to `Damageable`, and attempts to stick into objects.

### Networked Bow System

The `NetworkedBow` script adds Photon support. It synchronizes bow visual state using `OnPhotonSerializeView`, including:

| Synchronized Value | Purpose |
|---|---|
| Arrow knock local position | Shows bow string pull position remotely |
| Bow model local rotation | Shows bow orientation remotely |
| Draw percent | Shows draw amount |
| String distance | Shows how far the string is pulled |

It also uses RPCs to play bow draw and release sounds on remote clients.

### Networked Arrow System

The `NetworkedArrow` script synchronizes fast moving arrows. It uses RPCs for shooting, damage, and impact behavior, and uses `OnPhotonSerializeView` for position, rotation, flying state, and collider state.

### Bow and Arrow Outcome

| Completed Work | Outcome |
|---|---|
| VR bow draw mechanics | Bow string behavior responds to hand position |
| Arrow physics | Arrows fly using Rigidbody force and align with velocity |
| Collision damage | Arrows can damage `Damageable` objects |
| Arrow sticking | Arrows can attach to hit objects when conditions are valid |
| Haptic draw feedback | Bow draw can provide controller haptics |
| Networked bow state | Bow visual state can be synchronized |
| Networked arrow state | Arrow flight, impact, and damage can be synchronized |

## 3.6 Enemy AI System

The current repository contains the technical foundation required for enemy interaction, damage, movement, and future AI integration. However, it does not contain a completed custom enemy AI controller using `NavMeshAgent`. The Unity AI Navigation package is installed, which means the project has package level support for NavMesh based pathfinding. However, no current C# script in the repository imports `UnityEngine.AI` or uses `NavMeshAgent`.

Therefore, this section reports only the completed AI related groundwork and supporting systems. A full dungeon enemy controller with NavMesh movement, target detection, and combat state transitions is not completed in the current codebase.

### AI Related Work Present in the Project

| Work Item | Status | Evidence |
|---|---|---|
| Unity AI Navigation package | Completed package setup | `com.unity.ai.navigation` 2.0.12 is present in `Packages/manifest.json` |
| Damage receiving system | Completed | `Damageable` can be attached to enemies or props |
| Collision damage system | Completed | `DamageCollider` can damage `Damageable` targets |
| Projectile damage system | Completed | `Projectile`, `Arrow`, and `NetworkedArrow` can damage `Damageable` targets |
| Waypoint movement support | Present | `MoveToWaypoint` and `Waypoint` scripts exist |
| Enemy NavMesh controller | Not completed in current codebase | No `NavMeshAgent` or `UnityEngine.AI` usage found |
| Enemy state machine | Not completed in current codebase | No dedicated idle, patrol, chase, attack state machine script found |

### Existing Movement Foundation

Although the final enemy AI system is not implemented, the project includes `MoveToWaypoint` and `Waypoint`. These scripts provide simple waypoint based movement for objects. `MoveToWaypoint` moves a Rigidbody toward a destination waypoint, then continues to the next waypoint if assigned.

| Script | Current Use |
|---|---|
| `MoveToWaypoint` | Moves an object toward a waypoint using Rigidbody movement |
| `Waypoint` | Stores a destination waypoint and draws debug gizmos |

This movement foundation can support moving platforms, traps, simple patrol objects, or prototype enemy movement. It is not the same as NavMesh pathfinding, because it does not calculate paths around obstacles.

## 3.6.1 NavMesh Based Pathfinding

The current project includes Unity AI Navigation as a package dependency, but no custom NavMesh enemy script has been implemented in the repository. Therefore, the completed work in this section is package integration and readiness, not full AI behavior.

### Current NavMesh Status

| Item | Status |
|---|---|
| AI Navigation package installed | Yes |
| NavMesh runtime package available | Yes |
| Custom script using `NavMeshAgent` | No |
| Enemy pathfinding script | No |
| Scene level NavMesh baking evidence in code | Not visible from C# source |

### What Has Been Completed

| Completed Work | Explanation |
|---|---|
| AI package added | The Unity project has `com.unity.ai.navigation` in package dependencies |
| Damage compatible targets available | Enemy GameObjects can use `Damageable` to receive damage |
| Combat systems can affect enemies | Melee, projectile, raycast, and arrow systems can damage any object with `Damageable` |
| Waypoint movement exists | Simple point to point movement can be used for prototype movement |

### Current Limitation

A final NavMesh based enemy pathfinding implementation is not present in the current code. The report should not claim that enemies already patrol, chase, or attack using NavMesh unless such behavior is later implemented in the Unity project.

## 3.6.2 State Machine (Idle, Patrol, Chase, Attack)

The heading is included because it is part of the required report structure, but the actual idle, patrol, chase, attack enemy state machine is not completed in the current codebase. No dedicated script defining an enemy state enum or transitions between idle, patrol, chase, and attack was found in the current project source.

### Current State Machine Status

| State | Current Code Status |
|---|---|
| Idle | No dedicated enemy idle state script found |
| Patrol | No dedicated enemy patrol state script found |
| Chase | No dedicated enemy chase state script found |
| Attack | No dedicated enemy attack state script found |

### Supporting Systems Already Available

Although the full state machine is not complete, the project already contains systems that would support it:

| Supporting System | How It Supports Future Enemy AI |
|---|---|
| `Damageable` | Enemies can receive damage and die |
| `DamageCollider` | Enemies or player weapons can apply collision damage |
| `Projectile` | Projectiles can damage enemies |
| `RaycastWeapon` | Raycast weapons can damage enemies |
| `Arrow` and `NetworkedArrow` | Arrows can damage enemies |
| `MoveToWaypoint` | Can provide simple patrol like movement |
| AI Navigation package | Can support future NavMesh based movement |

### Report Accuracy Note

In the final printed report, this section should be written carefully. It is accurate to say that the project contains AI groundwork and Unity AI Navigation package support. It is not accurate to say that a complete enemy AI state machine has already been coded unless that script is added later.

## 3.7 Rendering and Performance Optimization

Rendering and performance optimization are critical in VR because low frame rate, unstable frame pacing, and high latency can cause user discomfort. The current project includes URP and XR packages, which provide the base for optimized VR rendering. It also includes several framework level performance related components and project structures that support optimization.

The current repository does not include measured frame rate results or profiler captures. Therefore, this section reports the implemented rendering foundation and optimization ready systems rather than final benchmark numbers.

### Rendering Foundation Present

| Item | Status | Purpose |
|---|---|---|
| Universal Render Pipeline | Present | Provides optimized scriptable rendering pipeline |
| XR Management | Present | Manages XR loaders and runtime selection |
| Oculus XR package | Present | Supports Meta Quest and Oculus runtime integration |
| OpenXR package | Present | Supports standard XR runtime integration |
| Unity UI package | Present | Supports Canvas based UI |
| BNG VR UI system | Present | Supports world space UI interaction in VR |
| Screen fade system | Present | Supports comfortable transitions |

### Performance Related Scripts and Assets

| Component | Purpose |
|---|---|
| `StaticBatch` | Supports static batching workflow for scene objects |
| `FixNonUniformScale` | Helps correct transform scale issues that can affect physics and rendering |
| `ScaleMaterialHelper` | Helps manage material scale behavior |
| `ScreenFader` | Supports comfort and transition masking |
| `DestroyObjectWithDelay` | Removes temporary objects after delay |
| `BulletHole` | Handles bullet hole attachment and cleanup behavior |
| `Damageable.RemoveBulletHolesOnDeath` | Removes child decals when an object is destroyed |
| Photon culling utility scripts | Photon includes utility scripts related to culling support |

## 3.7.1 Universal Render Pipeline (URP) and Stereo Rendering

The project includes Universal Render Pipeline version 17.3.0. URP is suitable for VR because it is designed for real time performance and scalable rendering. It supports mobile and desktop rendering targets, which is relevant because the project targets VR devices such as Meta Quest and PC VR.

### URP Role in the Project

| URP Function | Relevance to VR Project |
|---|---|
| Scriptable rendering | Allows modern Unity rendering configuration |
| Performance oriented design | Helps target real time frame rates |
| Mobile and desktop scalability | Useful for Meta Quest and PC VR targets |
| Compatibility with XR | Works with XR rendering paths through Unity's XR systems |
| Material and lighting pipeline | Supports consistent visual rendering for dungeon assets |

### Stereo Rendering Context

In VR, the scene must be rendered from the perspective of both eyes. Unity's XR system handles stereo rendering through the configured XR loader and rendering pipeline. The project includes the required package layer for this:

| Package | Stereo Rendering Relevance |
|---|---|
| XR Management | Selects and manages XR runtime loaders |
| Oculus XR | Supports stereo rendering for Oculus and Meta Quest runtime |
| OpenXR | Supports stereo rendering through OpenXR compatible runtimes |
| URP | Provides render pipeline used while XR rendering is active |

### VR UI Rendering

The project includes `VRUISystem`, `UIPointer`, and `VRCanvas`. These scripts allow UI canvases to be interacted with in world space using VR controller rays. `VRUISystem` creates or uses an event system, assigns cameras to canvases, performs raycasts, processes hover, press, release, drag, and scroll events, and supports controller input or Input System action references.

### Outcome

| Completed Work | Outcome |
|---|---|
| URP dependency present | Project is prepared for URP based rendering |
| XR rendering packages present | Project is prepared for VR headset stereo output |
| VR world space UI present | UI interaction works through VR pointer and raycast logic |
| Screen fade support present | Comfortable transitions can be used during teleport or scene loading |

## 3.7.2 Optimization Techniques

The project includes several optimization friendly systems. Some are implemented as scripts, while others are available through Unity packages and framework patterns.

### Implemented or Available Optimization Techniques

| Technique | Current Project Support | Description |
|---|---|---|
| Modular components | Present | Each system runs only the logic needed by its component |
| Rigidbody and collider based interaction | Present | Uses Unity physics systems directly instead of custom collision loops |
| Interpolation for network objects | Present | Reduces visual jitter from network updates |
| Kinematic remote network objects | Present | Non owned network objects are made kinematic to prevent conflicting physics simulation |
| Distance correction for networked objects | Present | Far away networked objects snap to correct positions instead of slowly drifting |
| Bullet hole cleanup | Present | Decals can be removed when damaged objects are destroyed |
| Destroy delay components | Present | Temporary objects can be cleaned up after use |
| Static batching helper | Present | Supports batching static scene objects |
| World space UI raycast system | Present | Uses event based UI interaction instead of unnecessary menu scene changes |
| Photon culling utilities | Present in Photon utility scripts | Provides networking culling utilities in the imported Photon code |

### Network Optimization

The networking code uses a snapshot synchronization approach. Instead of sending every internal object detail every frame, networked scripts send specific values needed by remote clients.

| Networked Object | Synchronized Data |
|---|---|
| Remote player | Head transform, hand transforms, finger values, pose IDs, holding states |
| Networked grabbable | Position, rotation, held state |
| Networked bow | Arrow knock position, bow rotation, draw percentage, string distance |
| Networked arrow | Position, rotation, flying state, collider state |
| Networked raycast weapon | Firing visual and audio events through RPC |

This reduces bandwidth compared to sending full object state for every component.

### Physics Optimization

For remote networked objects, `NetworkedGrabbable` makes the Rigidbody kinematic when the object is not locally owned. This prevents multiple clients from simulating the same object at the same time. Only the owner simulates and transmits the result.

### VR Comfort Optimization

| Comfort Feature | Implementation |
|---|---|
| Teleport locomotion | Reduces motion sickness for sensitive users |
| Smooth locomotion option | Allows continuous movement for users comfortable with it |
| Screen fade | Used during teleport or scene transitions |
| Snap or controlled turning support | Player rotation scripts are available |
| Physical climbing | Uses hand movement, which can feel more natural in VR |

### Current Performance Limitation

No final performance benchmark table is present in the project folder. The report should not include exact FPS, CPU frame time, GPU frame time, or build performance numbers unless those are measured later using Unity Profiler, headset performance tools, or a built build.

## Chapter Summary

| System | Current Status | Summary |
|---|---|---|
| Tools and technology setup | Completed | Unity, C#, URP, XR packages, AI Navigation, Input System, Photon, and BNG Framework are present |
| System architecture | Completed foundation | Project is organized into VR, interaction, combat, networking, rendering, and environment layers |
| VR player system | Completed foundation | Player body, head tracking, height adjustment, smooth locomotion, teleportation, climbing, and input abstraction exist |
| Hand tracking and animation | Completed foundation | Grip, point, thumb, hand poses, and held object hand states are implemented |
| Physics based grabbing | Completed foundation | Grabbable objects, grabbers, physics release, two handed options, remote grab, and snap zones exist |
| Networking system | Completed foundation | Photon connection, room management, remote player sync, object sync, ownership transfer, bow and arrow network support exist |
| Combat system | Completed foundation | Damageable objects, collision damage, raycast weapons, projectiles, bow, arrow, and networked weapon support exist |
| Enemy AI system | Partially prepared | AI Navigation package and damage support exist, but no custom NavMesh enemy controller or state machine exists |
| Rendering and optimization | Completed foundation | URP, XR packages, VR UI, interpolation, cleanup systems, and optimization helper scripts exist |

## Technical Terminology Used in This Chapter

| Term | Meaning in This Project |
|---|---|
| GameObject | The basic object type in Unity scenes |
| Component | A script or built in Unity behavior attached to a GameObject |
| MonoBehaviour | Base class for most Unity scripts that run in scenes |
| Transform | Stores position, rotation, and scale of a GameObject |
| Rigidbody | Unity physics component used for force, velocity, and collision behavior |
| Collider | Defines the physical shape used for collision detection |
| CharacterController | Unity movement component often used for player capsules |
| Raycast | A line query used to detect hits against objects or UI |
| LineRenderer | Unity component used to draw lines such as teleport arcs or pointers |
| HMD | Head mounted display used for VR viewing |
| XR | Extended reality, covering VR, AR, and MR |
| OpenXR | Open standard API for XR runtimes |
| Oculus XR | Unity package for Oculus and Meta Quest device support |
| URP | Universal Render Pipeline used for optimized Unity rendering |
| PUN | Photon Unity Networking |
| PhotonView | Photon component that identifies networked objects |
| RPC | Remote Procedure Call used to run a method on remote clients |
| IPunObservable | Photon interface used for stream based synchronization |
| Interpolation | Smooth movement between two received network states |
| Ownership Transfer | Process of changing which client controls a networked object |
| Grabbable | Object that can be picked up by the VR player |
| Grabber | Hand side component that grabs nearby Grabbable objects |
| SnapZone | A placement zone where compatible objects can snap into position |
| Damageable | Component that stores health and receives damage |
| Projectile | Moving object that can deal damage on collision |
| NavMesh | Navigation mesh used by AI agents for pathfinding |
| NavMeshAgent | Unity AI component that moves characters along a NavMesh |
| State Machine | Logic structure that switches behavior between states such as idle, patrol, chase, and attack |
| Stereo Rendering | Rendering the scene separately for left and right eyes in VR |
| Kinematic Rigidbody | Rigidbody controlled by script instead of normal physics forces |
| Haptics | Controller vibration feedback used to improve immersion |
| Prefab | Reusable Unity object template |
| Scene | Unity level or environment file |
| PlayerPrefs | Unity storage system for saving simple user settings |
| Culling | Technique for reducing rendering or network work for objects that do not need updates |
| Batching | Technique for combining rendering work to reduce draw call cost |


