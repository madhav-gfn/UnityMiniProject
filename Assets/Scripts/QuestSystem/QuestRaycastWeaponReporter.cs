using UnityEngine;
using BNG;

namespace QuestSystem {
    [RequireComponent(typeof(RaycastWeapon))]
    public class QuestRaycastWeaponReporter : MonoBehaviour {
        public QuestWeaponId weaponId;

        private RaycastWeapon raycastWeapon;

        private void Awake() {
            raycastWeapon = GetComponent<RaycastWeapon>();

            if (weaponId == null) {
                weaponId = GetComponent<QuestWeaponId>();
            }
        }

        private void OnEnable() {
            if (raycastWeapon != null) {
                raycastWeapon.onRaycastHitEvent.AddListener(HandleRaycastHit);
            }
        }

        private void OnDisable() {
            if (raycastWeapon != null) {
                raycastWeapon.onRaycastHitEvent.RemoveListener(HandleRaycastHit);
            }
        }

        private void HandleRaycastHit(RaycastHit hit) {
            if (weaponId == null) {
                return;
            }

            Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
            if (damageable == null) {
                return;
            }

            QuestCombatTracker.RegisterHit(damageable, weaponId);
        }
    }
}
