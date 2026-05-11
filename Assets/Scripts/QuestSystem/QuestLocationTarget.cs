using UnityEngine;

namespace QuestSystem {
    public class QuestLocationTarget : MonoBehaviour {
        [Header("Location")]
        public string displayName = "Location";
        public float radius = 1.5f;
        public float requiredStaySeconds = 0f;
    }
}
