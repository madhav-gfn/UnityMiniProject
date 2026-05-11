# 4. RESULTS AND DISCUSSIONS

This chapter presents the results obtained from the current project implementation and discusses the observations made during source-code inspection, project structure review, and local Git branch review. The results reported here are limited to what could be verified from the Unity project files, C# scripts, Unity package configuration, scenes, prefabs, and branch history available in the local repository.

No claim is made for runtime frame rate, final headset performance, user study data, or completed gameplay balance unless direct evidence exists in the project files. The project was inspected in the workspace through source files and Git history. A live Unity Editor play mode test, headset build test, or profiler benchmark was not executed from this environment.

## Verification Basis

| Verification Item | Method Used | Result Type |
|---|---|---|
| Current project structure | Local file inspection | Verified |
| Unity version | `ProjectSettings/ProjectVersion.txt` | Verified |
| Unity package dependencies | `Packages/manifest.json` | Verified |
| C# implementation | Script inspection under `Assets` | Verified |
| Scene availability | Unity scene file listing under `Assets` | Verified |
| Current branch status | Git branch inspection using safe-directory option | Verified |
| Other branch changes | Git log, branch list, file differences, and selected file inspection | Verified |
| Runtime gameplay testing | Not executed in this environment | Not measured |
| VR headset testing | Not executed in this environment | Not measured |
| FPS and profiler metrics | Not available in project files | Not measured |

## Branch Review Summary

The local Git repository contains multiple branches that represent different stages of project development. These branches were inspected without checking them out, so the current working tree was not changed. The results from other branches are included only where files or commits could be verified.

| Branch | Verified Commit or Status | Relevant Result Observed |
|---|---|---|
| `non_non_URP` | Current branch | Contains documentation work and current integrated Unity project files |
| `origin/non_non_URP` | Same as current branch | Documentation branch with BNG, Photon, PolygonDungeon, and report files |
| `origin/non_URP` | Commit message: multiplayer demo worthy | Contains networked bow and arrow support, networked grabbable changes, dungeon map prefab, networked marker, and networked pistol prefab |
| `origin/Multiplayer_native` | Commit message: assets add | Contains networked gesture spell casting, spell prefabs, VFX assets, and `NetworkedSpellCaster` |
| `origin/main` | Commit message: Back to Built In RP | Contains enemy AI scripts, player health script, scene loader script, enemy animation assets, enemy prefab assets, NavMesh surface assets, and main menu scene |

The branch review shows that the project has multiple implemented feature tracks. The current branch contains the main VR interaction and networking foundation. Other branches contain additional work for gesture spells and enemy AI. Since these branch features are not necessarily merged into the current branch, they are reported as branch verified results rather than current branch runtime results.

## 4.1 VR Interaction Testing Results

The current project contains a complete VR interaction foundation through BNG Framework. The available scripts show that the player can be represented through a VR camera rig, controller based hands, grip and trigger input, hand animation states, interactable objects, world space UI, and locomotion systems.

The VR interaction result is therefore positive at the source-code and project-structure level. The project has the required components for VR interaction, but final runtime behavior must still be validated inside Unity Play Mode and on a headset.

### VR Player Interaction Results

| Feature | Evidence from Current Project | Result |
|---|---|---|
| VR player body control | `BNGPlayerController.cs` | Implemented in current branch |
| Camera rig and head alignment | `TrackingSpace`, `CameraRig`, `CenterEyeAnchor` fields in `BNGPlayerController` | Implemented in current branch |
| Player height adjustment | `ResizeCharacterHeightWithCamera` and capsule height update logic | Implemented in current branch |
| Ground checking | Physics raycast based ground distance calculation | Implemented in current branch |
| Player reset safety | Minimum and maximum elevation checks | Implemented in current branch |
| Unified controller input | `InputBridge.cs` | Implemented in current branch |
| Hand model animation | `HandController.cs` | Implemented in current branch |
| Object grabbing | `Grabber.cs` and `Grabbable.cs` | Implemented in current branch |
| World space UI interaction | `VRUISystem.cs`, `UIPointer.cs`, `VRCanvas.cs` | Implemented in current branch |

### Discussion

The VR interaction system is one of the strongest completed parts of the project. The use of `InputBridge` allows input values such as grip, trigger, thumbstick, and button states to be accessed through a common interface. This reduces dependency on one specific VR input provider and makes the project more adaptable to different VR runtimes.

The `HandController` script provides finger animation through grip, point, thumb, and pose values. This is important for VR because hand presence improves the sense of embodiment. The project also supports object specific hand poses through the `Grabbable` system, which means the hand can visually adapt when holding different items.

The object interaction system is also well structured. `Grabber` handles the hand side logic, while `Grabbable` stores the properties of interactable objects. This separation makes the system reusable because the same grabbing logic can be applied to weapons, props, arrows, tools, and other dungeon objects.

However, the result is not a completed gameplay evaluation. The code confirms that the systems exist and are integrated, but it does not prove comfort, usability, collision reliability, or player satisfaction. Those results require actual headset testing.

### VR Interaction Result Summary

| Parameter | Result |
|---|---|
| Source-code completeness | Strong foundation present |
| VR player setup | Present |
| Hand animation | Present |
| Object grabbing | Present |
| World space UI | Present |
| Runtime comfort testing | Not measured |
| Headset validation | Not measured |

## 4.2 Multiplayer Network Performance

The current project includes Photon Unity Networking and BNG PUN integration scripts. The available source code verifies that multiplayer room management, remote player instantiation, networked player synchronization, networked object synchronization, and ownership transfer are implemented at the script level.

No actual latency values, packet loss values, bandwidth data, or multi-client runtime measurements were found in the project files. Therefore, this section discusses implemented networking capability, not measured network performance.

### Networking Results in Current Branch

| Feature | Evidence | Result |
|---|---|---|
| Photon connection | `NetworkManager.cs` uses `PhotonNetwork.ConnectUsingSettings()` | Implemented |
| Room joining | `NetworkManager.cs` uses `PhotonNetwork.JoinRoom()` | Implemented |
| Room creation fallback | `OnJoinRoomFailed()` creates a room | Implemented |
| Remote player spawn | `OnJoinedRoom()` uses `PhotonNetwork.Instantiate()` | Implemented |
| Head and hand synchronization | `NetworkPlayer.cs` sends head and hand transforms | Implemented |
| Hand animation synchronization | `NetworkPlayer.cs` sends grip, point, thumb, pose ID, and holding state | Implemented |
| Object synchronization | `NetworkedGrabbable.cs` sends position, rotation, and held state | Implemented |
| Object ownership transfer | `RequestOwnership()` and ownership transfer logic | Implemented |
| Networked weapon events | `NetworkedRaycastWeapon.cs` uses RPC for firing feedback | Implemented |
| Networked bow visuals | `NetworkedBow.cs` sends arrow knock, bow rotation, draw percent, and string distance | Implemented |
| Networked arrow behavior | `NetworkedArrow.cs` uses RPC and stream synchronization | Implemented |

### Player Synchronization Discussion

`NetworkPlayer` uses `IPunObservable` to serialize player state. It sends head and hand positions, rotations, and animation values. On receiving clients, movement is smoothed using interpolation through `Vector3.Lerp`, `Quaternion.Lerp`, and `Mathf.Lerp`.

This is a suitable approach for VR multiplayer because head and hand motion changes frequently. Directly snapping remote hands to new network positions would look unstable. Interpolation helps reduce jitter and improves visual continuity.

The synchronization result is technically valid at the implementation level. However, the final quality depends on actual network update rate, latency, Photon room conditions, and how many objects are active in the scene. These values were not measured in the current environment.

### Networked Object Discussion

`NetworkedGrabbable` confirms that shared VR object interaction was considered. When the local client owns an object, it sends transform and held state. When the object is remote, it is made kinematic and interpolated to the received transform. This prevents two clients from simulating the same Rigidbody independently.

Ownership transfer is especially important in VR because players often hand objects to one another or grab objects that are currently controlled by another client. The project includes logic for requesting and transferring ownership, which is a necessary result for multiplayer interaction.

### Branch Verified Multiplayer Results

| Branch | Verified Multiplayer Work |
|---|---|
| `origin/non_URP` | Includes networked bow, networked arrow, networked grabbable changes, networked pistol prefab, networked marker prefab, and multiplayer demo updates |
| `origin/Multiplayer_native` | Includes networked spell caster and gesture based spell RPC synchronization |
| Current branch | Includes BNG PUN integration files for player, grabbable object, bow, arrow, marker, and raycast weapon synchronization |

### Multiplayer Result Summary

| Parameter | Result |
|---|---|
| Photon integration | Present |
| Room management | Present |
| Remote player representation | Present |
| Transform interpolation | Present |
| Hand animation synchronization | Present |
| Networked object ownership | Present |
| Networked weapon feedback | Present |
| Measured latency | Not measured |
| Measured bandwidth | Not measured |
| Multi-headset playtest | Not verified in this environment |

## 4.3 Frame Rate and Rendering Performance

The project contains URP and XR packages in the current `Packages/manifest.json`. The current project version is Unity 6000.3.13f1. The project also contains Oculus XR, OpenXR, XR Management, and URP dependencies, which are suitable foundations for VR rendering.

No profiler output, headset build report, frame time table, or FPS measurement file was found in the project folder. Therefore, the actual runtime frame rate cannot be honestly reported.

### Rendering Configuration Results

| Rendering Feature | Evidence | Result |
|---|---|---|
| Unity version | Unity 6000.3.13f1 in project version file | Verified |
| URP package | `com.unity.render-pipelines.universal` 17.3.0 | Present |
| XR Management | `com.unity.xr.management` 4.5.4 | Present |
| Oculus XR | `com.unity.xr.oculus` 4.5.4 | Present |
| OpenXR | `com.unity.xr.openxr` 1.16.1 | Present |
| World space UI | `VRUISystem`, `UIPointer`, `VRCanvas` | Present |
| Frame rate benchmark | No measured data found | Not available |
| GPU frame time | No measured data found | Not available |
| CPU frame time | No measured data found | Not available |

### Branch Verified Rendering Results

| Branch | Rendering Related Result |
|---|---|
| `origin/Multiplayer_native` | Contains URP default resources, including PC VR and Quest related URP assets |
| `origin/main` | Commit message indicates a return to Built In Render Pipeline |
| Current branch | Contains URP package dependency in `Packages/manifest.json` |

### Discussion

The presence of URP and XR packages shows that the project has a modern rendering foundation for VR. URP is generally suitable for VR projects because it provides scalable rendering options for desktop and standalone devices. However, package presence alone is not equivalent to performance validation.

The project includes several systems that support performance indirectly, such as network interpolation, kinematic handling of remote networked objects, object cleanup behavior, and UI raycast based interaction. These are good implementation decisions, but they do not provide numerical performance proof.

For a formal report, it is more accurate to write that the project is prepared for VR rendering and optimization, but final frame rate measurements remain pending. If later testing is performed, this section can be expanded with FPS, CPU frame time, GPU frame time, draw call count, and memory usage.

### Rendering and Performance Result Summary

| Parameter | Result |
|---|---|
| URP dependency | Present |
| XR rendering dependencies | Present |
| Quest and PC VR support assets in branch history | Present in branch review |
| Current measured FPS | Not measured |
| Current profiler result | Not available |
| Final performance conclusion | Cannot be claimed without runtime testing |

## 4.4 AI Behavior Evaluation

The AI result differs between the current branch and other branches. In the current branch, AI Navigation is installed as a package, and general damage and projectile systems exist. However, no current-branch C# script uses `UnityEngine.AI` or `NavMeshAgent`.

In `origin/main`, enemy AI scripts were found and inspected. These scripts include NavMesh based enemy movement, melee enemy behavior, ranged enemy behavior, enemy health drop handling, player health handling, and scene loading support. Since these files are present in another branch but not in the current branch, they are reported as branch verified AI results.

### Current Branch AI Results

| AI Related Item | Current Branch Result |
|---|---|
| Unity AI Navigation package | Present |
| `Damageable` system | Present |
| Projectile damage system | Present |
| Collision damage system | Present |
| Waypoint movement scripts | Present |
| Custom `NavMeshAgent` enemy controller | Not present in current branch |
| Enemy idle, patrol, chase, attack state machine | Not present in current branch |

### Branch Verified AI Results from `origin/main`

| Script | Verified Behavior |
|---|---|
| `EnemyAIBase.cs` | Uses `UnityEngine.AI`, requires `NavMeshAgent`, handles target tracking, region wandering, movement, animation parameters, damage events, death handling, and BNG `Damageable` integration |
| `EnemyMeleeAI.cs` | Extends `EnemyAIBase`, chases the player, stops within attack distance, faces the player, applies proximity based melee damage, and uses attack cooldown |
| `EnemyRangedAI.cs` | Extends `EnemyAIBase`, maintains preferred distance, retreats when too close, aims at the player, fires projectile prefab or raycast damage, and uses attack cooldown |
| `EnemyHealthDrops.cs` | Tracks enemy health through BNG `Damageable` and spawns drops on death |
| `PlayerHealth.cs` | Provides player health, damage, healing, reset, and death event support |
| `SceneLoader.cs` | Provides index and name based scene loading with validation and optional async loading |

### AI Behavior Discussion

The current branch alone should not be described as having a complete enemy AI implementation. It has the foundations required to support AI, including AI Navigation package installation, damage receiving, projectile damage, and movement support. However, the actual enemy controller scripts are found in `origin/main`, not the current branch.

The AI scripts in `origin/main` are technically meaningful. `EnemyAIBase` uses `NavMeshAgent` and can choose a wandering target inside a region. It can also chase the player when the player is within range. The melee enemy can attack based on distance and cooldown. The ranged enemy can maintain distance and attack with either projectile spawning or raycast damage.

This indicates that enemy AI was worked on in another branch and reached a source-code implementation stage. However, without opening that branch in Unity and testing the scenes, the report cannot claim that the AI is fully tuned, balanced, animated correctly, or free of runtime errors.

### AI Result Summary

| Parameter | Result |
|---|---|
| Current branch AI package support | Present |
| Current branch custom NavMesh enemy code | Not present |
| Other branch custom NavMesh enemy code | Present in `origin/main` |
| Melee enemy behavior | Present in `origin/main` |
| Ranged enemy behavior | Present in `origin/main` |
| Health drop behavior | Present in `origin/main` |
| Runtime AI validation | Not measured |
| Final AI gameplay quality | Cannot be claimed without scene testing |

## 4.5 User Experience Observations

The user experience results are based on implemented interaction systems and available project structure. No formal user study, questionnaire, or user comfort rating data was found in the repository. Therefore, this section reports design observations rather than measured user feedback.

### Observed User Experience Strengths

| Area | Observation |
|---|---|
| Hand presence | Hand animation values and object specific poses support stronger VR embodiment |
| Interaction clarity | Grabbable objects, grab points, and snap zones provide recognizable object interaction patterns |
| Comfort support | Teleportation, smooth locomotion, climbing, and screen fade systems are available |
| Multiplayer presence | Remote head and hand synchronization can represent other players in the scene |
| Combat feedback | Bow draw haptics, weapon sounds, projectile impacts, and damage events provide feedback systems |
| UI interaction | World space UI pointer and VR canvas support allow in-world menu or control panels |

### Observed User Experience Limitations

| Limitation | Explanation |
|---|---|
| No formal user test data | The repository does not contain survey results or user feedback tables |
| No headset comfort data | Motion sickness, comfort, or accessibility results were not measured |
| No final gameplay loop validation | Extraction, loot persistence, dungeon progression, and upgrade loop are not fully verified from current branch code |
| No final enemy balancing data | Enemy AI exists in another branch, but runtime balance is not verified |
| No final performance data | FPS and frame time data are not present |

### Discussion

The project is strongest as a VR systems prototype. It demonstrates a broad technical foundation for VR interaction, multiplayer synchronization, combat, and dungeon environment setup. From a user experience point of view, the most important positive result is that the project avoids relying only on flat screen style interaction. It uses VR specific ideas such as hand based grabbing, physical object manipulation, controller based locomotion, bow drawing, and world space UI.

The main weakness is that the repository does not provide formal testing data. For an academic report, this means the discussion should remain evidence based. It is acceptable to state that the implemented systems are expected to support immersive interaction, but it is not accurate to claim that users found the game comfortable or engaging unless user testing is later conducted.

## Consolidated Results Table

| System Area | Current Branch Result | Other Branch Result | Final Discussion |
|---|---|---|---|
| VR player movement | Present | Present through common BNG base | Strong source-code foundation |
| Hand animation | Present | Present through common BNG base | Strong source-code foundation |
| Physics grabbing | Present | Present through common BNG base | Strong source-code foundation |
| Multiplayer player sync | Present | Present in multiplayer branches | Implemented, but performance not measured |
| Networked grabbables | Present | Present in `origin/non_URP` and current branch | Implemented at source-code level |
| Networked bow and arrow | Present | Present in `origin/non_URP` | Implemented at source-code level |
| Gesture spell casting | Not present in current branch | Present in `origin/Multiplayer_native` | Branch verified, not current branch integrated |
| Enemy AI | Not present in current branch as custom NavMesh code | Present in `origin/main` | Branch verified, not current branch integrated |
| Dungeon environment | Present through PolygonDungeon | Additional dungeon map work in branches | Available as asset and scene foundation |
| Rendering setup | URP packages present | URP and Built In RP work exist in different branches | Rendering path changed during development |
| Runtime FPS | Not measured | Not measured | Cannot be claimed |
| User feedback | Not measured | Not measured | Cannot be claimed |

## Final Discussion

The results show that the project has reached a substantial technical implementation stage. The current branch contains a mature VR interaction foundation, including player control, hand animation, object grabbing, locomotion, UI interaction, combat components, and Photon based multiplayer synchronization. These results are supported by source-code evidence in the project folder.

The branch review adds important context. Work related to gesture based spell casting exists in `origin/Multiplayer_native`, and work related to enemy AI exists in `origin/main`. This means the overall repository contains more functionality than what is visible in the current branch alone. However, because these features are distributed across branches, they should be reported carefully. They are valid development results, but they are not all simultaneously integrated into the current working branch.

The most reliable conclusion is that the project successfully implements the technical foundations of a VR multiplayer dungeon extraction game. It supports VR movement, hand interaction, physics based grabbing, multiplayer synchronization, combat objects, and several advanced feature branches. The main remaining gap is final integration and validation. The project still needs merged feature consolidation, Unity play mode testing, VR headset testing, multiplayer latency testing, profiler benchmarking, and user evaluation before final gameplay quality can be claimed.

## Limitations of the Reported Results

| Limitation | Reason |
|---|---|
| No live Unity test was executed | This environment inspected files but did not run Unity Editor |
| No headset build was tested | The report cannot claim device level performance |
| No FPS data is available | No profiler capture or benchmark table was found |
| Some features are branch specific | Enemy AI and gesture spells are present in other branches, not necessarily merged into current branch |
| No user study data is present | User experience observations are design based, not survey based |
| No final extraction loop proof found | Complete loot extraction, upgrade persistence, and dungeon progression were not verified from current branch code |

## Recommended Result Additions After Testing

The following data should be added after actual Unity and headset testing:

| Test Area | Data to Record |
|---|---|
| VR interaction | Grab success rate, hand pose correctness, UI pointer usability |
| Locomotion comfort | Teleport comfort, smooth movement comfort, climbing comfort |
| Multiplayer | Number of clients tested, average latency, synchronization issues, ownership transfer success |
| Combat | Hit detection consistency, bow accuracy, projectile behavior, damage response |
| Enemy AI | Patrol, chase, melee attack, ranged attack, death, drop behavior |
| Performance | Average FPS, CPU frame time, GPU frame time, memory usage, draw calls |
| User feedback | Comfort rating, interaction rating, difficulty rating, suggestions |
