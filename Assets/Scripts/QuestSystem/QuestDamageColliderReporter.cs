using UnityEngine;
using BNG;

namespace QuestSystem {
    [RequireComponent(typeof(DamageCollider))]
    public class QuestDamageColliderReporter : MonoBehaviour {
        public QuestWeaponId weaponId;

        private DamageCollider damageCollider;

        private void Awake() {
            damageCollider = GetComponent<DamageCollider>();

            if (weaponId == null) {
                weaponId = GetComponentInParent<QuestWeaponId>();
            }
        }

        private void OnCollisionEnter(Collision collision) {
            if (weaponId == null || damageCollider == null) {
                return;
            }

            if (collision.impulse.magnitude < damageCollider.MinForce) {
                return;
            }

            Damageable damageable = collision.collider.GetComponentInParent<Damageable>();
            if (damageable == null) {
                return;
            }

            QuestCombatTracker.RegisterHit(damageable, weaponId);
        }
    }
}
