#if PUN_2_OR_NEWER
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace BNG {

    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkedArrow : MonoBehaviourPun, IPunObservable {

        Rigidbody rb;
        Grabbable grab;
        PhotonView view;
        AudioSource impactSound;

        public bool Flying = false;
        public float ZVel = 0;

        public Collider ShaftCollider;
        public Projectile ProjectileObject;

        float flightTime = 0f;
        float destroyTime = 10f;
        float arrowDamage;
        Coroutine queueDestroy;

        Vector3 syncStartPosition;
        Vector3 syncEndPosition;
        Quaternion syncStartRotation = Quaternion.identity;
        Quaternion syncEndRotation = Quaternion.identity;
        bool syncFlying;
        bool syncColliderEnabled = true;
        float lastSynchronizationTime;
        float syncDelay;
        float syncTime;
        bool initialized;

        void Awake() {
            EnsureInitialized();
        }

        void EnsureInitialized() {
            if (initialized) {
                return;
            }

            view = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();
            impactSound = GetComponent<AudioSource>();
            ShaftCollider = ShaftCollider != null ? ShaftCollider : GetComponent<Collider>();
            grab = GetComponent<Grabbable>();

            if (ProjectileObject == null) {
                ProjectileObject = gameObject.AddComponent<Projectile>();
                ProjectileObject.Damage = 50;
                ProjectileObject.StickToObject = true;
                ProjectileObject.enabled = false;
            }

            arrowDamage = ProjectileObject.Damage;
            initialized = true;
        }

        void FixedUpdate() {
            EnsureInitialized();

            if (!CanControlArrow()) {
                rb.isKinematic = true;

                syncTime += Time.fixedDeltaTime;
                float syncValue = syncDelay > 0 ? syncTime / syncDelay : 1f;
                transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncValue);
                transform.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncValue);
                Flying = syncFlying;

                if (ShaftCollider) {
                    ShaftCollider.enabled = syncColliderEnabled;
                }

                return;
            }

            bool beingHeld = grab != null && grab.BeingHeld;

            if (!beingHeld && rb != null && rb.linearVelocity != Vector3.zero && Flying && ZVel > 0.02f) {
                rb.rotation = Quaternion.LookRotation(rb.linearVelocity);
            }

            ZVel = transform.InverseTransformDirection(rb.linearVelocity).z;

            if (Flying) {
                flightTime += Time.fixedDeltaTime;
            }

            if (queueDestroy != null && grab != null && grab.BeingHeld) {
                StopCoroutine(queueDestroy);
                queueDestroy = null;
            }
        }

        public void ShootArrow(Vector3 shotForce) {
            EnsureInitialized();

            if (view != null && PhotonNetwork.InRoom && !view.IsMine) {
                view.RequestOwnership();
            }

            if (CanBroadcast()) {
                view.RPC(nameof(ShootArrowRPC), RpcTarget.Others, transform.position, transform.rotation, shotForce);
            }

            flightTime = 0f;
            Flying = true;
            transform.parent = null;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(shotForce, ForceMode.VelocityChange);

            StartCoroutine(ReEnableCollider());
            queueDestroy = StartCoroutine(QueueDestroy());
        }

        [PunRPC]
        void ShootArrowRPC(Vector3 position, Quaternion rotation, Vector3 shotForce) {
            if (CanControlArrow()) {
                return;
            }

            EnsureInitialized();
            transform.parent = null;
            transform.position = position;
            transform.rotation = rotation;
            Flying = true;
            flightTime = 0f;

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (ShaftCollider) {
                ShaftCollider.enabled = false;
            }
        }

        IEnumerator QueueDestroy() {
            yield return new WaitForSeconds(destroyTime);

            if ((grab != null && grab.BeingHeld) || transform.parent != null) {
                yield break;
            }

            if (PhotonNetwork.InRoom && view != null) {
                if (view.IsMine) {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else {
                Destroy(gameObject);
            }
        }

        IEnumerator ReEnableCollider() {
            for (int x = 0; x < 3; x++) {
                yield return new WaitForFixedUpdate();
            }

            if (ShaftCollider) {
                ShaftCollider.enabled = true;
            }
        }

        void OnCollisionEnter(Collision collision) {
            EnsureInitialized();

            if (!CanControlArrow()) {
                return;
            }

            if (transform.parent != null && collision.transform == transform.parent) {
                return;
            }

            if (grab != null && grab.BeingHeld) {
                return;
            }

            if (collision.collider.isTrigger) {
                return;
            }

            string colNameLower = collision.transform.name.ToLower();

            if (flightTime < 1 && (colNameLower.Contains("arrow") || colNameLower.Contains("bow"))) {
                Physics.IgnoreCollision(collision.collider, ShaftCollider, true);
                return;
            }

            if (flightTime < 1 && collision.transform.name.ToLower().Contains("player")) {
                Physics.IgnoreCollision(collision.collider, ShaftCollider, true);
                return;
            }

            float zVel = System.Math.Abs(transform.InverseTransformDirection(rb.linearVelocity).z);
            bool doStick = true;

            if (zVel > 0.02f && !rb.isKinematic) {
                Damageable damageable = collision.collider.GetComponent<Damageable>();
                if (damageable == null) {
                    damageable = collision.collider.GetComponentInParent<Damageable>();
                }
                if (damageable) {
                    Vector3 hitPoint = collision.GetContact(0).point;
                    Vector3 hitNormal = collision.GetContact(0).normal;
                    damageable.DealDamage(arrowDamage, hitPoint, hitNormal, true, gameObject, collision.collider.gameObject);
                    BroadcastDamage(collision.collider, hitPoint, hitNormal);
                }

                if (damageable != null && damageable.Health <= 0) {
                    doStick = false;
                }
            }

            if (!rb.isKinematic && Flying && zVel > 0.02f) {
                if (grab != null && grab.BeingHeld) {
                    grab.DropItem(false, false);
                }

                if (doStick) {
                    TryStickArrow(collision);
                }

                Flying = false;
                BroadcastImpact(collision.collider);
                PlaySoundInterval(2.462f, 2.68f);
            }
        }

        void BroadcastDamage(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal) {
            if (!CanBroadcast()) {
                return;
            }

            PhotonView targetView = hitCollider.GetComponentInParent<PhotonView>();
            if (targetView != null) {
                view.RPC(nameof(DealDamageRPC), RpcTarget.Others, targetView.ViewID, arrowDamage, hitPoint, hitNormal);
            }
        }

        [PunRPC]
        void DealDamageRPC(int targetViewId, float damage, Vector3 hitPoint, Vector3 hitNormal) {
            PhotonView targetView = PhotonView.Find(targetViewId);
            if (targetView == null) {
                return;
            }

            Damageable damageable = targetView.GetComponentInChildren<Damageable>();
            if (damageable != null) {
                damageable.DealDamage(damage, hitPoint, hitNormal, true, gameObject, targetView.gameObject);
            }
        }

        void TryStickArrow(Collision collision) {
            Rigidbody colRigid = collision.collider.GetComponent<Rigidbody>();
            transform.parent = null;

            if (collision.gameObject.isStatic) {
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }
            else if (colRigid != null && !colRigid.isKinematic) {
                FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = colRigid;
                joint.enableCollision = false;
                joint.breakForce = float.MaxValue;
                joint.breakTorque = float.MaxValue;
            }
            else if (colRigid != null && colRigid.isKinematic && collision.transform.localScale == Vector3.one) {
                transform.SetParent(collision.transform);
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.WakeUp();
            }
            else {
                if (collision.transform.localScale == Vector3.one) {
                    transform.SetParent(collision.transform);
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                else {
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }
            }
        }

        void BroadcastImpact(Collider hitCollider) {
            if (!CanBroadcast()) {
                return;
            }

            PhotonView parentView = hitCollider.GetComponentInParent<PhotonView>();
            int parentViewId = parentView != null ? parentView.ViewID : 0;
            view.RPC(nameof(ImpactRPC), RpcTarget.Others, transform.position, transform.rotation, parentViewId);
        }

        [PunRPC]
        void ImpactRPC(Vector3 position, Quaternion rotation, int parentViewId) {
            if (CanControlArrow()) {
                return;
            }

            EnsureInitialized();
            Flying = false;
            transform.position = position;
            transform.rotation = rotation;

            PhotonView parentView = parentViewId != 0 ? PhotonView.Find(parentViewId) : null;
            transform.SetParent(parentView != null ? parentView.transform : null, true);

            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            if (ShaftCollider) {
                ShaftCollider.enabled = true;
            }

            PlaySoundInterval(2.462f, 2.68f);
        }

        void PlaySoundInterval(float fromSeconds, float toSeconds) {
            if (impactSound) {
                if (impactSound.isPlaying) {
                    impactSound.Stop();
                }

                impactSound.time = fromSeconds;
                impactSound.pitch = Time.timeScale;
                impactSound.Play();
                impactSound.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
            }
        }

        bool CanControlArrow() {
            return view == null || !PhotonNetwork.InRoom || view.IsMine;
        }

        bool CanBroadcast() {
            return view != null && PhotonNetwork.InRoom && view.IsMine;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            EnsureInitialized();

            if (stream.IsWriting && CanControlArrow()) {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(Flying);
                stream.SendNext(ShaftCollider == null || ShaftCollider.enabled);
            }
            else {
                syncStartPosition = transform.position;
                syncEndPosition = (Vector3)stream.ReceiveNext();
                syncStartRotation = transform.rotation;
                syncEndRotation = (Quaternion)stream.ReceiveNext();
                syncFlying = (bool)stream.ReceiveNext();
                syncColliderEnabled = (bool)stream.ReceiveNext();

                syncTime = 0f;
                syncDelay = Time.time - lastSynchronizationTime;
                lastSynchronizationTime = Time.time;
            }
        }
    }
}
#endif
