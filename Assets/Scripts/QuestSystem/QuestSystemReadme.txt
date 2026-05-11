Quest System Setup (high level)

1) Add QuestManager to a scene object.
2) Assign Left/Right Grabber references (BNG grabbers).
3) Create a quest and objectives in the inspector.
4) Add QuestWeaponId + reporter components to weapons:
   - RaycastWeapon: QuestWeaponId + QuestRaycastWeaponReporter
   - Melee (DamageCollider): QuestWeaponId + QuestDamageColliderReporter
   - Projectile prefab: QuestWeaponId + QuestProjectileReporter
5) Add QuestUIController to a UI panel and assign TMP_Text or Text.
