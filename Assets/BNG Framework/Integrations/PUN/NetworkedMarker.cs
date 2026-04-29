#if PUN_2_OR_NEWER
using Photon.Pun;
using UnityEngine;

namespace BNG {
    [RequireComponent(typeof(PhotonView))]
    public class NetworkedMarker : Marker {

        PhotonView photonView;

        protected override void Awake() {
            base.Awake();
            photonView = GetComponent<PhotonView>();
        }

        public override void OnGrab(Grabber grabber) {
            if (CanDrawLocally()) {
                base.OnGrab(grabber);
            }
        }

        public override void OnRelease() {
            if (CanDrawLocally()) {
                base.OnRelease();
            }
        }

        protected override void InitDraw(Vector3 position, Quaternion rotation, float lineWidth, Color lineColor) {
            bool isNewStroke = IsNewDraw;
            bool shouldDrawPoint = isNewStroke || Vector3.Distance(lastDrawPoint, position) > MinDrawDistance;

            if (CanBroadcast() && shouldDrawPoint) {
                if (isNewStroke) {
                    photonView.RPC(nameof(BeginStrokeRPC), RpcTarget.OthersBuffered, position, rotation, lineWidth);
                }
                else {
                    photonView.RPC(nameof(AddPointRPC), RpcTarget.OthersBuffered, position, rotation, lineWidth);
                }
            }

            base.InitDraw(position, rotation, lineWidth, lineColor);
        }

        bool CanDrawLocally() {
            return photonView == null || !PhotonNetwork.IsConnected || photonView.IsMine;
        }

        bool CanBroadcast() {
            return photonView != null && PhotonNetwork.IsConnected && photonView.IsMine;
        }

        [PunRPC]
        void BeginStrokeRPC(Vector3 position, Quaternion rotation, float lineWidth) {
            if (CanDrawLocally()) {
                return;
            }

            IsNewDraw = true;
            base.InitDraw(position, rotation, lineWidth, DrawColor);
        }

        [PunRPC]
        void AddPointRPC(Vector3 position, Quaternion rotation, float lineWidth) {
            if (CanDrawLocally()) {
                return;
            }

            base.InitDraw(position, rotation, lineWidth, DrawColor);
        }
    }
}
#endif
