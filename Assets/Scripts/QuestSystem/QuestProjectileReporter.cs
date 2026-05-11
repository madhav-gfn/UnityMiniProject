using UnityEngine;
using BNG;

namespace QuestSystem {
    [RequireComponent(typeof(Projectile))]
    public class QuestProjectileReporter : MonoBehaviour {
        public QuestWeaponId weaponId;

        private Projectile projectile;

        private void Awake() {
            projectile = GetComponent<Projectile>();

            if (weaponId == null) {
                weaponId = GetComponentInParent<QuestWeaponId>();
            }
        }

        private void OnCollisionEnter(Collision collision) {
            if (weaponId == null || projectile == null) {
                return;
            }

            if (collision.collider.isTrigger) {
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
