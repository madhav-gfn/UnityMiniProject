using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkedSpellCaster : MonoBehaviourPun, IPunObservable
{
    [Header("Networked Spell Prefabs")]
    [Tooltip("Must match the exact order: 0=Fire, 1=Ice, 2=Shield, 3=Lightning")]
    public GameObject[] networkedPrefabs;

    [Header("UI")]
    [Tooltip("Assign the Text component located on this player's prefab to show their gesture state")]
    public Text networkedDebugTextUI;

    private string syncedDebugText = "";

    void Awake()
    {
        // Hide the entire debug panel immediately before the first frame to prevent the 1-frame flash
        GameObject root = GetDebugPanelRoot();
        if (root != null) root.SetActive(false);
    }

    void Start()
    {
        // If this is OUR local network avatar, automatically find the gesture manager in the scene and connect to it!
        if (photonView.IsMine)
        {
            // Turn the entire debug panel back on because it belongs to us!
            GameObject root = GetDebugPanelRoot();
            if (root != null) root.SetActive(true);

            DirectionalGestureCaster caster = FindObjectOfType<DirectionalGestureCaster>();
            if (caster != null)
            {
                caster.OnSpellCastEvent.AddListener(BroadcastSpellCast);
                caster.OnDebugTextChanged.AddListener(UpdateLocalDebugText);
            }
        }
    }

    private GameObject GetDebugPanelRoot()
    {
        if (networkedDebugTextUI != null)
        {
            Canvas canvas = networkedDebugTextUI.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.transform.parent != null)
            {
                // Returns the GestureDebug object (parent of the Canvas), which holds both the Canvas and the Backsplash
                return canvas.transform.parent.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// This method should be called by the local DirectionalGestureCaster's OnSpellCastEvent.
    /// It sends an RPC to all OTHER players to spawn the spell visual on their screens.
    /// </summary>
    public void BroadcastSpellCast(int spellId, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Only the player who owns this networked object can broadcast spells from it
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_SpawnSpell", RpcTarget.Others, spellId, spawnPosition, spawnRotation);
        }
    }

    [PunRPC]
    private void RPC_SpawnSpell(int spellId, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Validate the spell ID
        if (networkedPrefabs == null || spellId < 0 || spellId >= networkedPrefabs.Length)
        {
            Debug.LogWarning($"[NetworkedSpellCaster] Received invalid spell ID: {spellId}");
            return;
        }

        GameObject prefab = networkedPrefabs[spellId];
        if (prefab != null)
        {
            // Spawn the spell visual locally for the remote player to see
            Instantiate(prefab, spawnPosition, spawnRotation);
        }
    }

    private void UpdateLocalDebugText(string text)
    {
        syncedDebugText = text;
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        if (networkedDebugTextUI != null)
        {
            networkedDebugTextUI.text = syncedDebugText;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send our local debug text to other players
            stream.SendNext(syncedDebugText);
        }
        else
        {
            // Receive remote player's debug text
            syncedDebugText = (string)stream.ReceiveNext();
            UpdateUIText();
        }
    }
}
