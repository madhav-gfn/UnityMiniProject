using UnityEngine;
using UnityEngine.Events;
using BNG;

namespace QuestSystem {
    public class QuestEquipmentChecker : MonoBehaviour {
        [Header("Grabbers")]
        public Grabber leftGrabber;
        public Grabber rightGrabber;

        [Header("Target")]
        public Grabbable targetItem;
        public QuestWeaponId targetWeapon;

        [Header("State")]
        [SerializeField]
        private bool isEquipped;

        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        public BoolEvent onEquippedChanged;

        public bool IsEquipped => isEquipped;

        private void OnEnable() {
            RegisterGrabber(leftGrabber);
            RegisterGrabber(rightGrabber);
            RefreshState();
        }

        private void OnDisable() {
            UnregisterGrabber(leftGrabber);
            UnregisterGrabber(rightGrabber);
        }

        private void RegisterGrabber(Grabber grabber) {
            if (grabber == null) {
                return;
            }

            grabber.onAfterGrabEvent.AddListener(HandleGrabbed);
            grabber.onReleaseEvent.AddListener(HandleReleased);
        }

        private void UnregisterGrabber(Grabber grabber) {
            if (grabber == null) {
                return;
            }

            grabber.onAfterGrabEvent.RemoveListener(HandleGrabbed);
            grabber.onReleaseEvent.RemoveListener(HandleReleased);
        }

        private void HandleGrabbed(Grabbable grabbed) {
            RefreshState();
        }

        private void HandleReleased(Grabbable released) {
            RefreshState();
        }

        public void RefreshState() {
            bool equippedNow = IsGrabberHolding(leftGrabber) || IsGrabberHolding(rightGrabber);
            if (equippedNow == isEquipped) {
                return;
            }

            isEquipped = equippedNow;
            onEquippedChanged?.Invoke(isEquipped);
        }

        private bool IsGrabberHolding(Grabber grabber) {
            if (grabber == null || grabber.HeldGrabbable == null) {
                return false;
            }

            if (targetItem != null) {
                return grabber.HeldGrabbable == targetItem;
            }

            if (targetWeapon != null) {
                QuestWeaponId weaponId = grabber.HeldGrabbable.GetComponentInParent<QuestWeaponId>();
                return weaponId == targetWeapon;
            }

            return false;
        }
    }
}
