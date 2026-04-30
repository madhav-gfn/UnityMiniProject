#if PUN_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace BNG {

    [RequireComponent(typeof(Grabbable))]
    [RequireComponent(typeof(PhotonView))]
    public class NetworkedBow : GrabbableEvents, IPunObservable {

        [Header("Bow Settings")]
        public float BowForce = 50f;
        public bool AlignBowToArrow = true;
        public Transform BowModel;

        [Header("Arrow Settings")]
        public Transform ArrowRest;
        public bool CanGrabArrowFromKnock = true;

        [Tooltip("Name of the prefab used to create an arrow. Must be in a /Resources/ directory.")]
        public string ArrowPrefabName = "Arrow2";

        public Transform ArrowRestLeftHanded;
        public Transform ArrowKnock;

        [Header("Arrow Positioning")]
        public bool IgnoreXPosition = false;
        public bool IgnoreYPosition = false;
        public bool AllowNegativeZ = true;

        [Header("Arrow Grabbing")]
        public bool CanGrabArrow = false;

        [HideInInspector]
        public Grabber ClosestGrabber;

        [HideInInspector]
        public NetworkedArrow GrabbedArrow;

        [HideInInspector]
        public Grabber arrowGrabber;

        [HideInInspector]
        public Vector3 LastValidPosition;

        [Header("String Settings")]
        public float MaxStringDistance = 0.3f;
        public float StringDistance = 0;

        public float DrawPercent { get; private set; } = 0;

        [Header("Debug Text")]
        public Text PercentageUI;

        public Vector3 BowUp = Vector3.forward;
        public float AlignBowSpeed = 20f;

        PhotonView photonView;
        Grabbable bowGrabbable;
        Grabbable arrowGrabbable;
        AudioSource audioSource;
        List<DrawDefinition> drawDefs;

        Vector3 initialKnockPosition;
        float lastDrawPercent;
        float lastDrawHaptic;
        float lastDrawHapticTime;
        bool playedDrawSound;
        bool holdingArrow;

        Vector3 syncedArrowKnockLocalPosition;
        Quaternion syncedBowModelLocalRotation = Quaternion.identity;
        float syncedDrawPercent;
        float syncedStringDistance;

        protected override void Awake() {
            base.Awake();
            photonView = GetComponent<PhotonView>();
        }

        void Reset() {
            CopySettingsFromLocalBow(false);
        }

        void OnValidate() {
            CopySettingsFromLocalBow(true);
        }

        void Start() {
            if (ArrowKnock == null || ArrowRest == null) {
                Debug.LogError("NetworkedBow requires ArrowKnock and ArrowRest references.", this);
                enabled = false;
                return;
            }

            initialKnockPosition = ArrowKnock.localPosition;
            syncedArrowKnockLocalPosition = initialKnockPosition;
            bowGrabbable = GetComponent<Grabbable>();
            audioSource = GetComponent<AudioSource>();

            drawDefs = new List<DrawDefinition>() {
                { new DrawDefinition() { DrawPercentage = 30f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 40f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 50f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 60f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 70f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 80f, HapticAmplitude = 0.1f, HapticFrequency = 0.1f } },
                { new DrawDefinition() { DrawPercentage = 90f, HapticAmplitude = 0.1f, HapticFrequency = 0.9f } },
                { new DrawDefinition() { DrawPercentage = 100f, HapticAmplitude = 0.1f, HapticFrequency = 1f } },
            };
        }

        void Update() {
            if (!CanControlBow()) {
                ApplyRemoteVisuals();
                return;
            }

            UpdateDrawDistance();
            CheckBowHaptics();

            if (!bowGrabbable.BeingHeld) {
                if (holdingArrow) {
                    ReleaseArrow();
                }

                ResetStringPosition();
                return;
            }

            holdingArrow = GrabbedArrow != null;

            if (CanGrabArrowFromKnockArea()) {
                NetworkedArrow arrow = SpawnArrow();
                if (arrow != null) {
                    Grabbable arrowGrab = arrow.GetComponent<Grabbable>();
                    if (arrowGrab) {
                        arrowGrab.GrabButton = GrabButton.Trigger;
                        arrowGrab.AddControllerVelocityOnDrop = false;
                    }

                    GrabArrow(arrow);
                }
            }

            if (GrabbedArrow == null) {
                ResetStringPosition();
            }

            StringDistance = arrowGrabber != null ? Vector3.Distance(transform.position, arrowGrabber.transform.position) : 0;

            if (holdingArrow) {
                SetKnockPosition();
                AlignArrow();
                CheckDrawSound();
                CheckBowHaptics();

                if (GetGrabArrowInput() <= 0.2f) {
                    ReleaseArrow();
                }
            }

            AlignBow();
        }

        NetworkedArrow SpawnArrow() {
            Vector3 spawnPosition = ArrowKnock.transform.position;
            Quaternion spawnRotation = Quaternion.identity;
            Transform arrowRest = GetArrowRest();

            if (arrowRest != null && arrowRest.position != spawnPosition) {
                spawnRotation = Quaternion.LookRotation(arrowRest.position - spawnPosition);
            }

            GameObject arrowObject = null;
            if (PhotonNetwork.InRoom) {
                arrowObject = PhotonNetwork.Instantiate(ArrowPrefabName, spawnPosition, spawnRotation, 0);
            }
            else {
                arrowObject = Instantiate(Resources.Load(ArrowPrefabName, typeof(GameObject)), spawnPosition, spawnRotation) as GameObject;
            }

            if (arrowObject == null) {
                Debug.LogWarning("Unable to spawn NetworkedArrow prefab named " + ArrowPrefabName);
                return null;
            }

            NetworkedArrow arrow = arrowObject.GetComponent<NetworkedArrow>();
            if (arrow == null) {
                Debug.LogWarning("Arrow prefab " + ArrowPrefabName + " needs a NetworkedArrow component.");
                return null;
            }

            Transform rest = GetArrowRest();
            if (rest != null) {
                arrow.transform.LookAt(rest);
            }

            return arrow;
        }

        void CopySettingsFromLocalBow(bool onlyMissingReferences) {
            Bow localBow = GetComponent<Bow>();
            if (localBow == null) {
                return;
            }

            BowForce = localBow.BowForce;
            AlignBowToArrow = localBow.AlignBowToArrow;
            CanGrabArrowFromKnock = localBow.CanGrabArrowFromKnock;
            ArrowPrefabName = localBow.ArrowPrefabName;
            IgnoreXPosition = localBow.IgnoreXPosition;
            IgnoreYPosition = localBow.IgnoreYPosition;
            AllowNegativeZ = localBow.AllowNegativeZ;
            MaxStringDistance = localBow.MaxStringDistance;
            BowUp = localBow.BowUp;
            AlignBowSpeed = localBow.AlignBowSpeed;

            if (!onlyMissingReferences || BowModel == null) {
                BowModel = localBow.BowModel;
            }
            if (!onlyMissingReferences || ArrowRest == null) {
                ArrowRest = localBow.ArrowRest;
            }
            if (!onlyMissingReferences || ArrowRestLeftHanded == null) {
                ArrowRestLeftHanded = localBow.ArrowRestLeftHanded;
            }
            if (!onlyMissingReferences || ArrowKnock == null) {
                ArrowKnock = localBow.ArrowKnock;
            }
            if (!onlyMissingReferences || PercentageUI == null) {
                PercentageUI = localBow.PercentageUI;
            }
        }

        Transform GetArrowRest() {
            if (bowGrabbable.GetPrimaryGrabber() != null && bowGrabbable.GetPrimaryGrabber().HandSide == ControllerHand.Right && ArrowRestLeftHanded != null) {
                return ArrowRestLeftHanded;
            }

            return ArrowRest;
        }

        bool CanGrabArrowFromKnockArea() {
            if (!CanGrabArrowFromKnock || ClosestGrabber == null || bowGrabbable.GetPrimaryGrabber() == null) {
                return false;
            }

            ControllerHand hand = bowGrabbable.GetControllerHand(bowGrabbable.GetPrimaryGrabber()) == ControllerHand.Left ? ControllerHand.Right : ControllerHand.Left;
            return CanGrabArrow && GetTriggerInput(hand) > 0.75f && !holdingArrow;
        }

        float GetGrabArrowInput() {
            if (arrowGrabber != null && arrowGrabbable != null) {
                GrabButton grabButton = arrowGrabber.GetGrabButton(arrowGrabbable);

                if (grabButton == GrabButton.Grip) {
                    return GetGripInput(arrowGrabber.HandSide);
                }
                else if (grabButton == GrabButton.Trigger) {
                    return GetTriggerInput(arrowGrabber.HandSide);
                }
            }

            return 0;
        }

        float GetGripInput(ControllerHand handSide) {
            if (handSide == ControllerHand.Left) {
                return input.LeftGrip;
            }
            else if (handSide == ControllerHand.Right) {
                return input.RightGrip;
            }

            return 0;
        }

        float GetTriggerInput(ControllerHand handSide) {
            if (handSide == ControllerHand.Left) {
                return input.LeftTrigger;
            }
            else if (handSide == ControllerHand.Right) {
                return input.RightTrigger;
            }

            return 0;
        }

        void SetKnockPosition() {
            if (StringDistance <= MaxStringDistance) {
                ArrowKnock.position = arrowGrabber.transform.position;
            }
            else {
                ArrowKnock.localPosition = initialKnockPosition;
                ArrowKnock.LookAt(arrowGrabber.transform, ArrowKnock.forward);
                ArrowKnock.position += ArrowKnock.forward * (MaxStringDistance * 0.65f);
            }

            if (IgnoreXPosition) {
                ArrowKnock.localPosition = new Vector3(GetArrowRest().localPosition.x, ArrowKnock.localPosition.y, ArrowKnock.localPosition.z);
            }
            if (IgnoreYPosition) {
                ArrowKnock.localPosition = new Vector3(ArrowKnock.localPosition.x, 0, ArrowKnock.localPosition.z);
            }
            if (!AllowNegativeZ && ArrowKnock.localPosition.z > initialKnockPosition.z) {
                ArrowKnock.localPosition = new Vector3(ArrowKnock.localPosition.x, ArrowKnock.localPosition.y, initialKnockPosition.z);
            }
        }

        void CheckDrawSound() {
            if (holdingArrow && !playedDrawSound && DrawPercent > 30f) {
                PlayBowDraw();
                playedDrawSound = true;

                if (CanBroadcast()) {
                    photonView.RPC(nameof(PlayBowDrawRPC), RpcTarget.Others);
                }
            }
        }

        void UpdateDrawDistance() {
            lastDrawPercent = DrawPercent;

            float knockDistance = Math.Abs(Vector3.Distance(ArrowKnock.localPosition, initialKnockPosition));
            DrawPercent = (knockDistance / MaxStringDistance) * 100;

            if (PercentageUI != null) {
                PercentageUI.text = (int)DrawPercent + "%";
            }
        }

        void CheckBowHaptics() {
            if (DrawPercent < lastDrawPercent || Time.time - lastDrawHapticTime < 0.11f || drawDefs == null) {
                return;
            }

            DrawDefinition definition = drawDefs.FirstOrDefault(x => x.DrawPercentage <= DrawPercent && x.DrawPercentage != lastDrawHaptic);
            if (definition != null && arrowGrabber != null) {
                input.VibrateController(definition.HapticFrequency, definition.HapticAmplitude, 0.1f, arrowGrabber.HandSide);
                lastDrawHaptic = definition.DrawPercentage;
                lastDrawHapticTime = Time.time;
            }
        }

        void ResetStringPosition() {
            ArrowKnock.localPosition = Vector3.Lerp(ArrowKnock.localPosition, initialKnockPosition, Time.deltaTime * 100);
        }

        void AlignArrow() {
            GrabbedArrow.transform.parent = transform;
            Rigidbody arrowRigid = GrabbedArrow.GetComponent<Rigidbody>();
            arrowRigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
            arrowRigid.isKinematic = true;

            GrabbedArrow.transform.position = ArrowKnock.transform.position;
            GrabbedArrow.transform.LookAt(GetArrowRest());
        }

        void AlignBow() {
            if (AlignBowToArrow == false || BowModel == null || grab == null || !grab.BeingHeld) {
                return;
            }

            if (holdingArrow) {
                if (GrabbedArrow != null) {
                    BowModel.transform.rotation = GrabbedArrow.transform.rotation;
                }
                else {
                    BowModel.transform.localRotation = Quaternion.Slerp(BowModel.transform.localRotation, Quaternion.identity, Time.deltaTime * AlignBowSpeed);
                }

                Vector3 eulers = BowModel.transform.localEulerAngles;
                eulers.z = 0;
                BowModel.transform.localEulerAngles = eulers;
            }
            else {
                BowModel.transform.localRotation = Quaternion.Slerp(BowModel.transform.localRotation, Quaternion.identity, Time.deltaTime * AlignBowSpeed);
            }
        }

        public void ResetBowAlignment() {
            if (BowModel != null) {
                BowModel.localEulerAngles = Vector3.zero;
            }
        }

        public void GrabArrow(NetworkedArrow arrow) {
            if (arrow == null || ClosestGrabber == null) {
                return;
            }

            PhotonView arrowView = arrow.GetComponent<PhotonView>();
            if (arrowView != null && PhotonNetwork.InRoom && !arrowView.IsMine) {
                arrowView.RequestOwnership();
            }

            arrowGrabber = ClosestGrabber;
            GrabbedArrow = arrow;

            if (GrabbedArrow.ShaftCollider) {
                GrabbedArrow.ShaftCollider.enabled = false;
            }

            arrowGrabbable = arrow.GetComponent<Grabbable>();
            if (arrowGrabbable) {
                arrowGrabbable.GrabItem(arrowGrabber);
                arrowGrabber.HeldGrabbable = arrowGrabbable;
                arrowGrabbable.AddControllerVelocityOnDrop = false;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Collider playerCollider = player != null ? player.GetComponentInChildren<Collider>() : null;
            if (playerCollider && GrabbedArrow.ShaftCollider) {
                Physics.IgnoreCollision(GrabbedArrow.ShaftCollider, playerCollider);
            }

            holdingArrow = true;
        }

        public void ReleaseArrow() {
            if (GrabbedArrow == null) {
                return;
            }

            PlayBowRelease();
            if (CanBroadcast()) {
                photonView.RPC(nameof(PlayBowReleaseRPC), RpcTarget.Others);
            }

            if (arrowGrabbable) {
                arrowGrabbable.GrabButton = GrabButton.Grip;
                arrowGrabbable.DropItem(false, true);
                arrowGrabbable.AddControllerVelocityOnDrop = true;
            }

            float shotForce = BowForce * StringDistance;
            GrabbedArrow.ShootArrow(GrabbedArrow.transform.forward * shotForce);

            if (arrowGrabber) {
                arrowGrabber.ResetHandGraphics();
            }

            ResetArrowValues();
        }

        public override void OnRelease() {
            ResetBowAlignment();
            ResetStringPosition();
        }

        void ResetArrowValues() {
            GrabbedArrow = null;
            arrowGrabbable = null;
            arrowGrabber = null;
            holdingArrow = false;
            playedDrawSound = false;
        }

        void ApplyRemoteVisuals() {
            if (ArrowKnock != null) {
                ArrowKnock.localPosition = Vector3.Lerp(ArrowKnock.localPosition, syncedArrowKnockLocalPosition, Time.deltaTime * 20f);
            }

            if (BowModel != null) {
                BowModel.localRotation = Quaternion.Slerp(BowModel.localRotation, syncedBowModelLocalRotation, Time.deltaTime * 20f);
            }

            DrawPercent = syncedDrawPercent;
            StringDistance = syncedStringDistance;

            if (PercentageUI != null) {
                PercentageUI.text = (int)DrawPercent + "%";
            }
        }

        void PlaySoundInterval(float fromSeconds, float toSeconds, float volume) {
            if (audioSource) {
                if (audioSource.isPlaying) {
                    audioSource.Stop();
                }

                audioSource.pitch = Time.timeScale;
                audioSource.time = fromSeconds;
                audioSource.volume = volume;
                audioSource.Play();
                audioSource.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
            }
        }

        void PlayBowDraw() {
            PlaySoundInterval(0, 1.66f, 0.4f);
        }

        void PlayBowRelease() {
            PlaySoundInterval(1.67f, 2.2f, 0.3f);
        }

        [PunRPC]
        void PlayBowDrawRPC() {
            if (!CanControlBow()) {
                PlayBowDraw();
            }
        }

        [PunRPC]
        void PlayBowReleaseRPC() {
            if (!CanControlBow()) {
                PlayBowRelease();
            }
        }

        bool CanControlBow() {
            return photonView == null || !PhotonNetwork.InRoom || photonView.IsMine;
        }

        bool CanBroadcast() {
            return photonView != null && PhotonNetwork.InRoom && photonView.IsMine;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting && CanControlBow()) {
                stream.SendNext(ArrowKnock != null ? ArrowKnock.localPosition : Vector3.zero);
                stream.SendNext(BowModel != null ? BowModel.localRotation : Quaternion.identity);
                stream.SendNext(DrawPercent);
                stream.SendNext(StringDistance);
            }
            else {
                syncedArrowKnockLocalPosition = (Vector3)stream.ReceiveNext();
                syncedBowModelLocalRotation = (Quaternion)stream.ReceiveNext();
                syncedDrawPercent = (float)stream.ReceiveNext();
                syncedStringDistance = (float)stream.ReceiveNext();
            }
        }
    }
}
#endif
