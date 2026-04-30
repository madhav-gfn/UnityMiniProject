using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Set the Rotation of a Transform based on a Bow's draw percentage
    /// </summary>
    public class BowArm : MonoBehaviour {

        public Bow BowItem;
#if PUN_2_OR_NEWER
        public NetworkedBow NetworkedBowItem;
#endif

        /// <summary>
        /// When to modify the rotation : 0-1;
        /// </summary>
        public float BowPercentStart = 50f;
        public float RotateDegrees = 10f; // How much past the initial rotation we should rotate

        public float Speed = 50f;

        private Quaternion _startRotation;
        private Quaternion _endRotation;

        public bool RotateX = true;
        public bool RotateZ = false;

        void Start() {
            _startRotation = Quaternion.Euler(transform.localEulerAngles);

            if(RotateX) {
                _endRotation = Quaternion.Euler(new Vector3(_startRotation.x + RotateDegrees, transform.localEulerAngles.y, transform.localEulerAngles.z));
            }

            if (RotateZ) {
                _endRotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + RotateDegrees));
            }
        }

        // Update is called once per frame
        void Update() {

            float drawPercent = BowItem != null ? BowItem.DrawPercent : 0;
#if PUN_2_OR_NEWER
            if (BowItem == null && NetworkedBowItem != null) {
                drawPercent = NetworkedBowItem.DrawPercent;
            }
#endif

            if (drawPercent >= BowPercentStart) {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _endRotation, Speed * Time.deltaTime);
            }
            else if(drawPercent < BowPercentStart && drawPercent > 5) {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _startRotation, Speed * Time.deltaTime);
            }
            else {
                transform.localRotation = _startRotation;
            }
        }
    }
}

