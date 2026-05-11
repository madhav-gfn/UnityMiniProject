DARKFORGE VR: A COOPERATIVE DUNGEON EXTRACTION GAME
________________________________________
1. INTRODUCTION
Virtual Reality gaming is one of the fastest evolving areas in interactive media, offering a level of immersion that traditional flat-screen games fundamentally cannot replicate. However, the full potential of VR — particularly around novel input methods, physical interaction mechanics, and cooperative social presence — remains largely underexplored in most commercial titles.
DarkForge VR is a cooperative-only multiplayer dungeon extraction game built ground-up for VR, inspired by the extraction RPG genre. Three players form a party, drop into a dangerous dungeon map, fight NPCs, gather loot, and attempt to extract alive. What they bring out can be used to upgrade gear and stats before the next run. The ultimate objective is to grow strong enough to face and defeat the dungeon’s final boss. The deeper a party ventures, the richer the rewards — but also the greater the danger.
The project is built with a strong emphasis on VR-native design: every mechanic, input method, and interaction is designed specifically around what is possible and meaningful in VR, rather than porting flat-screen conventions into a headset.
________________________________________
2. OBJECTIVE
•	To develop a co-op VR dungeon extraction game with a complete gameplay loop: enter → loot → fight → extract → upgrade → repeat.
•	To research and implement novel VR-specific input methodologies that go beyond standard button mapping — using physical gestures, motion, and spatial awareness as core gameplay inputs.
•	To explore new categories of gameplay mechanics that are only possible or meaningful in a VR context, such as physics-based combat, spatial inventory, and embodied co-op interactions.
•	To design a tiered dungeon progression system that gates access to deeper levels based on party strength and equipment score.
•	To build a co-op multiplayer experience using Steam’s networking infrastructure, enabling friends to play together across PC VR and Meta Quest 3.
•	To deliver a functional, playable product within a constrained development timeline and with limited team resources, prioritizing core VR innovation over scope.
________________________________________
3. PROBLEM STATEMENT
Most VR games today either adapt existing flat-screen game designs into VR with minimal rethinking of input and interaction, or limit themselves to simple experiences that don’t take full advantage of the medium. Deep, cooperative multiplayer games with extraction mechanics — a genre that is highly engaging on flat screens — have rarely been explored in a VR-native context.
Additionally, developing VR games presents compounded technical challenges: maintaining stable high frame rates (essential to prevent motion sickness), designing interactions that feel physically intuitive rather than menu-driven, and synchronizing multiplayer state across clients while keeping latency low enough for immersive play.
This project directly confronts these challenges — with the additional constraint of a small team, limited development time, and no large asset budget — by focusing on innovative VR mechanics and a tight, well-executed core loop rather than broad feature coverage.
________________________________________
4. PROPOSED SOLUTION
DarkForge VR is built in Unreal Engine using Blueprints as the primary development approach, with Steam Multiplayer (via the Advanced Sessions Plugin / Steam Online Subsystem) handling co-op networking. The game targets Meta Quest 3 (standalone) and PC VR platforms.
The proposed solution centers on three pillars:
Pillar 1 — VR-Native Input & Interaction Mechanics Rather than mapping traditional controls onto VR controllers, every interaction will be physically grounded. Weapon swings use actual tracked controller velocity and direction to determine hit impact. Spells may require specific hand gestures or two-handed motions. Looting is done by physically reaching and grabbing. Inventory is spatial — players wear their equipment or store it on their body. New VR interaction paradigms will be explored and implemented iteratively throughout development.
Pillar 2 — Extraction Gameplay Loop The core loop: Party of 3 drops into a dungeon map → fights NPC enemies with varied AI behaviors → collects loot drops → must extract through an exit point before being overwhelmed. Extracted loot persists and can be used in an upgrade/loadout screen between runs. Party strength gates access to deeper dungeon tiers.
Pillar 3 — Co-op Only Design All mechanics are designed around 3-player cooperation. Combat roles, loot distribution, flanking strategies, and boss encounters all reward coordinated teamwork. The game will not support solo or PvP modes — co-op is the singular design target.
________________________________________
5. TOOLS AND TECHNOLOGIES USED
Category	Tool / Technology
Game Engine	Unreal Engine 5
Scripting	Blueprints (Visual Scripting)
Multiplayer Networking	Steam Online Subsystem + Advanced Sessions Plugin
VR SDK	OpenXR / Meta XR Plugin
Target Platforms	Meta Quest 3 (standalone), PC VR (SteamVR)
3D Assets	Fab (UE Marketplace), Mixamo, Blender (minor edits)
Version Control	Git / GitHub
Project Tracking	Notion / Trello
IDE / Editor	Unreal Engine Editor, VS Code
________________________________________
6. BLOCK DIAGRAM

  ________________________________________
7. METHODOLOGY
a. Phase 1 — Engine Setup & VR Foundation Configure Unreal Engine 5 with OpenXR and the Meta XR plugin. Set up the Steam Online Subsystem and Advanced Sessions Plugin for multiplayer lobby creation and joining. Build a basic VR pawn with teleportation and smooth locomotion options and verify hand tracking and controller input pipelines.
b. Phase 2 — VR Input Research & Prototype Mechanics This is the core research phase. Prototype and evaluate novel VR input methods — physics-based melee using controller velocity, gesture-triggered abilities, spatial inventory worn on the body, two-handed object interactions. Identify which mechanics feel genuinely immersive and add to gameplay, and discard those that don’t. The findings will directly shape the game’s interaction design.
c. Phase 3 — Core Gameplay Loop Build the fundamental extraction loop: dungeon entry, NPC combat, loot pickup, and extraction. Keep the dungeon layout simple at this stage — the focus is on the loop feeling complete and fun, not visual polish.
d. Phase 4 — Multiplayer Integration Replicate player state (position, animation, health, held items) and enemy AI state across all three clients using Unreal’s built-in replication system over Steam. Ensure combat interactions (hits, deaths, loot drops) are server-authoritative to keep all clients consistent.
e. Phase 5 — Dungeon Progression & Upgrade System Implement tiered dungeon levels gated by party equipment score. Build the between-run upgrade and loadout screen. Tune loot drop tables per tier.
f. Phase 6 — NPC AI & Final Boss Develop enemy AI using Unreal’s Behavior Trees and NavMesh — basic patrol, aggro, and attack patterns for standard enemies. Design a multi-phase final boss with distinct attack phases requiring party coordination.
g. Phase 7 — Polish, Optimization & Testing Playtest all mechanics for comfort and fun. Optimize for 72+ FPS on Meta Quest 3 (standalone). Address motion sickness triggers. Fix multiplayer synchronization bugs. Final QA pass.
*phases are not in order of executions
8. EXPECTED OUTCOME
By the end of the project, the team expects to deliver:
•	A playable co-op VR dungeon extraction game supporting 3 players over Steam.
•	A complete, satisfying gameplay loop of dungeon entry, combat, looting, extraction, and upgrading.
•	A documented set of novel VR interaction mechanics and input methodologies prototyped and evaluated during development.
•	A tiered progression system with at least two dungeon difficulty levels and a final boss encounter.
•	A stable build targeting Meta Quest 3 and PC VR at comfortable VR frame rates.
•	Honest documentation of what was attempted, what worked, and what was dropped — reflective of the real constraints of a short-timeline academic project.
________________________________________
9. APPLICATION
•	VR Game Development Research: The project serves as a practical research effort into which new VR interaction paradigms enhance immersion and gameplay in a real game context.
•	Cooperative Social VR: Demonstrates how co-op multiplayer mechanics can be meaningfully reimagined for VR, with potential applications in social VR platform design.
•	Academic Demonstration: Showcases Unreal Engine 5, Blueprints, Steam networking, and XR development as applied skills in a complete working product.
•	Indie Game Prototype: The project could serve as a foundation or proof-of-concept for a broader indie game development effort post-project.
________________________________________
10. FUTURE SCOPE
•	Expanded Class System: Distinct VR-native character roles (e.g., a tank class that uses a physical shield held with both hands, a mage class driven entirely by gesture-based spellcasting).
•	Procedural Dungeon Generation: Fully procedural layouts for unlimited replayability.
•	PvPvE Mode: Allow two rival parties to encounter each other in the dungeon simultaneously — the true extraction RPG experience.
•	Hand Tracking (No Controller): Explore pure hand tracking on Meta Quest 3 as an input method for future iterations.
•	AI-Adaptive Enemies: Reinforcement learning-based enemy behavior that adapts to how the party plays over multiple runs.
•	Voice-Driven Mechanics: Spatial in-world voice chat with gameplay implications (e.g., loud players attract more enemies).
________________________________________
11. TEAM MEMBERS
1.	Rohan Choudhary (BT23CSH042)
2.	Madhav Mishra (BT23CSH047)
3.	Swapnil Kakade (BT23CSH013)
4.	Rajkamal Meena (BT23CSH032)
________________________________________
12. SUPERVISOR DETAIL
Dr. Jitendra V. Tembhurne
Assistant Professor
Department of Computer Science & Engineering
Indian Institute of Information Technology, Nagpur
________________________________________
13. REFERENCES
a.	Unreal Engine 5 Documentation — Epic Games [https://docs.unrealengine.com]
b.	Unreal Engine VR Development Guide [https://docs.unrealengine.com/5.0/en-US/developing-for-xr-experiences-in-unreal-engine/]
c.	Advanced Sessions Plugin for Steam Multiplayer [https://forums.unrealengine.com/t/advanced-sessions-plugin/30116]
d.	Steam Online Subsystem — Unreal Engine Docs [https://docs.unrealengine.com/4.27/en-US/ProgrammingAndScripting/Online/Steam/]
e.	Meta XR SDK for Unreal [https://developer.oculus.com/documentation/unreal/unreal-engine/]
f.	OpenXR Specification — Khronos Group [https://www.khronos.org/openxr/]
g.	Physics-Based VR Interaction Design — IEEE VR 2023 [https://ieeexplore.ieee.org/xpl/conhome/10108464/proceeding]
h.	Procedural Content Generation in Games — Shaker et al. [http://pcgbook.com]
i.	Presence and Immersion in Virtual Reality — Mel Slater, 2018 [https://doi.org/10.1093/acrefore/9780190236557.013.412]
j.	Dark and Darker — Ironmace, Game Design Reference [https://www.darkanddarker
