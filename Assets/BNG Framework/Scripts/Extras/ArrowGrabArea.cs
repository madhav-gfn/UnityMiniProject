using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Spawn an arrow if Trigger is grabbed
    /// </summary>
    public class ArrowGrabArea : MonoBehaviour {

        Bow theBow;
#if PUN_2_OR_NEWER
        NetworkedBow networkedBow;
#endif

        // Start is called before the first frame update
        void Start() {
            theBow = transform.parent.GetComponent<Bow>();
#if PUN_2_OR_NEWER
            networkedBow = transform.parent.GetComponent<NetworkedBow>();
#endif
        }

        void OnTriggerEnter(Collider other) {

            // Grabber entered grab area. We can potentially grab an arrow
            Grabber grabObject = other.GetComponent<Grabber>();
            if (grabObject != null) {

#if PUN_2_OR_NEWER
                if (networkedBow != null) {
                    networkedBow.ClosestGrabber = grabObject;

                    if (!grabObject.HoldingItem) {
                        networkedBow.CanGrabArrow = true;
                    }
                    else if (grabObject.HoldingItem && grabObject.HeldGrabbable != null) {
                        NetworkedArrow arrowObject = grabObject.HeldGrabbable.GetComponent<NetworkedArrow>();
                        if (arrowObject != null && networkedBow.GrabbedArrow == null) {
                            networkedBow.GrabArrow(arrowObject);
                        }
                    }

                    return;
                }
#endif

                if (theBow == null) {
                    return;
                }

                theBow.ClosestGrabber = grabObject;

                // Not holding anything. 
                if (!grabObject.HoldingItem) {
                    theBow.CanGrabArrow = true;                    
                }
                // Holding an arrow
                else if(grabObject.HoldingItem && grabObject.HeldGrabbable != null) {
                    // A held Arrow entered the grab area but has not yet been knocked
                    Arrow arrowObject = grabObject.HeldGrabbable.GetComponent<Arrow>();
                    if (arrowObject != null && theBow.GrabbedArrow == null) {
                        theBow.GrabArrow(arrowObject);
                    }
                }
            }
        }

        void OnTriggerExit(Collider other) {
            // Grabber exited grab area. No longer able to grab an arrow
            Grabber grabObject = other.GetComponent<Grabber>();
#if PUN_2_OR_NEWER
            if (networkedBow != null && networkedBow.ClosestGrabber != null && grabObject != null && networkedBow.ClosestGrabber == grabObject) {
                networkedBow.CanGrabArrow = false;
                networkedBow.ClosestGrabber = null;
                return;
            }
#endif
            if (theBow == null) {
                return;
            }

            if (theBow.ClosestGrabber != null &&  grabObject != null && theBow.ClosestGrabber == grabObject) {
                theBow.CanGrabArrow = false;
                theBow.ClosestGrabber = null;
            }
        }
    }
}

